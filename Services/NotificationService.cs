using Microsoft.EntityFrameworkCore;
using GestionFinanciere.API.Data;
using GestionFinanciere.API.Models.Entities;
using System.Text.Json;

namespace GestionFinanciere.API.Services
{
    public interface INotificationService
    {
        Task<List<Notification>> ObtenirNotificationsUtilisateur(string userId, bool inclureNonLues = true);
        Task<Notification> CreerNotification(string userId, string titre, string message, TypeNotification type, object? contexte = null);
        Task MarquerCommeLue(int notificationId, string userId);
        Task SupprimerNotification(int notificationId, string userId);
        Task VerifierDepassementsBudget(string userId);
        Task VerifierSoldesComptes(string userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Notification>> ObtenirNotificationsUtilisateur(string userId, bool inclureNonLues = true)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (inclureNonLues)
                query = query.Where(n => !n.EstLue);

            return await query
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();
        }

        public async Task<Notification> CreerNotification(
            string userId, 
            string titre, 
            string message, 
            TypeNotification type, 
            object? contexte = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Titre = titre,
                Message = message,
                Type = type,
                DateCreation = DateTime.UtcNow,
                EstLue = false,
                DonneesContexte = contexte != null ? JsonSerializer.Serialize(contexte) : null
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task MarquerCommeLue(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.EstLue = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SupprimerNotification(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task VerifierDepassementsBudget(string userId)
        {
            var maintenant = DateTime.UtcNow;
            var debutMois = new DateTime(maintenant.Year, maintenant.Month, 1);
            var finMois = debutMois.AddMonths(1).AddDays(-1);

            var categories = await _context.Categories
                .Where(c => c.UserId == userId && c.BudgetMensuel > 0)
                .ToListAsync();

            foreach (var categorie in categories)
            {
                var totalDepenses = await _context.Transactions
                    .Where(t => t.CategorieId == categorie.Id &&
                           t.Type == TypeTransaction.Depense &&
                           t.DateTransaction >= debutMois &&
                           t.DateTransaction <= finMois)
                    .SumAsync(t => t.Montant);

                if (totalDepenses > categorie.BudgetMensuel)
                {
                    var depassement = totalDepenses - categorie.BudgetMensuel;
                    await CreerNotification(
                        userId,
                        "Dépassement de budget",
                        $"Le budget de la catégorie {categorie.Nom} a été dépassé de {depassement:C2}",
                        TypeNotification.DepassementBudget,
                        new { CategorieId = categorie.Id, Depassement = depassement }
                    );
                }
            }
        }

        public async Task VerifierSoldesComptes(string userId)
        {
            var comptes = await _context.Comptes
                .Where(c => c.UserId == userId && c.SeuilAlerte > 0)
                .ToListAsync();

            foreach (var compte in comptes)
            {
                var soldeActuel = await _context.Transactions
                    .Where(t => t.CompteId == compte.Id)
                    .SumAsync(t => t.Type == TypeTransaction.Revenu ? t.Montant : -t.Montant);

                soldeActuel += compte.SoldeInitial;

                if (soldeActuel <= compte.SeuilAlerte)
                {
                    await CreerNotification(
                        userId,
                        "Solde bas",
                        $"Le solde du compte {compte.Nom} est descendu à {soldeActuel:C2}, en dessous du seuil d'alerte de {compte.SeuilAlerte:C2}",
                        TypeNotification.SoldeBasCompte,
                        new { CompteId = compte.Id, Solde = soldeActuel }
                    );
                }
            }
        }
    }
} 