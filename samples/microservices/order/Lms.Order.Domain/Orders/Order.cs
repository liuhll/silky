using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lms.Order.Domain.Shared.Orders;
using Silky.Lms.Rpc.Runtime.Session;

namespace Lms.Order.Domain.Orders
{
    public class Order
    {
        private readonly ISession _session;
        public Order()
        {
            _session = NullSession.Instance;
            CreateTime = DateTime.Now;
            CreateBy = _session.UserId;
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