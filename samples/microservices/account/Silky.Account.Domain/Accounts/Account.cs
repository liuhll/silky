using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Silky.EntityFrameworkCore.Entities;
using Silky.Rpc.Runtime.Session;

namespace Silky.Account.Domain.Accounts
{
    public class Account : IEntity
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
        public string UserName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Password { get; set; }

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