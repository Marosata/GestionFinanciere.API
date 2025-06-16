using GestionFinanciere.API.Models.Entities;

namespace GestionFinanciere.API.Models.DTOs
{
    /// <summary>
    /// DTO pour le rapport mensuel
    /// </summary>
    public class RapportMensuelDto
    {
        public int Annee { get; set; }
        public int Mois { get; set; }
        public decimal TotalDepenses { get; set; }
        public decimal TotalRevenus { get; set; }
        public decimal Solde => TotalRevenus - TotalDepenses;
        public decimal MoyenneJournaliere { get; set; }
        public List<RapportCategorieDto> TopCategories { get; set; } = new();
        public List<RapportCompteDto> Comptes { get; set; } = new();
        public List<RapportJournalierDto> TransactionsParJour { get; set; } = new();
        public RapportComparaisonDto ComparaisonMoisPrecedent { get; set; } = new();
    }

    /// <summary>
    /// DTO pour les statistiques par catégorie
    /// </summary>
    public class RapportCategorieDto
    {
        public int CategorieId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Couleur { get; set; } = string.Empty;
        public TypeTransaction Type { get; set; }
        public decimal Montant { get; set; }
        public decimal Pourcentage { get; set; }
        public int NombreTransactions { get; set; }
        public decimal MoyenneParTransaction => NombreTransactions > 0 
            ? Math.Round(Montant / NombreTransactions, 2) 
            : 0;
    }

    /// <summary>
    /// DTO pour les statistiques par compte
    /// </summary>
    public class RapportCompteDto
    {
        public int CompteId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public TypeCompte Type { get; set; }
        public decimal SoldeDebut { get; set; }
        public decimal SoldeFin { get; set; }
        public decimal Variation => SoldeFin - SoldeDebut;
        public decimal TotalEntrees { get; set; }
        public decimal TotalSorties { get; set; }
    }

    /// <summary>
    /// DTO pour les statistiques journalières
    /// </summary>
    public class RapportJournalierDto
    {
        public DateTime Date { get; set; }
        public decimal TotalDepenses { get; set; }
        public decimal TotalRevenus { get; set; }
        public decimal Solde => TotalRevenus - TotalDepenses;
        public int NombreTransactions { get; set; }
    }

    /// <summary>
    /// DTO pour la comparaison avec le mois précédent
    /// </summary>
    public class RapportComparaisonDto
    {
        public decimal VariationDepenses { get; set; }
        public decimal VariationRevenus { get; set; }
        public decimal PourcentageVariationDepenses { get; set; }
        public decimal PourcentageVariationRevenus { get; set; }
        public List<VariationCategorieDto> TopVariationsCategories { get; set; } = new();
    }

    /// <summary>
    /// DTO pour les variations par catégorie
    /// </summary>
    public class VariationCategorieDto
    {
        public int CategorieId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public decimal MontantMoisActuel { get; set; }
        public decimal MontantMoisPrecedent { get; set; }
        public decimal Variation => MontantMoisActuel - MontantMoisPrecedent;
        public decimal PourcentageVariation { get; set; }
    }
} 