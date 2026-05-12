using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduBoost.Models
{
    public class Curso
    {
        [Key]
        public int IdCurso { get; set; }

        public int? IdUsuarioAsesor { get; set; }

        [Required(ErrorMessage = "El nombre del curso es obligatorio")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [StringLength(100)]
        public string? Profesor { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Propiedades de Navegación
        [ForeignKey("IdUsuarioAsesor")]
        public virtual Usuario? Asesor { get; set; }

        public virtual ICollection<MaterialCurso> Materiales { get; set; } = new List<MaterialCurso>();
    }
}
