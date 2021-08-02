using Silky.Account.Domain.Utils;

namespace Silky.Account.Domain.Accounts
{
    public class PasswordHelper : IPasswordHelper
    {
        public string EncryptPassword(string userName, string plainPassword)
        {
            return EncryptHelper.Md5(EncryptHelper.Md5(userName + plainPassword));
        }
    }
}