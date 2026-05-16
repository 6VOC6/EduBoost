using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduBoost.Models
{
    public class Pregunta
    {
        [Key]
        public int IdPregunta { get; set; }

        public int IdEvaluacion { get; set; }

        [Required]
        public string Enunciado { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string OpcionA { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string OpcionB { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string OpcionC { get; set; } = string.Empty;

        [Required]
        [StringLength(1)]
        public string RespuestaCorrecta { get; set; } = "A"; // 'A', 'B' o 'C'

        public bool Activo { get; set; } = true;

        // Propiedad de navegación
        [ForeignKey("IdEvaluacion")]
        public virtual Evaluacion? Evaluacion { get; set; }
    }
}
