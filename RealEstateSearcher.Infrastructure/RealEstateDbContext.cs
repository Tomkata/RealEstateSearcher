using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RealEstateSearcher.Core.Models;
using System.Security.Cryptography.X509Certificates;
using Property = RealEstateSearcher.Core.Models.Property;

namespace RealEstateSearcher.Infrastructure
{
    public class RealEstateDbContext : DbContext
    {
        public RealEstateDbContext()
        {
        }

        public RealEstateDbContext(DbContextOptions<RealEstateDbContext> options)
            :base(options)
        {
        }

        public DbSet<Property> Properties { get; set; }

        public DbSet<Quarter>  Quarters { get; set; }

        public DbSet<BuildingType> BuildingTypes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Property>(entity =>
            {

                entity.HasOne(x => x.Quarter)
                  .WithMany(x => x.Properties)
                  .HasForeignKey(x => x.QuarterId)
                  .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.BuildingType)
                    .WithMany(x => x.Properties)
                    .HasForeignKey(x => x.BuildingTypeId)
                     .OnDelete(DeleteBehavior.SetNull)
                     .IsRequired(false);


            });
        }

    }
}
