namespace Silky.Rpc.Runtime.Session
{
    public static class SessionExtensions
    {
        public static bool IsLogin(this ISession session)
        {
            if (session == null)
            {
                return false;
            }
            return session.UserId.HasValue;
        }
    }
}