using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Accounts.Repository.DbContext
{
    public class AccountsContextFactory : IDesignTimeDbContextFactory<AccountsContext>
    {
        public AccountsContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(Path.Combine(basePath, "../Accounts.API/appsettings.json"))
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AccountsContext>();
            var connectionString = configuration.GetConnectionString("PostGresConnectionString");
            optionsBuilder.UseNpgsql(connectionString);

            return new AccountsContext(optionsBuilder.Options);
        }
    }
}