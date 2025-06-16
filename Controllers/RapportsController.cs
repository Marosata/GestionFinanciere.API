using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinanciere.API.Data;
using GestionFinanciere.API.Models.Entities;
using GestionFinanciere.API.Models.DTOs;
using GestionFinanciere.API.Services;
using System.Security.Claims;

namespace GestionFinanciere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RapportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RapportsController> _logger;
        private readonly IExportService _exportService;

        public RapportsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<RapportsController> logger,
            IExportService exportService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _exportService = exportService;
        }

        /// <summary>
        /// Génère un rapport mensuel détaillé
        /// </summary>
        [HttpGet("mensuel")]
        public async Task<IActionResult> GetRapportMensuel([FromQuery] int annee, [FromQuery] int mois)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Valider les paramètres
                if (mois < 1 || mois > 12)
                    return BadRequest(new { message = "Mois invalide (1-12)." });

                if (annee < 2000 || annee > DateTime.Now.Year + 1)
                    return BadRequest(new { message = "Année invalide." });

                // Définir la période
                var debutMois = new DateTime(annee, mois, 1);
                var finMois = debutMois.AddMonths(1).AddDays(-1);
                var debutMoisPrecedent = debutMois.AddMonths(-1);
                var finMoisPrecedent = debutMois.AddDays(-1);

                // Récupérer toutes les transactions du mois
                var transactions = await _context.Transactions
                    .Include(t => t.Categorie)
                    .Include(t => t.Compte)
                    .Where(t => t.UserId == userId &&
                           t.DateTransaction >= debutMois &&
                           t.DateTransaction <= finMois)
                    .ToListAsync();

                // Récupérer les transactions du mois précédent pour comparaison
                var transactionsPrecedentes = await _context.Transactions
                    .Include(t => t.Categorie)
                    .Where(t => t.UserId == userId &&
                           t.DateTransaction >= debutMoisPrecedent &&
                           t.DateTransaction <= finMoisPrecedent)
                    .ToListAsync();

                // Calculer les totaux
                var totalDepenses = transactions
                    .Where(t => t.Type == TypeTransaction.Depense)
                    .Sum(t => t.Montant);

                var totalRevenus = transactions
                    .Where(t => t.Type == TypeTransaction.Revenu)
                    .Sum(t => t.Montant);

                // Calculer la moyenne journalière des dépenses
                var nombreJours = (finMois - debutMois).Days + 1;
                var moyenneJournaliere = totalDepenses / nombreJours;

                // Analyser les catégories
                var categoriesStats = transactions
                    .GroupBy(t => new { t.CategorieId, t.Categorie.Nom, t.Categorie.Couleur, t.Type })
                    .Select(g => new RapportCategorieDto
                    {
                        CategorieId = g.Key.CategorieId,
                        Nom = g.Key.Nom,
                        Couleur = g.Key.Couleur,
                        Type = g.Key.Type,
                        Montant = g.Sum(t => t.Montant),
                        NombreTransactions = g.Count(),
                        Pourcentage = g.Key.Type == TypeTransaction.Depense
                            ? totalDepenses > 0 ? (g.Sum(t => t.Montant) / totalDepenses) * 100 : 0
                            : totalRevenus > 0 ? (g.Sum(t => t.Montant) / totalRevenus) * 100 : 0
                    })
                    .OrderByDescending(c => c.Montant)
                    .ToList();

                // Analyser les comptes
                var comptesStats = await _context.Comptes
                    .Where(c => c.UserId == userId)
                    .Select(c => new RapportCompteDto
                    {
                        CompteId = c.Id,
                        Nom = c.Nom,
                        Type = c.Type,
                        SoldeDebut = c.SoldeInitial + c.Transactions
                            .Where(t => t.DateTransaction < debutMois)
                            .Sum(t => t.Type == TypeTransaction.Revenu ? t.Montant : -t.Montant),
                        SoldeFin = c.SoldeInitial + c.Transactions
                            .Where(t => t.DateTransaction <= finMois)
                            .Sum(t => t.Type == TypeTransaction.Revenu ? t.Montant : -t.Montant),
                        TotalEntrees = c.Transactions
                            .Where(t => t.DateTransaction >= debutMois && 
                                   t.DateTransaction <= finMois && 
                                   t.Type == TypeTransaction.Revenu)
                            .Sum(t => t.Montant),
                        TotalSorties = c.Transactions
                            .Where(t => t.DateTransaction >= debutMois && 
                                   t.DateTransaction <= finMois && 
                                   t.Type == TypeTransaction.Depense)
                            .Sum(t => t.Montant)
                    })
                    .ToListAsync();

                // Analyser les transactions par jour
                var transactionsParJour = transactions
                    .GroupBy(t => t.DateTransaction.Date)
                    .Select(g => new RapportJournalierDto
                    {
                        Date = g.Key,
                        TotalDepenses = g.Where(t => t.Type == TypeTransaction.Depense).Sum(t => t.Montant),
                        TotalRevenus = g.Where(t => t.Type == TypeTransaction.Revenu).Sum(t => t.Montant),
                        NombreTransactions = g.Count()
                    })
                    .OrderBy(j => j.Date)
                    .ToList();

                // Calculer les variations par rapport au mois précédent
                var totalDepensesPrecedentes = transactionsPrecedentes
                    .Where(t => t.Type == TypeTransaction.Depense)
                    .Sum(t => t.Montant);

                var totalRevenusPrecedents = transactionsPrecedentes
                    .Where(t => t.Type == TypeTransaction.Revenu)
                    .Sum(t => t.Montant);

                var variationsCategories = categoriesStats
                    .Select(c =>
                    {
                        var montantPrecedent = transactionsPrecedentes
                            .Where(t => t.CategorieId == c.CategorieId)
                            .Sum(t => t.Montant);

                        return new VariationCategorieDto
                        {
                            CategorieId = c.CategorieId,
                            Nom = c.Nom,
                            MontantMoisActuel = c.Montant,
                            MontantMoisPrecedent = montantPrecedent,
                            PourcentageVariation = montantPrecedent > 0
                                ? ((c.Montant - montantPrecedent) / montantPrecedent) * 100
                                : c.Montant > 0 ? 100 : 0
                        };
                    })
                    .OrderByDescending(v => Math.Abs(v.Variation))
                    .Take(5)
                    .ToList();

                var comparaison = new RapportComparaisonDto
                {
                    VariationDepenses = totalDepenses - totalDepensesPrecedentes,
                    VariationRevenus = totalRevenus - totalRevenusPrecedents,
                    PourcentageVariationDepenses = totalDepensesPrecedentes > 0
                        ? ((totalDepenses - totalDepensesPrecedentes) / totalDepensesPrecedentes) * 100
                        : totalDepenses > 0 ? 100 : 0,
                    PourcentageVariationRevenus = totalRevenusPrecedents > 0
                        ? ((totalRevenus - totalRevenusPrecedents) / totalRevenusPrecedents) * 100
                        : totalRevenus > 0 ? 100 : 0,
                    TopVariationsCategories = variationsCategories
                };

                // Construire le rapport final
                var rapport = new RapportMensuelDto
                {
                    Annee = annee,
                    Mois = mois,
                    TotalDepenses = totalDepenses,
                    TotalRevenus = totalRevenus,
                    MoyenneJournaliere = moyenneJournaliere,
                    TopCategories = categoriesStats,
                    Comptes = comptesStats,
                    TransactionsParJour = transactionsParJour,
                    ComparaisonMoisPrecedent = comparaison
                };

                return Ok(rapport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport mensuel");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Génère un rapport par catégorie sur une période donnée
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetRapportCategories(
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin,
            [FromQuery] TypeTransaction? type = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var query = _context.Transactions
                    .Include(t => t.Categorie)
                    .Where(t => t.UserId == userId &&
                           t.DateTransaction >= dateDebut &&
                           t.DateTransaction <= dateFin);

                if (type.HasValue)
                    query = query.Where(t => t.Type == type.Value);

                var transactions = await query.ToListAsync();

                var total = transactions.Sum(t => t.Montant);

                var categoriesStats = transactions
                    .GroupBy(t => new { t.CategorieId, t.Categorie.Nom, t.Categorie.Couleur, t.Type })
                    .Select(g => new RapportCategorieDto
                    {
                        CategorieId = g.Key.CategorieId,
                        Nom = g.Key.Nom,
                        Couleur = g.Key.Couleur,
                        Type = g.Key.Type,
                        Montant = g.Sum(t => t.Montant),
                        NombreTransactions = g.Count(),
                        Pourcentage = total > 0 ? (g.Sum(t => t.Montant) / total) * 100 : 0
                    })
                    .OrderByDescending(c => c.Montant)
                    .ToList();

                return Ok(categoriesStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport par catégories");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Exporte les transactions en CSV pour une période donnée
        /// </summary>
        [HttpGet("export/transactions")]
        public async Task<IActionResult> ExporterTransactions(
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var transactions = await _context.Transactions
                    .Include(t => t.Categorie)
                    .Include(t => t.Compte)
                    .Where(t => t.UserId == userId &&
                           t.DateTransaction >= dateDebut &&
                           t.DateTransaction <= dateFin)
                    .OrderBy(t => t.DateTransaction)
                    .ToListAsync();

                var csvBytes = _exportService.ExporterTransactionsEnCsv(transactions);
                var fileName = $"transactions_{dateDebut:yyyyMMdd}_{dateFin:yyyyMMdd}.csv";

                return File(csvBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'exportation des transactions");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Exporte le rapport mensuel en CSV
        /// </summary>
        [HttpGet("export/mensuel")]
        public async Task<IActionResult> ExporterRapportMensuel([FromQuery] int annee, [FromQuery] int mois)
        {
            try
            {
                var rapportResult = await GetRapportMensuel(annee, mois) as ObjectResult;
                if (rapportResult?.Value is not RapportMensuelDto rapport)
                {
                    return BadRequest(new { message = "Impossible de générer le rapport." });
                }

                var csvBytes = _exportService.ExporterRapportMensuelEnCsv(rapport);
                var fileName = $"rapport_mensuel_{annee}_{mois:00}.csv";

                return File(csvBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'exportation du rapport mensuel");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }
    }
} 