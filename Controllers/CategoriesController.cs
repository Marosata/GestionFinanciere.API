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
    [Authorize] // Toutes les méthodes nécessitent une authentification
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CategoriesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Récupère toutes les catégories accessibles à l'utilisateur
        /// (catégories globales + catégories personnelles)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] TypeTransaction? type = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var query = _context.Categories
                    .Where(c => c.IsGlobal || c.UserId == userId);

                // Filtrer par type si spécifié
                if (type.HasValue)
                {
                    query = query.Where(c => c.Type == type.Value);
                }

                var categories = await query
                    .OrderBy(c => c.Nom)
                    .Select(c => new CategorieDto
                    {
                        Id = c.Id,
                        Nom = c.Nom,
                        Description = c.Description,
                        Type = c.Type,
                        Couleur = c.Couleur,
                        IsGlobal = c.IsGlobal,
                        CreatedAt = c.CreatedAt,
                        IsOwner = c.UserId == userId
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des catégories");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Récupère une catégorie spécifique par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategorie(int id)
        {
            try
            {
                if (!ValidationHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de catégorie invalide." });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var categorie = await _context.Categories
                    .Where(c => c.Id == id && (c.IsGlobal || c.UserId == userId))
                    .Select(c => new CategorieDto
                    {
                        Id = c.Id,
                        Nom = c.Nom,
                        Description = c.Description,
                        Type = c.Type,
                        Couleur = c.Couleur,
                        IsGlobal = c.IsGlobal,
                        CreatedAt = c.CreatedAt,
                        IsOwner = c.UserId == userId
                    })
                    .FirstOrDefaultAsync();

                if (categorie == null)
                {
                    return NotFound(new { message = "Catégorie non trouvée." });
                }

                return Ok(categorie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la catégorie {Id}", id);
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Crée une nouvelle catégorie personnelle
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCategorie([FromBody] CreateCategorieDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Vérifier si une catégorie avec le même nom et type existe déjà pour cet utilisateur
                var existingCategorie = await _context.Categories
                    .AnyAsync(c => c.Nom.ToLower() == model.Nom.ToLower() 
                              && c.Type == model.Type 
                              && c.UserId == userId);

                if (existingCategorie)
                {
                    return BadRequest(new { message = "Une catégorie avec ce nom et ce type existe déjà." });
                }

                var categorie = new Categorie
                {
                    Nom = ValidationHelper.SanitizeString(model.Nom),
                    Description = string.IsNullOrEmpty(model.Description) ? null : ValidationHelper.SanitizeString(model.Description),
                    Type = model.Type,
                    Couleur = model.Couleur,
                    IsGlobal = false, // Les utilisateurs ne peuvent créer que des catégories personnelles
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(categorie);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Nouvelle catégorie créée: {Nom} par {UserId}", categorie.Nom, userId);

                var categorieDto = new CategorieDto
                {
                    Id = categorie.Id,
                    Nom = categorie.Nom,
                    Description = categorie.Description,
                    Type = categorie.Type,
                    Couleur = categorie.Couleur,
                    IsGlobal = categorie.IsGlobal,
                    CreatedAt = categorie.CreatedAt,
                    IsOwner = true
                };

                return CreatedAtAction(nameof(GetCategorie), new { id = categorie.Id }, categorieDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la catégorie");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Met à jour une catégorie personnelle existante
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategorie(int id, [FromBody] UpdateCategorieDto model)
        {
            try
            {
                if (!ValidationHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de catégorie invalide." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var categorie = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsGlobal);

                if (categorie == null)
                {
                    return NotFound(new { message = "Catégorie non trouvée ou non modifiable." });
                }

                // Vérifier si le nouveau nom n'entre pas en conflit avec une autre catégorie
                var conflictingCategorie = await _context.Categories
                    .AnyAsync(c => c.Id != id 
                              && c.Nom.ToLower() == model.Nom.ToLower() 
                              && c.Type == model.Type 
                              && c.UserId == userId);

                if (conflictingCategorie)
                {
                    return BadRequest(new { message = "Une catégorie avec ce nom et ce type existe déjà." });
                }

                // Mettre à jour les propriétés
                categorie.Nom = ValidationHelper.SanitizeString(model.Nom);
                categorie.Description = string.IsNullOrEmpty(model.Description) ? null : ValidationHelper.SanitizeString(model.Description);
                categorie.Type = model.Type;
                categorie.Couleur = model.Couleur;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Catégorie mise à jour: {Id} par {UserId}", id, userId);

                var categorieDto = new CategorieDto
                {
                    Id = categorie.Id,
                    Nom = categorie.Nom,
                    Description = categorie.Description,
                    Type = categorie.Type,
                    Couleur = categorie.Couleur,
                    IsGlobal = categorie.IsGlobal,
                    CreatedAt = categorie.CreatedAt,
                    IsOwner = true
                };

                return Ok(categorieDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la catégorie {Id}", id);
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Supprime une catégorie personnelle
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategorie(int id)
        {
            try
            {
                if (!ValidationHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de catégorie invalide." });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var categorie = await _context.Categories
                    .Include(c => c.Transactions)
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsGlobal);

                if (categorie == null)
                {
                    return NotFound(new { message = "Catégorie non trouvée ou non supprimable." });
                }

                // Vérifier si la catégorie est utilisée dans des transactions
                if (categorie.Transactions.Any())
                {
                    return BadRequest(new { 
                        message = "Impossible de supprimer cette catégorie car elle est utilisée dans des transactions." 
                    });
                }

                _context.Categories.Remove(categorie);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Catégorie supprimée: {Id} par {UserId}", id, userId);

                return Ok(new { message = "Catégorie supprimée avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la catégorie {Id}", id);
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Récupère les statistiques d'utilisation des catégories
        /// </summary>
        [HttpGet("statistiques")]
        public async Task<IActionResult> GetCategoriesStatistiques([FromQuery] DateTime? dateDebut = null, [FromQuery] DateTime? dateFin = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Définir les dates par défaut (mois actuel)
                var debut = dateDebut ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var fin = dateFin ?? debut.AddMonths(1).AddDays(-1);

                var statistiques = await _context.Categories
                    .Where(c => c.IsGlobal || c.UserId == userId)
                    .Select(c => new CategorieStatistiqueDto
                    {
                        Id = c.Id,
                        Nom = c.Nom,
                        Type = c.Type,
                        Couleur = c.Couleur,
                        NombreTransactions = c.Transactions
                            .Count(t => t.UserId == userId && t.DateTransaction >= debut && t.DateTransaction <= fin),
                        MontantTotal = c.Transactions
                            .Where(t => t.UserId == userId && t.DateTransaction >= debut && t.DateTransaction <= fin)
                            .Sum(t => (decimal?)t.Montant) ?? 0
                    })
                    .Where(s => s.NombreTransactions > 0) // Afficher seulement les catégories utilisées
                    .OrderByDescending(s => s.MontantTotal)
                    .ToListAsync();

                return Ok(statistiques);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques des catégories");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }
    }
}