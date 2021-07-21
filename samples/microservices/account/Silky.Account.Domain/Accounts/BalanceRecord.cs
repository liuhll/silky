using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Silky.Account.Domain.Shared.Accounts;
using Silky.Rpc.Runtime.Session;

namespace Silky.Account.Domain.Accounts
{
    public class BalanceRecord
    {
        private readonly ISession _session;
        public BalanceRecord()
        {
            _session = NullSession.Instance;
            CreateTime = DateTime.Now;
            CreateBy = _session.UserId;
        }

        [Key]
        public long Id { get; set; }

        public long OrderId { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal OrderBalance { get; set; }

        public PayStatus PayStatus { get; set; }

        public DateTime CreateTime { get; set; }
        
        public long? CreateBy { get; set; }
        
        public DateTime UpdateTime { get; set; }

        public long? UpdateBy { get; set; }
    }
}