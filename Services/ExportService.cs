using System.Globalization;
using CsvHelper;
using GestionFinanciere.API.Models.DTOs;
using GestionFinanciere.API.Models.Entities;

namespace GestionFinanciere.API.Services
{
    public interface IExportService
    {
        byte[] ExporterTransactionsEnCsv(List<Transaction> transactions);
        byte[] ExporterRapportMensuelEnCsv(RapportMensuelDto rapport);
    }

    public class ExportService : IExportService
    {
        public byte[] ExporterTransactionsEnCsv(List<Transaction> transactions)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csv = new CsvWriter(writer, CultureInfo.GetCultureInfo("fr-FR"));

            var records = transactions.Select(t => new
            {
                Date = t.DateTransaction.ToString("dd/MM/yyyy"),
                Description = t.Description,
                Montant = t.Montant,
                Type = t.Type.ToString(),
                Categorie = t.Categorie?.Nom,
                Compte = t.Compte?.Nom
            });

            csv.WriteRecords(records);
            writer.Flush();
            return memoryStream.ToArray();
        }

        public byte[] ExporterRapportMensuelEnCsv(RapportMensuelDto rapport)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csv = new CsvWriter(writer, CultureInfo.GetCultureInfo("fr-FR"));

            // Écrire les informations générales
            csv.WriteField("Rapport Mensuel");
            csv.NextRecord();
            csv.WriteField($"Période: {rapport.Mois}/{rapport.Annee}");
            csv.NextRecord();
            csv.NextRecord();

            // Résumé financier
            csv.WriteField("Résumé Financier");
            csv.NextRecord();
            csv.WriteField("Total Dépenses"); csv.WriteField(rapport.TotalDepenses);
            csv.NextRecord();
            csv.WriteField("Total Revenus"); csv.WriteField(rapport.TotalRevenus);
            csv.NextRecord();
            csv.WriteField("Solde"); csv.WriteField(rapport.Solde);
            csv.NextRecord();
            csv.NextRecord();

            // Top Catégories
            csv.WriteField("Catégories");
            csv.NextRecord();
            csv.WriteField("Nom"); csv.WriteField("Type"); csv.WriteField("Montant"); csv.WriteField("Pourcentage");
            csv.NextRecord();
            foreach (var cat in rapport.TopCategories)
            {
                csv.WriteField(cat.Nom);
                csv.WriteField(cat.Type);
                csv.WriteField(cat.Montant);
                csv.WriteField($"{cat.Pourcentage:F2}%");
                csv.NextRecord();
            }

            writer.Flush();
            return memoryStream.ToArray();
        }
    }
} 