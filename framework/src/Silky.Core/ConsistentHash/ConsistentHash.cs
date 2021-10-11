using System.Collections.Generic;
using System.Linq;

namespace Silky.Core
{
    public class ConsistentHash<T>
    {
        private SortedDictionary<int, T> _ring = new SortedDictionary<int, T>();
        private int[] _nodeKeysInRing = null;
        private IHashAlgorithm _hashAlgorithm;
        private int _virtualNodeReplicationFactor = 1000;

        public ConsistentHash(IHashAlgorithm hashAlgorithm)
        {
            _hashAlgorithm = hashAlgorithm;
        }

        public ConsistentHash() : this(new MurmurHash2HashAlgorithm())
        {
        }

        public ConsistentHash(IHashAlgorithm hashAlgorithm, int virtualNodeReplicationFactor)
            : this(hashAlgorithm)
        {
            _virtualNodeReplicationFactor = virtualNodeReplicationFactor;
        }

        public int VirtualNodeReplicationFactor
        {
            get { return _virtualNodeReplicationFactor; }
        }

        public void Initialize(IEnumerable<T> nodes)
        {
            foreach (T node in nodes)
            {
                AddNode(node);
            }

            _nodeKeysInRing = _ring.Keys.ToArray();
        }

        public void Add(T node)
        {
            AddNode(node);
            _nodeKeysInRing = _ring.Keys.ToArray();
        }

        public void Remove(T node)
        {
            RemoveNode(node);
            _nodeKeysInRing = _ring.Keys.ToArray();
        }

        public bool ContainNode(T node)
        {
            return _ring.ContainsValue(node);
        }


        private void AddNode(T node)
        {
            for (int i = 0; i < _virtualNodeReplicationFactor; i++)
            {
                int hashOfVirtualNode = _hashAlgorithm.Hash(node.GetHashCode().ToString() + i);
                _ring[hashOfVirtualNode] = node;
            }
        }

        private void RemoveNode(T node)
        {
            for (int i = 0; i < _virtualNodeReplicationFactor; i++)
            {
                int hashOfVirtualNode = _hashAlgorithm.Hash(node.GetHashCode().ToString() + i);
                _ring.Remove(hashOfVirtualNode);
            }
        }

        public T GetItemNode(string item)
        {
            int hashOfItem = _hashAlgorithm.Hash(item);
            int nearestNodePosition = GetClockwiseNearestNode(_nodeKeysInRing, hashOfItem);
            return _ring[_nodeKeysInRing[nearestNodePosition]];
        }

        private int GetClockwiseNearestNode(int[] keys, int hashOfItem)
        {
            int begin = 0;
            int end = keys.Length - 1;

            if (keys[end] < hashOfItem || keys[0] > hashOfItem)
            {
                return 0;
            }

            int mid = begin;
            while ((end - begin) > 1)
            {
                mid = (end + begin) / 2;
                if (keys[mid] >= hashOfItem) end = mid;
                else begin = mid;
            }

            return end;
        }

        public int GetNodeCount()
        {
            return _ring.Count();
        }
    }
}