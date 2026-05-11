using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduBoost.Models
{
    public class Progreso
    {
        [Key]
        public int IdProgreso { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdCurso { get; set; }

        public int Porcentaje { get; set; } = 0;

        public DateTime? UltimaActualizacion { get; set; }

        // Navegacion
        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        [ForeignKey("IdCurso")]
        public virtual Curso? Curso { get; set; }
    }
}
