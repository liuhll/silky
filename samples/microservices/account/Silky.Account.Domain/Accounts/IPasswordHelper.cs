using Silky.Core.DependencyInjection;

namespace Silky.Account.Domain.Accounts
{
    public interface IPasswordHelper : ITransientDependency
    {
        string EncryptPassword(string userName, string plainPassword);
    }
}