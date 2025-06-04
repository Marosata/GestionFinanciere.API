using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GestionFinanciere.API.Models.Entities;

namespace GestionFinanciere.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        // DbSets pour nos entités
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Categorie> Categories { get; set; }
        public DbSet<Compte> Comptes { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configuration des relations et contraintes
            ConfigureTransactions(builder);
            ConfigureCategories(builder);
            ConfigureComptes(builder);
            
            // Données de base (seed)
            SeedData(builder);
        }
        
        private void ConfigureTransactions(ModelBuilder builder)
        {
            builder.Entity<Transaction>(entity =>
            {
                // Configuration des décimaux avec précision
                entity.Property(e => e.Montant)
                    .HasPrecision(18, 2);
                
                // Index pour améliorer les performances des requêtes
                entity.HasIndex(e => e.DateTransaction);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Type);
                
                // Relations
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Categorie)
                    .WithMany(c => c.Transactions)
                    .HasForeignKey(e => e.CategorieId)
                    .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression d'une catégorie utilisée
                
                entity.HasOne(e => e.Compte)
                    .WithMany(c => c.Transactions)
                    .HasForeignKey(e => e.CompteId)
                    .OnDelete(DeleteBehavior.SetNull); // Si compte supprimé, met à null
            });
        }
        
        private void ConfigureCategories(ModelBuilder builder)
        {
            builder.Entity<Categorie>(entity =>
            {
                // Index pour les recherches
                entity.HasIndex(e => e.Nom);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.UserId);
                
                // Contrainte unique : un utilisateur ne peut pas avoir deux catégories avec le même nom et type
                entity.HasIndex(e => new { e.Nom, e.Type, e.UserId })
                    .IsUnique();
                
                // Relation optionnelle avec User (pour les catégories globales)
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Categories)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
        
        private void ConfigureComptes(ModelBuilder builder)
        {
            builder.Entity<Compte>(entity =>
            {
                // Configuration des décimaux
                entity.Property(e => e.SoldeInitial)
                    .HasPrecision(18, 2);
                
                // Index pour les recherches
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Type);
                
                // Contrainte unique : un utilisateur ne peut pas avoir deux comptes avec le même nom
                entity.HasIndex(e => new { e.Nom, e.UserId })
                    .IsUnique();
                
                // Relation avec User
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Comptes)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
        
        private void SeedData(ModelBuilder builder)
        {
            // Catégories globales par défaut
            var categoriesGlobales = new[]
            {
                new Categorie 
                { 
                    Id = 1, 
                    Nom = "Alimentation", 
                    Description = "Courses, restaurants, snacks", 
                    Type = TypeTransaction.Depense, 
                    Couleur = "#FF6B6B", 
                    IsGlobal = true 
                },
                new Categorie 
                { 
                    Id = 2, 
                    Nom = "Transport", 
                    Description = "Carburant, transports en commun, taxi", 
                    Type = TypeTransaction.Depense, 
                    Couleur = "#4ECDC4", 
                    IsGlobal = true 
                },
                new Categorie 
                { 
                    Id = 3, 
                    Nom = "Logement", 
                    Description = "Loyer, électricité, eau, internet", 
                    Type = TypeTransaction.Depense, 
                    Couleur = "#45B7D1", 
                    IsGlobal = true 
                },
                new Categorie 
                { 
                    Id = 4, 
                    Nom = "Loisirs", 
                    Description = "Cinéma, sports, sorties", 
                    Type = TypeTransaction.Depense, 
                    Couleur = "#F7DC6F", 
                    IsGlobal = true 
                },
                new Categorie 
                { 
                    Id = 5, 
                    Nom = "Santé", 
                    Description = "Médecin, pharmacie, assurance santé", 
                    Type = TypeTransaction.Depense, 
                    Couleur = "#BB8FCE", 
                    IsGlobal = true 
                },
                new Categorie 
                { 
                    Id = 6, 
                    Nom = "Salaire", 
                    Description = "Salaire mensuel", 
                    Type = TypeTransaction.Revenu, 
                    Couleur = "#58D68D", 
                    IsGlobal = true 
                },
                new Categorie 
                { 
                    Id = 7, 
                    Nom = "Freelance", 
                    Description = "Revenus de missions freelance", 
                    Type = TypeTransaction.Revenu, 
                    Couleur = "#52C41A", 
                    IsGlobal = true 
                },
                new Categorie 
                { 
                    Id = 8, 
                    Nom = "Investissements", 
                    Description = "Dividendes, plus-values", 
                    Type = TypeTransaction.Revenu, 
                    Couleur = "#1890FF", 
                    IsGlobal = true 
                }
            };
            
            builder.Entity<Categorie>().HasData(categoriesGlobales);
        }
    }
}