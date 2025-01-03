namespace MyApiApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public required DbSet<User> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }

    public class ContactDbContext : DbContext
    {
        public required DbSet<Contact> Contacts { get; set; }

        public ContactDbContext(DbContextOptions<ContactDbContext> options)
            : base(options)
        {
        }
    }

    public class HistoryDbContext : DbContext
    {
        public required DbSet<History> History { get; set; }

        public HistoryDbContext(DbContextOptions<HistoryDbContext> options)
            : base(options)
        {
        }
    }
}
