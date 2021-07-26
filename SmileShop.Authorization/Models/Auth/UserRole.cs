using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmileShop.Authorization.Models.Auth
{
    [Table("UserRole", Schema = "auth")]
    public class UserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        public User User { get; set; }
        public Role Role { get; set; }

        public string CreatedByUserId { get; set; }

        public string CreatedDate { get; set; }
    }
}