using System.ComponentModel.DataAnnotations;

namespace SporSalonuProjesi.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; } 

        [Required]
        public string Sifre { get; set; }
    }
}