using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduBoost.Models
{
    public class Asesoria
    {
        [Key]
        public int IdAsesoria { get; set; }

        [Required]
        public int IdEstudiante { get; set; }

        public int? IdAsesor { get; set; }

        [Required]
        public int IdCurso { get; set; }

        [Required]
        [Display(Name = "Fecha y Hora")]
        public DateTime FechaHora { get; set; }

        [Required]
        [StringLength(255)]
        public string Tema { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente";

        [StringLength(500)]
        [Display(Name = "Enlace de Reunión")]
        public string? EnlaceReunion { get; set; }

        // Navegación
        [ForeignKey("IdEstudiante")]
        public virtual Usuario? Estudiante { get; set; }

        [ForeignKey("IdAsesor")]
        public virtual Usuario? Asesor { get; set; }

        [ForeignKey("IdCurso")]
        public virtual Curso? Curso { get; set; }
    }
}
