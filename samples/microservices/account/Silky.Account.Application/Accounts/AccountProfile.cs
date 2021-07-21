using System;
using AutoMapper;
using Silky.Account.Application.Contracts.Accounts.Dtos;
using Silky.Rpc.Runtime.Session;

namespace Silky.Account.Application.Accounts
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<CreateAccountInput, Domain.Accounts.Account>();
            CreateMap<Domain.Accounts.Account, GetAccountOutput>();
            CreateMap<UpdateAccountInput, Domain.Accounts.Account>().AfterMap((src, dest) =>
            {
                dest.UpdateTime = DateTime.Now;
                dest.UpdateBy = NullSession.Instance.UserId;
            });
        }
    }
}