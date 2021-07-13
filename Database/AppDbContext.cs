using Microsoft.EntityFrameworkCore;
using Project_X.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Application> Applications { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .HasKey(company => company.Id);
            modelBuilder.Entity<Company>()
                .Property(company => company.Name)
                .IsRequired();
            modelBuilder.Entity<Company>()
                .Property(company => company.Address)
                .IsRequired();
            modelBuilder.Entity<Company>()
               .Property(company => company.PhotoFile)
               .IsRequired();
            modelBuilder.Entity<Company>()
               .Property(company => company.PhotoFileName)
               .IsRequired();

            modelBuilder.Entity<Offer>()
                .HasKey(offer => offer.Id);
            modelBuilder.Entity<Offer>()
                .Property(offer => offer.Category)
                .IsRequired();
            modelBuilder.Entity<Offer>()
                .Property(offer => offer.Title)
                .IsRequired();
            modelBuilder.Entity<Offer>()
                .Property(offer => offer.Type)
                .IsRequired();
            modelBuilder.Entity<Offer>()
                .Property(offer => offer.ExperienceLowerBound)
                .IsRequired();
            modelBuilder.Entity<Offer>()
                .HasOne<Company>(offer => offer.Company)
                .WithMany(company => company.Offers)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Candidate>()
                .HasKey(candidate => candidate.Id);
            modelBuilder.Entity<Candidate>()
                .Property(candidate => candidate.FirstName)
                .IsRequired();
            modelBuilder.Entity<Candidate>()
               .Property(candidate => candidate.LastName)
               .IsRequired();
            modelBuilder.Entity<Candidate>()
               .Property(candidate => candidate.ResumeFile)
               .IsRequired();
            modelBuilder.Entity<Candidate>()
               .Property(candidate => candidate.ResumeFileName)
               .IsRequired();
            modelBuilder.Entity<Candidate>()
               .Property(candidate => candidate.PhotoFile)
               .IsRequired();
            modelBuilder.Entity<Candidate>()
               .Property(candidate => candidate.PhotoFileName)
               .IsRequired();

            modelBuilder.Entity<Application>()
                .HasKey(application => new { application.CandidateId, application.OfferId });
            modelBuilder.Entity<Application>()
                .Property(application => application.Date)
                .IsRequired();
            modelBuilder.Entity<Application>()
               .Property(application => application.Status)
               .IsRequired();
            modelBuilder.Entity<Application>()
                .HasOne<Candidate>(application => application.Candidate)
                .WithMany(candidate => candidate.Applications)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Application>()
                .HasOne<Offer>(application => application.Offer)
                .WithMany(application => application.Applications)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
