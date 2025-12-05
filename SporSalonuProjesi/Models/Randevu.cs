using System.ComponentModel.DataAnnotations;

namespace SporSalonuProjesi.Models
{
    public class Randevu
    {
        [Key]
        public int Id { get; set; }
        public string UyeId { get; set; }
        [Display(Name = "Durum")]
        [StringLength(20)]
        public string Durum { get; set; }
        
        public int RandevuId { get; set; } //burda hocayı çayırma sayfalarda 

        [Display(Name = "Üye Adı Soyadı")]
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "İsim en az 3, en fazla 50 karakter olmalı.")]
        public string UyeAdSoyad { get; set; }
       
        [Display(Name = "Eğitmen Adı")]
        [Required(ErrorMessage = "Eğitmen seçimi yapılmadı.")]
        public string EgitmenAdi { get; set; }
        [Display(Name = "Randevu Tarihi")]
        [Required(ErrorMessage = "Lütfen bir tarih seçiniz!")]
        [DataType(DataType.Date)] 
        public DateTime Tarih { get; set; }

        [Display(Name = "Randevu Saati")]
        [Required(ErrorMessage = "Saat seçmelisiniz!")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Lütfen geçerli bir saat giriniz (Örn: 14:30)")]
        public string Saat { get; set; }
        public int EgitmenId { get; set; }
       
    }
}
