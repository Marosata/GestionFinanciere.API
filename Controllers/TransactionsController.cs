using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinanciere.API.Data;
using GestionFinanciere.API.Models.Entities;
using GestionFinanciere.API.Models.DTOs;
using GestionFinanciere.API.Helpers;
using System.Security.Claims;

namespace GestionFinanciere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<TransactionsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Récupère la liste paginée des transactions de l'utilisateur avec filtres
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] TransactionFilterDto filter)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Construction de la requête de base
                var query = _context.Transactions
                    .Include(t => t.Categorie)
                    .Include(t => t.Compte)
                    .Where(t => t.UserId == userId);

                // Application des filtres
                if (filter.DateDebut.HasValue)
                    query = query.Where(t => t.DateTransaction >= filter.DateDebut.Value);

                if (filter.DateFin.HasValue)
                    query = query.Where(t => t.DateTransaction <= filter.DateFin.Value);

                if (filter.CategorieId.HasValue)
                    query = query.Where(t => t.CategorieId == filter.CategorieId.Value);

                if (filter.MinMontant.HasValue)
                    query = query.Where(t => t.Montant >= filter.MinMontant.Value);

                if (filter.MaxMontant.HasValue)
                    query = query.Where(t => t.Montant <= filter.MaxMontant.Value);

                if (filter.Type.HasValue)
                    query = query.Where(t => t.Type == filter.Type.Value);

                // Tri
                query = filter.SortOrder?.ToLower() == "asc" 
                    ? filter.SortBy?.ToLower() switch
                    {
                        "montant" => query.OrderBy(t => t.Montant),
                        _ => query.OrderBy(t => t.DateTransaction)
                    }
                    : filter.SortBy?.ToLower() switch
                    {
                        "montant" => query.OrderByDescending(t => t.Montant),
                        _ => query.OrderByDescending(t => t.DateTransaction)
                    };

                // Calcul du total
                var totalMontant = await query.SumAsync(t => t.Type == TypeTransaction.Revenu ? t.Montant : -t.Montant);

                // Pagination
                var totalItems = await query.CountAsync();
                var items = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(t => new TransactionDto
                    {
                        Id = t.Id,
                        Montant = t.Montant,
                        Description = t.Description,
                        DateTransaction = t.DateTransaction,
                        Type = t.Type,
                        CreatedAt = t.CreatedAt,
                        UserId = t.UserId,
                        CategorieId = t.CategorieId,
                        CompteId = t.CompteId,
                        NomCategorie = t.Categorie.Nom,
                        NomCompte = t.Compte != null ? t.Compte.Nom : null
                    })
                    .ToListAsync();

                var response = new TransactionPagedDto
                {
                    Transactions = items,
                    TotalCount = totalItems,
                    PageNumber = filter.Page,
                    PageSize = filter.PageSize,
                    TotalMontant = totalMontant
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des transactions");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Récupère une transaction spécifique par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(int id)
        {
            try
            {
                if (!ValidationHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de transaction invalide." });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var transaction = await _context.Transactions
                    .Include(t => t.Categorie)
                    .Include(t => t.Compte)
                    .Where(t => t.Id == id && t.UserId == userId)
                    .Select(t => new TransactionDto
                    {
                        Id = t.Id,
                        Montant = t.Montant,
                        Description = t.Description,
                        DateTransaction = t.DateTransaction,
                        Type = t.Type,
                        CreatedAt = t.CreatedAt,
                        UserId = t.UserId,
                        CategorieId = t.CategorieId,
                        CompteId = t.CompteId,
                        NomCategorie = t.Categorie.Nom,
                        NomCompte = t.Compte != null ? t.Compte.Nom : null
                    })
                    .FirstOrDefaultAsync();

                if (transaction == null)
                {
                    return NotFound(new { message = "Transaction non trouvée." });
                }

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la transaction {Id}", id);
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Crée une nouvelle transaction
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Vérifier si la catégorie existe et appartient à l'utilisateur
                var categorie = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == model.CategorieId && (c.IsGlobal || c.UserId == userId));

                if (categorie == null)
                {
                    return BadRequest(new { message = "Catégorie invalide." });
                }

                // Vérifier si le compte existe et appartient à l'utilisateur
                if (model.CompteId.HasValue)
                {
                    var compte = await _context.Comptes
                        .FirstOrDefaultAsync(c => c.Id == model.CompteId && c.UserId == userId);

                    if (compte == null)
                    {
                        return BadRequest(new { message = "Compte invalide." });
                    }
                }

                var transaction = new Transaction
                {
                    Montant = model.Montant,
                    Description = ValidationHelper.SanitizeString(model.Description),
                    DateTransaction = model.DateTransaction,
                    Type = model.Type,
                    UserId = userId!,
                    CategorieId = model.CategorieId,
                    CompteId = model.CompteId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Nouvelle transaction créée: {Id} par {UserId}", transaction.Id, userId);

                // Charger les relations pour le DTO de retour
                await _context.Entry(transaction)
                    .Reference(t => t.Categorie)
                    .LoadAsync();

                if (transaction.CompteId.HasValue)
                {
                    await _context.Entry(transaction)
                        .Reference(t => t.Compte)
                        .LoadAsync();
                }

                var transactionDto = new TransactionDto
                {
                    Id = transaction.Id,
                    Montant = transaction.Montant,
                    Description = transaction.Description,
                    DateTransaction = transaction.DateTransaction,
                    Type = transaction.Type,
                    CreatedAt = transaction.CreatedAt,
                    UserId = transaction.UserId,
                    CategorieId = transaction.CategorieId,
                    CompteId = transaction.CompteId,
                    NomCategorie = transaction.Categorie.Nom,
                    NomCompte = transaction.Compte?.Nom
                };

                return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transactionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la transaction");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Met à jour une transaction existante
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] UpdateTransactionDto model)
        {
            try
            {
                if (!ValidationHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de transaction invalide." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transaction == null)
                {
                    return NotFound(new { message = "Transaction non trouvée." });
                }

                // Vérifier si la catégorie existe et appartient à l'utilisateur
                var categorie = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == model.CategorieId && (c.IsGlobal || c.UserId == userId));

                if (categorie == null)
                {
                    return BadRequest(new { message = "Catégorie invalide." });
                }

                // Vérifier si le compte existe et appartient à l'utilisateur
                if (model.CompteId.HasValue)
                {
                    var compte = await _context.Comptes
                        .FirstOrDefaultAsync(c => c.Id == model.CompteId && c.UserId == userId);

                    if (compte == null)
                    {
                        return BadRequest(new { message = "Compte invalide." });
                    }
                }

                // Mettre à jour les propriétés
                transaction.Montant = model.Montant;
                transaction.Description = ValidationHelper.SanitizeString(model.Description);
                transaction.DateTransaction = model.DateTransaction;
                transaction.Type = model.Type;
                transaction.CategorieId = model.CategorieId;
                transaction.CompteId = model.CompteId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Transaction mise à jour: {Id} par {UserId}", id, userId);

                // Charger les relations pour le DTO de retour
                await _context.Entry(transaction)
                    .Reference(t => t.Categorie)
                    .LoadAsync();

                if (transaction.CompteId.HasValue)
                {
                    await _context.Entry(transaction)
                        .Reference(t => t.Compte)
                        .LoadAsync();
                }

                var transactionDto = new TransactionDto
                {
                    Id = transaction.Id,
                    Montant = transaction.Montant,
                    Description = transaction.Description,
                    DateTransaction = transaction.DateTransaction,
                    Type = transaction.Type,
                    CreatedAt = transaction.CreatedAt,
                    UserId = transaction.UserId,
                    CategorieId = transaction.CategorieId,
                    CompteId = transaction.CompteId,
                    NomCategorie = transaction.Categorie.Nom,
                    NomCompte = transaction.Compte?.Nom
                };

                return Ok(transactionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la transaction {Id}", id);
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Supprime une transaction
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                if (!ValidationHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de transaction invalide." });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transaction == null)
                {
                    return NotFound(new { message = "Transaction non trouvée." });
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Transaction supprimée: {Id} par {UserId}", id, userId);

                return Ok(new { message = "Transaction supprimée avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la transaction {Id}", id);
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }
    }
} 