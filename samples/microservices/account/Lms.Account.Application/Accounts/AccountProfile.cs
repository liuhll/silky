using System;
using AutoMapper;
using Lms.Account.Application.Contracts.Accounts.Dtos;
using Lms.Rpc.Runtime.Session;

namespace Lms.Account.Application.Accounts
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