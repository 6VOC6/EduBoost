using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduBoost.Models
{
    public class Inscripcion
    {
        [Key]
        public int IdInscripcion { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdCurso { get; set; }

        public DateTime FechaInscripcion { get; set; }

        // Navegacion
        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        [ForeignKey("IdCurso")]
        public virtual Curso? Curso { get; set; }
    }
}
