using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduBoost.Models
{
    public class MaterialCurso
    {
        [Key]
        public int IdMaterial { get; set; }

        public int IdCurso { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TipoMaterial { get; set; } = string.Empty;

        [StringLength(500)]
        public string? UrlVideo { get; set; }

        // Propiedad de navegación
        [ForeignKey("IdCurso")]
        public virtual Curso? Curso { get; set; }
    }
}
