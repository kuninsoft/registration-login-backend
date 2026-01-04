using Microsoft.EntityFrameworkCore;
using RegistrationAndLoginApi.DataAccess.Entities;

namespace RegistrationAndLoginApi.DataAccess;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}