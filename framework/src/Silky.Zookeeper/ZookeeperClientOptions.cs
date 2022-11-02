using System;

namespace Silky.Zookeeper
{
    /// <summary>
    /// ZooKeeper客户端选项。
    /// </summary>
    public class ZookeeperClientOptions
    {
        /// <summary>
        /// 创建一个新的ZooKeeper客户端选项。
        /// </summary>
        /// <remarks>
        /// <see cref="ConnectionTimeout"/> 为10秒。
        /// <see cref="SessionTimeout"/> 为20秒。
        /// <see cref="OperatingTimeout"/> 为60秒。
        /// <see cref="ReadOnly"/> 为false。
        /// <see cref="SessionId"/> 为0。
        /// <see cref="SessionPasswd"/> 为null。
        /// <see cref="BasePath"/> 为null。
        /// <see cref="EnableEphemeralNodeRestore"/> 为true。
        /// </remarks>
        protected ZookeeperClientOptions()
        {
            ConnectionTimeout = TimeSpan.FromSeconds(5);
            SessionTimeout = TimeSpan.FromSeconds(20);
            OperatingTimeout = TimeSpan.FromSeconds(60);
            ReadOnly = false;
            SessionId = 0;
            SessionPasswd = null;
            EnableEphemeralNodeRestore = true;
        }

        /// <summary>
        /// 创建一个新的ZooKeeper客户端选项。
        /// </summary>
        /// <param name="connectionString">连接字符串。</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> 为空。</exception>
        /// <remarks>
        /// <see cref="ConnectionTimeout"/> 为10秒。
        /// <see cref="SessionTimeout"/> 为20秒。
        /// <see cref="OperatingTimeout"/> 为60秒。
        /// <see cref="ReadOnly"/> 为false。
        /// <see cref="SessionId"/> 为0。
        /// <see cref="SessionPasswd"/> 为null。
        /// <see cref="BasePath"/> 为null。
        /// <see cref="EnableEphemeralNodeRestore"/> 为true。
        /// </remarks>
        public ZookeeperClientOptions(string connectionString) : this()
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
        }

        /// <summary>
        /// 连接字符串。
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 等待ZooKeeper连接的时间。
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; }

        /// <summary>
        /// 执行ZooKeeper操作的重试等待时间。
        /// </summary>
        public TimeSpan OperatingTimeout { get; set; }

        /// <summary>
        /// zookeeper会话超时时间。
        /// </summary>
        public TimeSpan SessionTimeout { get; set; }

        /// <summary>
        /// 是否只读，默认为false。
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// 会话Id。
        /// </summary>
        public long SessionId { get; set; }

        /// <summary>
        /// 会话密码。
        /// </summary>
        public byte[] SessionPasswd { get; set; }

        /// <summary>
        /// 基础路径，会在所有的zk操作节点路径上加入此基础路径。
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// 是否启用短暂类型节点的恢复。
        /// </summary>
        public bool EnableEphemeralNodeRestore { get; set; }
    }
}