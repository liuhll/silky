using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lms.Account.Domain.Accounts
{
    public class Account
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Balance { get; set; }

        public DateTime CreateTime { get; set; }
        
        public long CreateBy { get; set; }
        
        public DateTime UpdateTime { get; set; }

        public long UpdateBy { get; set; }
    }
}