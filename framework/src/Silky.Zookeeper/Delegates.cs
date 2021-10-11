using System.Collections.Generic;
using System.Threading.Tasks;
using org.apache.zookeeper;

namespace Silky.Zookeeper
{
    /// <summary>
    /// 连接状态变更事件参数。
    /// </summary>
    public class ConnectionStateChangeArgs
    {
        /// <summary>
        /// 连接状态。
        /// </summary>
        public Watcher.Event.KeeperState State { get; set; }
    }

    /// <summary>
    /// 节点变更参数。
    /// </summary>
    public abstract class NodeChangeArgs
    {
        /// <summary>
        /// 创建一个新的节点变更参数。
        /// </summary>
        /// <param name="path">节点路径。</param>
        /// <param name="type">事件类型。</param>
        protected NodeChangeArgs(string path, Watcher.Event.EventType type)
        {
            Path = path;
            Type = type;
        }

        /// <summary>
        /// 变更类型。
        /// </summary>
        public Watcher.Event.EventType Type { get; private set; }

        /// <summary>
        /// 节点路径。
        /// </summary>
        public string Path { get; private set; }
    }

    /// <summary>
    /// 节点数据变更参数。
    /// </summary>
    public sealed class NodeDataChangeArgs : NodeChangeArgs
    {
        /// <summary>
        /// 创建一个新的节点数据变更参数。
        /// </summary>
        /// <param name="path">节点路径。</param>
        /// <param name="type">事件类型。</param>
        /// <param name="currentData">最新的节点数据。</param>
        public NodeDataChangeArgs(string path, Watcher.Event.EventType type, IEnumerable<byte> currentData) : base(path,
            type)
        {
            CurrentData = currentData;
        }

        /// <summary>
        /// 当前节点数据（最新的）
        /// </summary>
        public IEnumerable<byte> CurrentData { get; private set; }
    }

    /// <summary>
    /// 节点子节点变更参数。
    /// </summary>
    public sealed class NodeChildrenChangeArgs : NodeChangeArgs
    {
        /// <summary>
        /// 创建一个新的节点子节点变更参数。
        /// </summary>
        /// <param name="path">节点路径。</param>
        /// <param name="type">事件类型。</param>
        /// <param name="currentChildrens">最新的子节点集合。</param>
        public NodeChildrenChangeArgs(string path, Watcher.Event.EventType type, IEnumerable<string> currentChildrens) :
            base(path, type)
        {
            CurrentChildrens = currentChildrens;
        }

        /// <summary>
        /// 当前节点的子节点数据（最新的）
        /// </summary>
        public IEnumerable<string> CurrentChildrens { get; private set; }
    }

    /// <summary>
    /// 节点数据变更委托。
    /// </summary>
    /// <param name="client">ZooKeeper客户端。</param>
    /// <param name="args">节点数据变更参数。</param>
    public delegate Task NodeDataChangeHandler(IZookeeperClient client, NodeDataChangeArgs args);

    /// <summary>
    /// 节点子节点变更委托。
    /// </summary>
    /// <param name="client">ZooKeeper客户端。</param>
    /// <param name="args">节点子节点变更参数。</param>
    public delegate Task NodeChildrenChangeHandler(IZookeeperClient client, NodeChildrenChangeArgs args);

    /// <summary>
    /// 连接状态变更委托。
    /// </summary>
    /// <param name="client">ZooKeeper客户端。</param>
    /// <param name="args">连接状态变更参数。</param>
    public delegate Task ConnectionStateChangeHandler(IZookeeperClient client, ConnectionStateChangeArgs args);
}