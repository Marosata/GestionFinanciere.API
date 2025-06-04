using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionFinanciere.API.Models.Entities
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public DateTime DateTransaction { get; set; }
        
        [Required]
        public TypeTransaction Type { get; set; } // Enum: Depense ou Revenu
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Clés étrangères
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public int CategorieId { get; set; }
        
        public int? CompteId { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Categorie Categorie { get; set; } = null!;
        public virtual Compte? Compte { get; set; }
    }
    
    public enum TypeTransaction
    {
        Depense = 0,
        Revenu = 1
    }
}