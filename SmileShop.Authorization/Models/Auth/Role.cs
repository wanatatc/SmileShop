using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmileShop.Authorization.Models.Auth
{
    [Table("Role", Schema = "auth")]
    public class Role : IId
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        public string CreatedByUserId { get; set; }

        public string CreatedDate { get; set; }
    }
}