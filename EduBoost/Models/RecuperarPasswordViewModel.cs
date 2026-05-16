using System.ComponentModel.DataAnnotations;

namespace EduBoost.Models
{
    public class RecuperarPasswordViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string Correo { get; set; } = string.Empty;
    }
}
