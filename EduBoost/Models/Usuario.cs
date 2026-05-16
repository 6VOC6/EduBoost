namespace EduBoost.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Usuario
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato valido.")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [StringLength(50)]
        public string Rol { get; set; } = "Estudiante";

        public DateTime FechaRegistro { get; set; }

        public bool Activo { get; set; } = true;

        [StringLength(100)]
        public string? TokenRecuperacion { get; set; }

        public DateTime? ExpiracionTokenRecuperacion { get; set; }
    }
}
