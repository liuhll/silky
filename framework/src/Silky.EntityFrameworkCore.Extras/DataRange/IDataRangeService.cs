using System.Threading.Tasks;

namespace Silky.EntityFrameworkCore.Extras.DataRange;

public interface IDataRangeService
{
    Task<UserDataRange> GetCurrentUserDataRangeAsync();
}