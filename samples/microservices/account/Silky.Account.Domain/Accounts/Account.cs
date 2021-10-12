using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Silky.Core.Extensions;
using Silky.EntityFrameworkCore.Entities;
using Silky.EntityFrameworkCore.Entities.Configures;
using Silky.Rpc.Runtime.Server;

namespace Silky.Account.Domain.Accounts
{
    public class Account : IEntity, IEntitySeedData<Account>
    {
        private readonly ISession _session;

        public Account()
        {
            _session = NullSession.Instance;
            CreateTime = DateTime.Now;
            CreateBy = _session.UserId?.ConventTo<long>();
        }

        [Key] public long Id { get; set; }

        [Required] [MaxLength(100)] public string UserName { get; set; }

        [Required] [MaxLength(100)] public string Password { get; set; }

        [MaxLength(500)] public string Address { get; set; }

        [MaxLength(100)] public string Email { get; set; }

        [Column(TypeName = "decimal(10, 2)")] public decimal Balance { get; set; }

        [Column(TypeName = "decimal(10, 2)")] public decimal LockBalance { get; set; }

        public DateTime CreateTime { get; set; }

        public long? CreateBy { get; set; }

        public DateTime UpdateTime { get; set; }

        public long? UpdateBy { get; set; }

        public IEnumerable<Account> HasData(DbContext dbContext, Type dbContextLocator)
        {
            var passwordHelper = new PasswordHelper();
            return new List<Account>()
            {
                new()
                {
                    Id = 1,
                    Address = "beijing",
                    Balance = 200,
                    Email = "admin@silky.com",
                    Password = passwordHelper.EncryptPassword("admin", "123qwe"),
                    UserName = "admin",
                    CreateTime = DateTime.Now
                },
                new()
                {
                    Id = 2,
                    Address = "beijing",
                    Balance = 500,
                    Email = "liuhll@silky.com",
                    Password = passwordHelper.EncryptPassword("liuhll", "123qwe"),
                    UserName = "liuhll",
                    CreateTime = DateTime.Now
                },
                new()
                {
                    Id = 3,
                    Address = "shenzhen",
                    Balance = 3000,
                    Email = "lisi@silky.com",
                    Password = passwordHelper.EncryptPassword("lisi", "123qwe"),
                    UserName = "lisi",
                    CreateTime = DateTime.Now
                }
            };
        }
    }
}