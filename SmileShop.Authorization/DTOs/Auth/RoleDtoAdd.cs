using SmileShop.Authorization.Validations;
using System.ComponentModel.DataAnnotations;

namespace SmileShop.Authorization.DTOs
{
    public class RoleDtoAdd
    {
        [Required]
        [FirstLetterUpperCase]
        public string RoleName { get; set; }
    }
}