using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api;

public class DatabaseContext : DbContext
{
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<LoginInstance> LoginInstance { get; set; } = null!;
    public DbSet<CardCollection> CardCollections { get; set; } = null!;
    public DbSet<Card> Cards { get; set; } = null!;

    public DatabaseContext(DbContextOptions options) : base(options)
    {
    }
}