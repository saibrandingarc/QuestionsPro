using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using QuestionProConsole.Models;
using Microsoft.EntityFrameworkCore;

namespace QuestionProConsole.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for your entities
        public DbSet<QuestionsPro> TuSurveyQuestions { get; set; }
        public DbSet<QuestionsLookup> QuestionsLookups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<QuestionsPro>().ToTable("QuestionsPro");
            // Configure entity properties and relationships here
        }
    }

}