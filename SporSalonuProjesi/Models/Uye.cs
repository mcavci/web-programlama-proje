using System.ComponentModel.DataAnnotations;

namespace SporSalonuProjesi.Models
{
    public class Uye
    {
        [Key]
        public int UyeId { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad en az 2 karakter olmalı.")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad en az 2 karakter olmalı.")]
        public string Soyad { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalı.")]
        public string Sifre { get; set; }

        [Display(Name = "Telefon Numarası")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string Telefon { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)] 
        public DateTime DogumTarihi { get; set; }

        [Display(Name = "Boy (cm)")]
        [Range(100, 250, ErrorMessage = "Boy 100cm ile 250cm arasında olmalıdır.")]
        public double Boy { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(30, 300, ErrorMessage = "Kilo 30kg ile 300kg arasında olmalıdır.")]
        public double Kilo { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        [DataType(DataType.Date)]
        public DateTime KayitTarihi { get; set; } = DateTime.Now;

        public int? EgitmenId { get; set; }
        public virtual Egitmen Egitmen { get; set; }

        public int PaketId { get; set; }
        public virtual Paket Paket { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime PaketBitisTarihi { get; set; }     
        public int KalanAiHakki { get; set; }
    }
}
