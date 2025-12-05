using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Models;
namespace SporSalonuProjesi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Uye> Uyeler { get; set; } //  Tablo adı 'uyeler' olacak
        public DbSet<Randevu> Randevular { get; set; } // Tablo adı 'Randevular' olacak
        public DbSet<Paket> Paketler { get; set; } // Tablo adı 'Randevular' olacak    
        public DbSet<Egitmen> Egitmenler { get; set; } // Tablo adı 'Egitmenler' olacak    
        public DbSet<DersProgrami> Dersler { get; set; } // Tablo adı 'dersler' olacak   
        public DbSet<Admin> Adminler { get; set; } // Tablo adı 'adminler' olacak   

    }
}
