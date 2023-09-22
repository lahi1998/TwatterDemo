using Microsoft.EntityFrameworkCore;
using TwatterDemo.Models;

namespace TwatterDemo
{
    public class DBConnector : DbContext
    {
        // Tables we can access
        public DbSet<Twats> twats { get; set; }
        public DbSet<User> users { get; set; }

        public DBConnector(DbContextOptions<DBConnector> options)
            : base(options)
        {
        }

    }
}
