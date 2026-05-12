using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduBoost.Models
{
    public class ResultadoEvaluacion
    {
        [Key]
        public int IdResultado { get; set; }

        public int IdUsuario { get; set; }

        public int IdEvaluacion { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Calificacion { get; set; }

        public DateTime FechaCompletado { get; set; } = DateTime.Now;

        // Propiedades de navegación
        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        [ForeignKey("IdEvaluacion")]
        public virtual Evaluacion? Evaluacion { get; set; }
    }
}
