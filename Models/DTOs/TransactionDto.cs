using System.ComponentModel.DataAnnotations;
using GestionFinanciere.API.Models.Entities;
using GestionFinanciere.API.Helpers;

namespace GestionFinanciere.API.Models.DTOs
{
    /// <summary>
    /// DTO pour afficher les informations d'une transaction
    /// </summary>
    public class TransactionDto
    {
        public int Id { get; set; }
        public decimal Montant { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime DateTransaction { get; set; }
        public TypeTransaction Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int CategorieId { get; set; }
        public int? CompteId { get; set; }
        
        // Propriétés de navigation
        public string NomCategorie { get; set; } = string.Empty;
        public string? NomCompte { get; set; }
        public string TypeNom => Type == TypeTransaction.Depense ? "Dépense" : "Revenu";
    }

    /// <summary>
    /// DTO pour créer une nouvelle transaction
    /// </summary>
    public class CreateTransactionDto : IValidatableObject
    {
        [Required(ErrorMessage = "Le montant est requis")]
        public decimal Montant { get; set; }

        [Required(ErrorMessage = "La description est requise")]
        [MaxLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date de transaction est requise")]
        public DateTime DateTransaction { get; set; }

        [Required(ErrorMessage = "Le type de transaction est requis")]
        public TypeTransaction Type { get; set; }

        [Required(ErrorMessage = "La catégorie est requise")]
        public int CategorieId { get; set; }

        public int? CompteId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Validation du montant
            if (!ValidationHelper.IsValidAmount(Montant))
            {
                results.Add(new ValidationResult(
                    "Le montant doit être supérieur à 0.",
                    new[] { nameof(Montant) }));
            }

            // Validation de la description
            if (!ValidationHelper.IsValidDescription(Description))
            {
                results.Add(new ValidationResult(
                    "La description contient des caractères non autorisés ou est trop longue.",
                    new[] { nameof(Description) }));
            }

            // Validation de la date
            if (!ValidationHelper.IsValidTransactionDate(DateTransaction))
            {
                results.Add(new ValidationResult(
                    "La date de transaction n'est pas valide.",
                    new[] { nameof(DateTransaction) }));
            }

            return results;
        }
    }

    /// <summary>
    /// DTO pour mettre à jour une transaction existante
    /// </summary>
    public class UpdateTransactionDto : CreateTransactionDto
    {
        // Hérite de toutes les propriétés et validations de CreateTransactionDto
    }

    /// <summary>
    /// DTO pour les paramètres de filtrage des transactions
    /// </summary>
    public class TransactionFilterDto
    {
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public int? CategorieId { get; set; }
        public decimal? MinMontant { get; set; }
        public decimal? MaxMontant { get; set; }
        public TypeTransaction? Type { get; set; }
        public string? SortBy { get; set; } = "DateTransaction"; // DateTransaction, Montant
        public string? SortOrder { get; set; } = "desc"; // asc, desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// DTO pour la réponse paginée des transactions
    /// </summary>
    public class TransactionPagedDto
    {
        public List<TransactionDto> Transactions { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public decimal TotalMontant { get; set; }
    }
} 