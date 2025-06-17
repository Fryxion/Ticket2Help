using Microsoft.EntityFrameworkCore;
using Ticket2Help.BLL.Models;

namespace Ticket2Help.DAL.Database
{
    public class TicketDbContext : DbContext
    {
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<HardwareTicket> HardwareTickets { get; set; }
        public DbSet<SoftwareTicket> SoftwareTickets { get; set; }

        public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração do relacionamento entre Ticket e suas subclasses
            modelBuilder.Entity<HardwareTicket>().HasBaseType<Ticket>();
            modelBuilder.Entity<SoftwareTicket>().HasBaseType<Ticket>();
        }
    }
}