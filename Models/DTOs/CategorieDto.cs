using System.ComponentModel.DataAnnotations;
using GestionFinanciere.API.Models.Entities;
using GestionFinanciere.API.Helpers;

namespace GestionFinanciere.API.Models.DTOs
{
    /// <summary>
    /// DTO pour afficher les informations d'une catégorie
    /// </summary>
    public class CategorieDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TypeTransaction Type { get; set; }
        public string Couleur { get; set; } = string.Empty;
        public bool IsGlobal { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsOwner { get; set; } // Indique si l'utilisateur connecté est propriétaire
    }

    /// <summary>
    /// DTO pour créer une nouvelle catégorie
    /// </summary>
    public class CreateCategorieDto : IValidatableObject
    {
        [Required(ErrorMessage = "Le nom de la catégorie est requis")]
        [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string Nom { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Le type de catégorie est requis")]
        public TypeTransaction Type { get; set; }

        [Required(ErrorMessage = "La couleur est requise")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "La couleur doit être au format hexadécimal (#RRGGBB)")]
        public string Couleur { get; set; } = "#000000";

        /// <summary>
        /// Validation personnalisée
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Validation du nom
            if (!ValidationHelper.IsValidName(Nom))
            {
                results.Add(new ValidationResult(
                    "Le nom contient des caractères non autorisés ou est vide.",
                    new[] { nameof(Nom) }));
            }

            // Validation de la description
            if (!ValidationHelper.IsValidDescription(Description))
            {
                results.Add(new ValidationResult(
                    "La description contient des caractères non autorisés ou est trop longue.",
                    new[] { nameof(Description) }));
            }

            // Validation de la couleur
            if (!ValidationHelper.IsValidHexColor(Couleur))
            {
                results.Add(new ValidationResult(
                    "La couleur doit être au format hexadécimal valide (#RRGGBB).",
                    new[] { nameof(Couleur) }));
            }

            // Validation du type d'énumération
            if (!Enum.IsDefined(typeof(TypeTransaction), Type))
            {
                results.Add(new ValidationResult(
                    "Le type de transaction n'est pas valide.",
                    new[] { nameof(Type) }));
            }

            return results;
        }
    }

    /// <summary>
    /// DTO pour mettre à jour une catégorie existante
    /// </summary>
    public class UpdateCategorieDto : IValidatableObject
    {
        [Required(ErrorMessage = "Le nom de la catégorie est requis")]
        [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string Nom { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Le type de catégorie est requis")]
        public TypeTransaction Type { get; set; }

        [Required(ErrorMessage = "La couleur est requise")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "La couleur doit être au format hexadécimal (#RRGGBB)")]
        public string Couleur { get; set; } = "#000000";

        /// <summary>
        /// Validation personnalisée
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Validation du nom
            if (!ValidationHelper.IsValidName(Nom))
            {
                results.Add(new ValidationResult(
                    "Le nom contient des caractères non autorisés ou est vide.",
                    new[] { nameof(Nom) }));
            }

            // Validation de la description
            if (!ValidationHelper.IsValidDescription(Description))
            {
                results.Add(new ValidationResult(
                    "La description contient des caractères non autorisés ou est trop longue.",
                    new[] { nameof(Description) }));
            }

            // Validation de la couleur
            if (!ValidationHelper.IsValidHexColor(Couleur))
            {
                results.Add(new ValidationResult(
                    "La couleur doit être au format hexadécimal valide (#RRGGBB).",
                    new[] { nameof(Couleur) }));
            }

            // Validation du type d'énumération
            if (!Enum.IsDefined(typeof(TypeTransaction), Type))
            {
                results.Add(new ValidationResult(
                    "Le type de transaction n'est pas valide.",
                    new[] { nameof(Type) }));
            }

            return results;
        }
    }

    /// <summary>
    /// DTO pour les statistiques d'utilisation des catégories
    /// </summary>
    public class CategorieStatistiqueDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public TypeTransaction Type { get; set; }
        public string Couleur { get; set; } = string.Empty;
        public int NombreTransactions { get; set; }
        public decimal MontantTotal { get; set; }
        public string TypeNom => Type == TypeTransaction.Depense ? "Dépense" : "Revenu";
    }

    /// <summary>
    /// DTO pour les réponses avec liste paginée de catégories
    /// </summary>
    public class CategoriePagedDto
    {
        public List<CategorieDto> Categories { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// DTO pour les paramètres de recherche et filtrage des catégories
    /// </summary>
    public class CategorieFilterDto
    {
        public string? Nom { get; set; }
        public TypeTransaction? Type { get; set; }
        public bool? IsGlobal { get; set; }
        public string? SortBy { get; set; } = "Nom"; // Nom, Type, CreatedAt
        public string? SortOrder { get; set; } = "asc"; // asc, desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}