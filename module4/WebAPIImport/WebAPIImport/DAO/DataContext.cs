using Microsoft.EntityFrameworkCore;
using WebAPIImport.DAO.Models;

namespace WebAPIImport.DAO
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RecordData>().ToTable("tb_record_data");
        }

        public DbSet<RecordData> RecordsData { get; set; }
    }
}
