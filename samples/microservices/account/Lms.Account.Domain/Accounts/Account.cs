using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Silky.Lms.Rpc.Runtime.Session;

namespace Lms.Account.Domain.Accounts
{
    public class Account
    {
        private readonly ISession _session;
        public Account()
        {
            _session = NullSession.Instance;
            CreateTime = DateTime.Now;
            CreateBy = _session.UserId;
        }

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

        [Column(TypeName = "decimal(10, 2)")]
        public decimal LockBalance { get; set; }

        public DateTime CreateTime { get; set; }
        
        public long? CreateBy { get; set; }
        
        public DateTime UpdateTime { get; set; }

        public long? UpdateBy { get; set; }
    }
}