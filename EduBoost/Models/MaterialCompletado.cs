using System.ComponentModel.DataAnnotations;

namespace EduBoost.Models
{
    public class MaterialCompletado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdMaterial { get; set; }

        public DateTime FechaCompletado { get; set; } = DateTime.UtcNow;
    }
}
