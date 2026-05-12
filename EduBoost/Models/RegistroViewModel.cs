namespace EduBoost.Models
{
    using System.ComponentModel.DataAnnotations;

    public class RegistroViewModel
    {
        [Display(Name = "Nombre completo")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "La contraseña debe incluir mayúscula, minúscula y número.")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Confirmar contraseña")]
        [Required(ErrorMessage = "Debes confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } = string.Empty;

        [Display(Name = "Rol")]
        [Required(ErrorMessage = "Debes seleccionar un rol.")]
        [RegularExpression("^(Estudiante|Asesor)$", ErrorMessage = "Rol no válido.")]
        public string Rol { get; set; } = "Estudiante";
    }
}
