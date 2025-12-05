using System.ComponentModel.DataAnnotations;

namespace SporSalonuProjesi.Models
{
    public class Egitmen
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Ad soyad alanı zorunludur.")]
        [StringLength(50)]
        public string AdSoyad { get; set; }
        [Required]      
        public string Uzmanlik { get; set; }
        [Required]
        public string Aciklama { get; set; }
        [Required]
        public string FotoUrl { get; set; }
        [Required]
        public string Instagram { get; set; }
        public virtual ICollection<Uye> Uyeler { get; set; }
        public virtual ICollection<DersProgrami> DersProgramlari { get; set; }
    }
}
