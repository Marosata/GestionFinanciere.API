using System.Text.RegularExpressions;

namespace GestionFinanciere.API.Helpers
{
    public static class ValidationHelper
    {
        /// Valide un format de couleur hexadécimal
        public static bool IsValidHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            if (!color.StartsWith("#") || color.Length != 7)
                return false;

            return Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$");
        }

        /// Valide un montant (doit être positif)
        public static bool IsValidAmount(decimal amount)
        {
            return amount > 0;
        }

        /// Valide une description (longueur max et contenu)
        public static bool IsValidDescription(string? description, int maxLength = 500)
        {
            if (string.IsNullOrEmpty(description))
                return true; // Description optionnelle

            if (description.Length > maxLength)
                return false;

            // Vérifier qu'il n'y a pas de caractères dangereux
            return !ContainsDangerousCharacters(description);
        }

        /// Valide un nom (obligatoire, longueur max)
        public static bool IsValidName(string name, int maxLength = 100)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (name.Length > maxLength)
                return false;

            return !ContainsDangerousCharacters(name);
        }

        /// Valide un email
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

        /// Valide une date (ne doit pas être dans un futur trop lointain)
        public static bool IsValidTransactionDate(DateTime date)
        {
            // La date ne peut pas être dans plus de 24h dans le futur
            var maxFutureDate = DateTime.Now.AddDays(1);
            
            // La date ne peut pas être antérieure à 1900
            var minDate = new DateTime(2010, 1, 1);

            return date >= minDate && date <= maxFutureDate;
        }

        /// Vérifie la présence de caractères potentiellement dangereux
        private static bool ContainsDangerousCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Caractères HTML/SQL potentiellement dangereux
            string[] dangerousPatterns = { "<script", "</script", "javascript:", "vbscript:", "onload=", "onerror=", "SELECT ", "INSERT ", "UPDATE ", "DELETE " };
            
            var upperInput = input.ToUpperInvariant();
            return dangerousPatterns.Any(pattern => upperInput.Contains(pattern.ToUpperInvariant()));
        }

        /// Sanitise une chaîne de caractères
        public static string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Supprimer les caractères de contrôle et espaces en trop
            input = Regex.Replace(input, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");
            input = input.Trim();

            return input;
        }

        /// Valide un ID (doit être positif)
        public static bool IsValidId(int id)
        {
            return id > 0;
        }

        /// Valide un GUID d'utilisateur
        public static bool IsValidUserId(string userId)
        {
            return !string.IsNullOrWhiteSpace(userId);
        }
    }
} 