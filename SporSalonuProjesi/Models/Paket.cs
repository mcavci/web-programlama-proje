using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalonuProjesi.Models
{
    public class Paket
    {
        [Key]
        public int PaketId { get; set; }

        [Required]
        public string PaketAdi { get; set; } 
        [Required]
        [Range(1, 12)]
        public int SureAy { get; set; } 
        [Column(TypeName = "decimal(18,2)")]
        public decimal Fiyat { get; set; }
        [Required]
        public int HaftalikRandevuLimiti { get; set; }        
        public int ToplamAiHakki { get; set; }
        public bool SinirsizMi { get; set; } = false;
    }
}
    