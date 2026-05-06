namespace EduBoost.Models
{
    using System.ComponentModel.DataAnnotations;

    public class LoginViewModel
    {
        [Display(Name = "Correo electronico")]
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato valido.")]
        public string Correo { get; set; } = string.Empty;

        [Display(Name = "Contrasena")]
        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Mantener sesion iniciada")]
        public bool Recordarme { get; set; }
    }
}
