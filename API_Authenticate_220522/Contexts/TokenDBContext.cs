using API_Authenticate_220522.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Authenticate_220522.Contexts
{
    public class TokenDBContext : DbContext
    {
        public DbSet<Token> Tokens { get; set; }

        public TokenDBContext(DbContextOptions<TokenDBContext> options):base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var Tokens = modelBuilder.Entity<Token>();

            Tokens.Property(t => t.token).IsRequired();
            Tokens.HasKey(t => t.token);

            base.OnModelCreating(modelBuilder);
        }
    }
}
