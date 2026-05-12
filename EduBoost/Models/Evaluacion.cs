using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduBoost.Models
{
    public class Evaluacion
    {
        [Key]
        public int IdEvaluacion { get; set; }

        public int IdCurso { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Propiedades de navegación
        [ForeignKey("IdCurso")]
        public virtual Curso? Curso { get; set; }

        public virtual ICollection<Pregunta> Preguntas { get; set; } = new List<Pregunta>();
    }
}
