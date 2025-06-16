using System.ComponentModel.DataAnnotations;
using GestionFinanciere.API.Models.Entities;
using GestionFinanciere.API.Helpers;

namespace GestionFinanciere.API.Models.DTOs
{
    /// <summary>
    /// DTO pour afficher les informations d'un compte
    /// </summary>
    public class CompteDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal SoldeInitial { get; set; }
        public decimal SoldeActuel { get; set; }
        public TypeCompte Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string TypeNom => Type.ToString();
        public int NombreTransactions { get; set; }
    }

    /// <summary>
    /// DTO pour créer un nouveau compte
    /// </summary>
    public class CreateCompteDto : IValidatableObject
    {
        [Required(ErrorMessage = "Le nom du compte est requis")]
        [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string Nom { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Le solde initial est requis")]
        public decimal SoldeInitial { get; set; }

        [Required(ErrorMessage = "Le type de compte est requis")]
        public TypeCompte Type { get; set; }

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

            // Validation du type d'énumération
            if (!Enum.IsDefined(typeof(TypeCompte), Type))
            {
                results.Add(new ValidationResult(
                    "Le type de compte n'est pas valide.",
                    new[] { nameof(Type) }));
            }

            return results;
        }
    }

    /// <summary>
    /// DTO pour mettre à jour un compte existant
    /// </summary>
    public class UpdateCompteDto : CreateCompteDto
    {
        // Hérite de toutes les propriétés et validations de CreateCompteDto
    }

    /// <summary>
    /// DTO pour le solde d'un compte
    /// </summary>
    public class CompteSoldeDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public TypeCompte Type { get; set; }
        public decimal SoldeInitial { get; set; }
        public decimal SoldeActuel { get; set; }
        public decimal TotalRevenus { get; set; }
        public decimal TotalDepenses { get; set; }
        public DateTime DerniereMiseAJour { get; set; }
    }

    /// <summary>
    /// DTO pour les paramètres de filtrage des comptes
    /// </summary>
    public class CompteFilterDto
    {
        public string? Nom { get; set; }
        public TypeCompte? Type { get; set; }
        public decimal? MinSolde { get; set; }
        public decimal? MaxSolde { get; set; }
        public string? SortBy { get; set; } = "Nom"; // Nom, Type, SoldeActuel
        public string? SortOrder { get; set; } = "asc"; // asc, desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// DTO pour la réponse paginée des comptes
    /// </summary>
    public class ComptePagedDto
    {
        public List<CompteDto> Comptes { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public decimal SoldeTotalActuel { get; set; }
    }
} 