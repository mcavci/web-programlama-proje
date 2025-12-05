
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalonuProjesi.Models
{
    public class DersProgrami
    {
        [Key]
        public int Id { get; set; }

      
        [Required(ErrorMessage = "Lütfen bir gün seçiniz.")]
        [Display(Name = "Gün")]
        public string Gun { get; set; }
        
        [Required(ErrorMessage = "Başlangıç saati girilmeli.")]
        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)] 
        public TimeSpan BaslangicSaati { get; set; }
        
        [Required(ErrorMessage = "Bitiş saati girilmeli.")]
        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeSpan BitisSaati { get; set; }
      
        [Required(ErrorMessage = "Ders adı boş bırakılamaz.")]
        [Display(Name = "Ders Adı")]
        public string DersAdi { get; set; }

        [Display(Name = "Eğitmen")]
        public int EgitmenId { get; set; }

        [ForeignKey("EgitmenId")]
        public virtual Egitmen Egitmen { get; set; }
        public int Kontenjan { get; set; } =3; 
    }
}

