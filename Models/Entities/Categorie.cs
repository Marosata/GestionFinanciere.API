using System.ComponentModel.DataAnnotations;

namespace GestionFinanciere.API.Models.Entities
{
    public class Categorie
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nom { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public TypeTransaction Type { get; set; }
        
        [Required]
        [MaxLength(7)] // Format #FFFFFF
        public string Couleur { get; set; } = "#000000";
        
        public bool IsGlobal { get; set; } = false; // Pour les catégories créées par l'admin
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Clé étrangère (null si catégorie globale)
        public string? UserId { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}