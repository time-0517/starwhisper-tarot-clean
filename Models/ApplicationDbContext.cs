using Microsoft.EntityFrameworkCore;

namespace 占卜.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TarotCard> TarotCards { get; set; }
        public DbSet<DrawSession> DrawSessions { get; set; }
        public DbSet<DrawCard> DrawCards { get; set; }

        public DbSet<User> Users { get; set; }
    }
}
