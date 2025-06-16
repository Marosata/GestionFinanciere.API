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
    public class ComptesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ComptesController> _logger;

        public ComptesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ComptesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Récupère la liste paginée des comptes de l'utilisateur avec filtres
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetComptes([FromQuery] CompteFilterDto filter)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Construction de la requête de base
                var query = _context.Comptes
                    .Include(c => c.Transactions)
                    .Where(c => c.UserId == userId);

                // Application des filtres
                if (!string.IsNullOrWhiteSpace(filter.Nom))
                    query = query.Where(c => c.Nom.Contains(filter.Nom));

                if (filter.Type.HasValue)
                    query = query.Where(c => c.Type == filter.Type.Value);

                // Tri
                query = filter.SortOrder?.ToLower() == "asc"
                    ? filter.SortBy?.ToLower() switch
                    {
                        "type" => query.OrderBy(c => c.Type),
                        "soldeactuel" => query.OrderBy(c => c.SoldeActuel),
                        _ => query.OrderBy(c => c.Nom)
                    }
                    : filter.SortBy?.ToLower() switch
                    {
                        "type" => query.OrderByDescending(c => c.Type),
                        "soldeactuel" => query.OrderByDescending(c => c.SoldeActuel),
                        _ => query.OrderByDescending(c => c.Nom)
                    };

                // Pagination
                var totalItems = await query.CountAsync();
                var items = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(c => new CompteDto
                    {
                        Id = c.Id,
                        Nom = c.Nom,
                        Description = c.Description,
                        SoldeInitial = c.SoldeInitial,
                        SoldeActuel = c.SoldeActuel,
                        Type = c.Type,
                        CreatedAt = c.CreatedAt,
                        UserId = c.UserId,
                        NombreTransactions = c.Transactions.Count
                    })
                    .ToListAsync();

                // Filtrage post-requête sur le solde (car c'est une propriété calculée)
                if (filter.MinSolde.HasValue)
                    items = items.Where(c => c.SoldeActuel >= filter.MinSolde.Value).ToList();

                if (filter.MaxSolde.HasValue)
                    items = items.Where(c => c.SoldeActuel <= filter.MaxSolde.Value).ToList();

                var soldeTotalActuel = items.Sum(c => c.SoldeActuel);

                var response = new ComptePagedDto
                {
                    Comptes = items,
                    TotalCount = items.Count,
                    PageNumber = filter.Page,
                    PageSize = filter.PageSize,
                    SoldeTotalActuel = soldeTotalActuel
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des comptes");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Récupère un compte spécifique par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompte(int id)
        {
            try
            {
                if (!ValidationHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de compte invalide." });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var compte = await _context.Comptes
                    .Include(c => c.Transactions)
                    .Where(c => c.Id == id && c.UserId == userId)
                    .Select(c => new CompteDto
                    {
                        Id = c.Id,
                        Nom = c.Nom,
                        Description = c.Description,
                        SoldeInitial = c.SoldeInitial,
                        SoldeActuel = c.SoldeActuel,
                        Type = c.Type,
                        CreatedAt = c.CreatedAt,
                        UserId = c.UserId,
                        NombreTransactions = c.Transactions.Count
                    })
                    .FirstOrDefaultAsync();

                if (compte == null)
                {
                    return NotFound(new { message = "Compte non trouvé." });
                }

                return Ok(compte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du compte {Id}", id);
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Crée un nouveau compte
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCompte([FromBody] CreateCompteDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Vérifier si un compte avec le même nom existe déjà
                var existingCompte = await _context.Comptes
                    .AnyAsync(c => c.Nom.ToLower() == model.Nom.ToLower() && c.UserId == userId);

                if (existingCompte)
                {
                    return BadRequest(new { message = "Un compte avec ce nom existe déjà." });
                }

                var compte = new Compte
                {
                    Nom = ValidationHelper.SanitizeString(model.Nom),
                    Description = string.IsNullOrEmpty(model.Description) ? null : ValidationHelper.SanitizeString(model.Description),
                    SoldeInitial = model.SoldeInitial,
                    Type = model.Type,
                    UserId = userId!,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Comptes.Add(compte);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Nouveau compte créé: {Id} par {UserId}", compte.Id, userId);

                var compteDto = new CompteDto
                {
                    Id = compte.Id,
                    Nom = compte.Nom,
                    Description = compte.Description,
                    SoldeInitial = compte.SoldeInitial,
                    SoldeActuel = compte.SoldeInitial, // Au début, le solde actuel est égal au solde initial
                    Type = compte.Type,
                    CreatedAt = compte.CreatedAt,
                    UserId = compte.UserId,
                    NombreTransactions = 0
                };

                return CreatedAtAction(nameof(GetCompte), new { id = compte.Id }, compteDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du compte");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Met à jour un compte existant
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompte(int id, [FromBody] UpdateCompteDto model)
        {
            try
            {
                if (!ValidationHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de compte invalide." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var compte = await _context.Comptes
                    .Include(c => c.Transactions)
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (compte == null)
                {
                    return NotFound(new { message = "Compte non trouvé." });
                }

                // Vérifier si le nouveau nom n'entre pas en conflit
                var conflictingCompte = await _context.Comptes
                    .AnyAsync(c => c.Id != id && c.Nom.ToLower() == model.Nom.ToLower() && c.UserId == userId);

                if (conflictingCompte)
                {
                    return BadRequest(new { message = "Un compte avec ce nom existe déjà." });
                }

                // Mettre à jour les propriétés
                compte.Nom = ValidationHelper.SanitizeString(model.Nom);
                compte.Description = string.IsNullOrEmpty(model.Description) ? null : ValidationHelper.SanitizeString(model.Description);
                compte.Type = model.Type;

                // Si le solde initial change, recalculer le solde actuel
                if (compte.SoldeInitial != model.SoldeInitial)
                {
                    compte.SoldeInitial = model.SoldeInitial;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Compte mis à jour: {Id} par {UserId}", id, userId);

                var compteDto = new CompteDto
                {
                    Id = compte.Id,
                    Nom = compte.Nom,
                    Description = compte.Description,
                    SoldeInitial = compte.SoldeInitial,
                    SoldeActuel = compte.SoldeActuel,
                    Type = compte.Type,
                    CreatedAt = compte.CreatedAt,
                    UserId = compte.UserId,
                    NombreTransactions = compte.Transactions.Count
                };

                return Ok(compteDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du compte {Id}", id);
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Supprime un compte
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompte(int id)
        {
            try
            {
                if (!ValidationHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de compte invalide." });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var compte = await _context.Comptes
                    .Include(c => c.Transactions)
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (compte == null)
                {
                    return NotFound(new { message = "Compte non trouvé." });
                }

                // Vérifier si le compte a des transactions
                if (compte.Transactions.Any())
                {
                    return BadRequest(new { 
                        message = "Impossible de supprimer ce compte car il contient des transactions." 
                    });
                }

                _context.Comptes.Remove(compte);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Compte supprimé: {Id} par {UserId}", id, userId);

                return Ok(new { message = "Compte supprimé avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du compte {Id}", id);
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Récupère le solde et les statistiques d'un compte
        /// </summary>
        [HttpGet("{id}/solde")]
        public async Task<IActionResult> GetCompteSolde(int id)
        {
            try
            {
                if (!ValidationHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de compte invalide." });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var compte = await _context.Comptes
                    .Include(c => c.Transactions)
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (compte == null)
                {
                    return NotFound(new { message = "Compte non trouvé." });
                }

                var totalRevenus = compte.Transactions
                    .Where(t => t.Type == TypeTransaction.Revenu)
                    .Sum(t => t.Montant);

                var totalDepenses = compte.Transactions
                    .Where(t => t.Type == TypeTransaction.Depense)
                    .Sum(t => t.Montant);

                var soldeDto = new CompteSoldeDto
                {
                    Id = compte.Id,
                    Nom = compte.Nom,
                    Type = compte.Type,
                    SoldeInitial = compte.SoldeInitial,
                    SoldeActuel = compte.SoldeActuel,
                    TotalRevenus = totalRevenus,
                    TotalDepenses = totalDepenses,
                    DerniereMiseAJour = compte.Transactions
                        .OrderByDescending(t => t.DateTransaction)
                        .Select(t => t.DateTransaction)
                        .FirstOrDefault()
                };

                return Ok(soldeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du solde du compte {Id}", id);
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }
    }
} 