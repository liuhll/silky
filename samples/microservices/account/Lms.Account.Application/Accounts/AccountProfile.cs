using AutoMapper;
using Lms.Account.Application.Contracts.Accounts.Dtos;

namespace Lms.Account.Application.Accounts
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<CreateAccountInput, Domain.Accounts.Account>();
            CreateMap<Domain.Accounts.Account, GetAccountOutput>();
        }
    }
}