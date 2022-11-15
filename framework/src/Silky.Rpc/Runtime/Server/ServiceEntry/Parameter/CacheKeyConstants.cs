namespace Silky.Rpc.Runtime.Server;

public class CacheKeyConstants
{
      public const string CacheKeyParameterRegex = @"{[^\}]+\}";
    
      public const string CacheKeyStringRegex = @"\{\D*\}";
        
      public const string CacheKeyIndexRegex = @"\{\d+\}";
}