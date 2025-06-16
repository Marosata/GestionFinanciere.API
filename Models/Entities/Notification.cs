namespace GestionFinanciere.API.Models.Entities
{
    public enum TypeNotification
    {
        DepassementBudget,
        SoldeBasCompte,
        RappelPaiement,
        NouvelleTransaction,
        RapportMensuel
    }

    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public TypeNotification Type { get; set; }
        public DateTime DateCreation { get; set; }
        public bool EstLue { get; set; }
        public string? DonneesContexte { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;
    }
} 