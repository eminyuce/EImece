using EImece.Domain.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    public class TwoFactorToken
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string UserId { get; set; }

        [Required]
        [MaxLength(128)]
        public string Token { get; set; }

        public DateTime ExpiresUtc { get; set; }

        public bool IsUsed { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }
    }
}
