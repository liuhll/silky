using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Silky.Core.Extensions;
using Silky.EntityFrameworkCore.Entities;
using Silky.Order.Domain.Shared.Orders;
using Silky.Rpc.Runtime.Server;

namespace Silky.Order.Domain.Orders
{
    public class Order : IEntity
    {
        private readonly ISession _session;
        public Order()
        {
            _session = NullSession.Instance;
            CreateTime = DateTime.Now;
            CreateBy = _session.UserId?.ConventTo<long>();
        }

        [Key]
        public long Id { get; set; }

        [Required]
        public long AccountId { get; set; }

        [Required]
        public long ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime CreateTime { get; set; }
        
        public long? CreateBy { get; set; }
        
        public DateTime UpdateTime { get; set; }

        public long? UpdateBy { get; set; }
    }
}