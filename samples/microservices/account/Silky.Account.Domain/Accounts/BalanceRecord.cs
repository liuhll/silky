using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Silky.Account.Domain.Shared.Accounts;
using Silky.Core.Extensions;
using Silky.EntityFrameworkCore.Entities;
using Silky.Rpc.Runtime.Server;

namespace Silky.Account.Domain.Accounts
{
    public class BalanceRecord : IEntity
    {
        private readonly ISession _session;
        public BalanceRecord()
        {
            _session = NullSession.Instance;
            CreateTime = DateTime.Now;
            CreateBy = _session.UserId?.ConventTo<long>();
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