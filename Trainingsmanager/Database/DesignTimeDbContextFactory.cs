using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Trainingsmanager.Database
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<Context>
    {
        public Context CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<Context>();

            // Copy the same connection string used in Program.cs
            optionsBuilder.UseNpgsql("Host=ep-black-meadow-a9ynyveo-pooler.gwc.azure.neon.tech;Database=trainigmanager;Username=neondb_owner;Password=npg_eSu1Kg2mtoPR;SSL Mode=Require;Trust Server Certificate=true");

            return new Context(optionsBuilder.Options);
        }
    }
}