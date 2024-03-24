using Api_Login.Models;
using Microsoft.EntityFrameworkCore;

namespace Api_Login.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
    }
}
