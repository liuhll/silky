using System;

namespace Lms.Account.Domain.Accounts
{
    public class Account
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateBy { get; set; }
        
        public DateTime UpdateTime { get; set; }

        public string UpdateBy { get; set; }
    }
}