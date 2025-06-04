using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionFinanciere.API.Models.Entities
{
    public class Compte
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nom { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SoldeInitial { get; set; }
        
        [Required]
        public TypeCompte Type { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Clé étrangère
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        
        // Propriété calculée pour le solde actuel
        [NotMapped]
        public decimal SoldeActuel => SoldeInitial + 
            Transactions.Where(t => t.Type == TypeTransaction.Revenu).Sum(t => t.Montant) -
            Transactions.Where(t => t.Type == TypeTransaction.Depense).Sum(t => t.Montant);
    }
    
    public enum TypeCompte
    {
        Courant = 0,
        Epargne = 1,
        Investissement = 2,
        Autre = 3
    }
}