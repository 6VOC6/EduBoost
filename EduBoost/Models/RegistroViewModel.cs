namespace EduBoost.Models
{
    using System.ComponentModel.DataAnnotations;

    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato valido.")]
        [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contrasena debe tener entre 8 y 100 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes confirmar la contrasena.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contrasenas no coinciden.")]
        public string ConfirmarPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes seleccionar un rol.")]
        [RegularExpression("^(Estudiante|Asesor)$", ErrorMessage = "Rol no valido.")]
        public string Rol { get; set; } = "Estudiante";
    }
}
