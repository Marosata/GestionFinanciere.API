using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionFinanciere.API.Services;
using System.Security.Claims;

namespace GestionFinanciere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Obtient toutes les notifications de l'utilisateur
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenirNotifications([FromQuery] bool inclureNonLues = true)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var notifications = await _notificationService.ObtenirNotificationsUtilisateur(userId!, inclureNonLues);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des notifications");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Marque une notification comme lue
        /// </summary>
        [HttpPut("{id}/lue")]
        public async Task<IActionResult> MarquerCommeLue(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _notificationService.MarquerCommeLue(id, userId!);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du marquage de la notification comme lue");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Supprime une notification
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> SupprimerNotification(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _notificationService.SupprimerNotification(id, userId!);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la notification");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Vérifie manuellement les dépassements de budget
        /// </summary>
        [HttpPost("verifier-budgets")]
        public async Task<IActionResult> VerifierBudgets()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _notificationService.VerifierDepassementsBudget(userId!);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification des budgets");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }

        /// <summary>
        /// Vérifie manuellement les soldes des comptes
        /// </summary>
        [HttpPost("verifier-soldes")]
        public async Task<IActionResult> VerifierSoldes()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _notificationService.VerifierSoldesComptes(userId!);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification des soldes");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }
    }
} 