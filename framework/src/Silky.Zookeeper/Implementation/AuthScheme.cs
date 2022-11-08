namespace Silky.Zookeeper;

/// <summary>
/// 认证类型
/// </summary>
public enum AuthScheme
{
    /// <summary>
    /// 下面只有一个id，叫anyone，world:anyone代表任何人，zookeeper中对所有人有权限的结点就是属于world:anyone类型的。创建节点的默认权限。有唯一的id是anyone授权的时候的模式为 world:anyone:rwadc 表示所有人都对这个节点有rwadc的权限
    /// </summary>
    World = 0,
    
    /// <summary>
    ///不需要id,只要是通过authentication的user都有权限（zookeeper支持通过kerberos来进行authencation, 也支持username/password形式的authentication)
    /// </summary>
    Auth = 1,
    
    /// <summary>
    /// 它对应的id为username:BASE64(SHA1(password))，它需要先通过加密过的username:password形式的authentication。
    /// </summary>
    Digest = 2,
    
    /// <summary>
    ///它对应的id为客户机的IP地址，设置的时候可以设置一个ip段，比如ip:192.168.1.0/16。
    /// </summary>
    Ip = 3,
    
    /// <summary>
    /// 在这种scheme情况下，对应的id拥有超级权限，可以做任何事情(cdrwa）
    /// </summary>
    Super = 4
}