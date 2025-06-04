using System.Text.RegularExpressions;

namespace GestionFinanciere.API.Helpers
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Valide un format de couleur hexadécimal
        /// </summary>
        public static bool IsValidHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            if (!color.StartsWith("#") || color.Length != 7)
                return false;

            return Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$");
        }

        /// <summary>
        /// Valide un montant (doit être positif)
        /// </summary>
        public static bool IsValidAmount(decimal amount)
        {
            return amount > 0;
        }

        /// <summary>
        /// Valide une description (longueur max et contenu)
        /// </summary>
        public static bool IsValidDescription(string? description, int maxLength = 500)
        {
            if (string.IsNullOrEmpty(description))
                return true; // Description optionnelle

            if (description.Length > maxLength)
                return false;

            // Vérifier qu'il n'y a pas de caractères dangereux
            return !ContainsDangerousCharacters(description);
        }

        /// <summary>
        /// Valide un nom (obligatoire, longueur max)
        /// </summary>
        public static bool IsValidName(string name, int maxLength = 100)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (name.Length > maxLength)
                return false;

            return !ContainsDangerousCharacters(name);
        }

        /// <summary>
        /// Valide un email
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valide une date (ne doit pas être dans un futur trop lointain)
        /// </summary>
        public static bool IsValidTransactionDate(DateTime date)
        {
            // La date ne peut pas être dans plus de 24h dans le futur
            var maxFutureDate = DateTime.Now.AddDays(1);
            
            // La date ne peut pas être antérieure à 1900
            var minDate = new DateTime(1900, 1, 1);

            return date >= minDate && date <= maxFutureDate;
        }

        /// <summary>
        /// Vérifie la présence de caractères potentiellement dangereux
        /// </summary>
        private static bool ContainsDangerousCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Caractères HTML/SQL potentiellement dangereux
            string[] dangerousPatterns = { "<script", "</script", "javascript:", "vbscript:", "onload=", "onerror=", "SELECT ", "INSERT ", "UPDATE ", "DELETE " };
            
            var upperInput = input.ToUpperInvariant();
            return dangerousPatterns.Any(pattern => upperInput.Contains(pattern.ToUpperInvariant()));
        }

        /// <summary>
        /// Sanitise une chaîne de caractères
        /// </summary>
        public static string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Supprimer les caractères de contrôle et espaces en trop
            input = Regex.Replace(input, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");
            input = input.Trim();

            return input;
        }

        /// <summary>
        /// Valide un ID (doit être positif)
        /// </summary>
        public static bool IsValidId(int id)
        {
            return id > 0;
        }

        /// <summary>
        /// Valide un GUID d'utilisateur
        /// </summary>
        public static bool IsValidUserId(string userId)
        {
            return !string.IsNullOrWhiteSpace(userId);
        }
    }
} 