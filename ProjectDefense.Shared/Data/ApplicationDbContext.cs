using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Shared.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Sala> Sale { get; set; }
        public DbSet<DostepnoscProwadzacego> DostepnosciProwadzacych { get; set; }
        public DbSet<Rezerwacja> Rezerwacje { get; set; }
        public DbSet<BlokadaStudenta> BlokadyStudentow { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Konfiguracja relacji Rezerwacja - Student
            builder.Entity<Rezerwacja>()
                .HasOne(r => r.Student)
                .WithMany(u => u.Rezerwacje)
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Konfiguracja relacji Rezerwacja - DostepnoscProwadzacego
            builder.Entity<Rezerwacja>()
                .HasOne(r => r.DostepnoscProwadzacego)
                .WithMany(d => d.Rezerwacje)
                .HasForeignKey(r => r.DostepnoscProwadzacegoId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Konfiguracja relacji DostepnoscProwadzacego - Prowadzacy
            builder.Entity<DostepnoscProwadzacego>()
                .HasOne(d => d.Prowadzacy)
                .WithMany(u => u.Dostepnosci)
                .HasForeignKey(d => d.ProwadzacyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Konfiguracja relacji DostepnoscProwadzacego - Sala
            builder.Entity<DostepnoscProwadzacego>()
                .HasOne(d => d.Sala)
                .WithMany(s => s.Dostepnosci)
                .HasForeignKey(d => d.SalaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Konfiguracja relacji BlokadaStudenta
            builder.Entity<BlokadaStudenta>()
                .HasOne(b => b.Student)
                .WithMany()
                .HasForeignKey(b => b.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<BlokadaStudenta>()
                .HasOne(b => b.BlokowalProwadzacy)
                .WithMany()
                .HasForeignKey(b => b.BlokowalProwadzacyId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Indeksy dla lepszej wydajności
            builder.Entity<Rezerwacja>()
                .HasIndex(r => r.CzasRozpoczecia);
            
            builder.Entity<Rezerwacja>()
                .HasIndex(r => r.StudentId);
            
            builder.Entity<DostepnoscProwadzacego>()
                .HasIndex(d => new { d.SalaId, d.DataPoczatkowa, d.DataKoncowa });
        }
    }