using System.ComponentModel.DataAnnotations;

namespace Machly.Web.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public string Role { get; set; } = "";
    }
}

