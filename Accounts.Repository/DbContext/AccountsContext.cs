using Accounts.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Accounts.Repository.DbContext
{
    public class AccountsContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public AccountsContext(DbContextOptions<AccountsContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}