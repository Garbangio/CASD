using System;
using System.Collections.Generic;

namespace Task18
{
    internal class TreeNode<TKey, TValue>
    {
        public TKey Key;
        public TValue Data;
        public TreeNode<TKey, TValue> LeftChild;
        public TreeNode<TKey, TValue> RightChild;
        public TreeNode<TKey, TValue> Ancestor;

        public TreeNode(TKey key, TValue data)
        {
            Key = key;
            Data = data;
        }
    }

    public class CustomTreeMap<TKey, TValue> where TKey : IComparable<TKey>
    {
        private readonly IComparer<TKey> _keyComparer;
        private TreeNode<TKey, TValue> _treeRoot;
        private int _itemCount;

        public CustomTreeMap()
        {
            _keyComparer = Comparer<TKey>.Default;
            _treeRoot = null;
            _itemCount = 0;
        }

        public CustomTreeMap(IComparer<TKey> customComparer)
        {
            _keyComparer = customComparer ?? Comparer<TKey>.Default;
            _treeRoot = null;
            _itemCount = 0;
        }

        public void EraseAll()
        {
            _treeRoot = null;
            _itemCount = 0;
        }

        public bool HasKey(TKey key)
        {
            return LocateNode(key) != null;
        }

        public bool HasValue(TValue value)
        {
            return ScanForValue(_treeRoot, value);
        }

        private bool ScanForValue(TreeNode<TKey, TValue> currentNode, TValue targetValue)
        {
            if (currentNode == null) 
                return false;
            
            if (Equals(currentNode.Data, targetValue)) 
                return true;
            
            return ScanForValue(currentNode.LeftChild, targetValue) || 
                   ScanForValue(currentNode.RightChild, targetValue);
        }

        public List<KeyValuePair<TKey, TValue>> GetAllEntries()
        {
            var resultCollection = new List<KeyValuePair<TKey, TValue>>();
            TraverseInOrder(_treeRoot, resultCollection);
            return resultCollection;
        }

        private void TraverseInOrder(TreeNode<TKey, TValue> currentNode, List<KeyValuePair<TKey, TValue>> accumulator)
        {
            if (currentNode == null) 
                return;
            
            TraverseInOrder(currentNode.LeftChild, accumulator);
            accumulator.Add(new KeyValuePair<TKey, TValue>(currentNode.Key, currentNode.Data));
            TraverseInOrder(currentNode.RightChild, accumulator);
        }

        public TValue Retrieve(TKey key)
        {
            var node = LocateNode(key);
            return node == null ? default : node.Data;
        }

        public bool Empty => _itemCount == 0;

        public List<TKey> ExtractKeys()
        {
            var keyCollection = new List<TKey>();
            CollectKeys(_treeRoot, keyCollection);
            return keyCollection;
        }

        private void CollectKeys(TreeNode<TKey, TValue> currentNode, List<TKey> storage)
        {
            if (currentNode == null) 
                return;
            
            CollectKeys(currentNode.LeftChild, storage);
            storage.Add(currentNode.Key);
            CollectKeys(currentNode.RightChild, storage);
        }

        public void Insert(TKey key, TValue data)
        {
            if (_treeRoot == null)
            {
                _treeRoot = new TreeNode<TKey, TValue>(key, data);
                _itemCount = 1;
                return;
            }

            var current = _treeRoot;
            TreeNode<TKey, TValue> parent = null;

            while (current != null)
            {
                var comparisonResult = _keyComparer.Compare(key, current.Key);
                parent = current;

                if (comparisonResult < 0)
                    current = current.LeftChild;
                else if (comparisonResult > 0)
                    current = current.RightChild;
                else
                {
                    current.Data = data;
                    return;
                }
            }

            var newNode = new TreeNode<TKey, TValue>(key, data) { Ancestor = parent };

            if (_keyComparer.Compare(key, parent.Key) < 0)
                parent.LeftChild = newNode;
            else
                parent.RightChild = newNode;

            _itemCount++;
        }

        public bool Delete(TKey key)
        {
            var targetNode = LocateNode(key);
            if (targetNode == null) 
                return false;

            RemoveNode(targetNode);
            _itemCount--;
            return true;
        }

        public int Count => _itemCount;

        public TKey SmallestKey()
        {
            if (_treeRoot == null) 
                throw new InvalidOperationException("Коллекция пуста");
            
            return FindMinNode(_treeRoot).Key;
        }

        public TKey LargestKey()
        {
            if (_treeRoot == null) 
                throw new InvalidOperationException("Коллекция пуста");
            
            return FindMaxNode(_treeRoot).Key;
        }

        public CustomTreeMap<TKey, TValue> ExtractPreceding(TKey boundaryKey)
        {
            var resultMap = new CustomTreeMap<TKey, TValue>(_keyComparer);
            AccumulatePreceding(_treeRoot, boundaryKey, resultMap);
            return resultMap;
        }

        private void AccumulatePreceding(TreeNode<TKey, TValue> currentNode, TKey limit, CustomTreeMap<TKey, TValue> targetMap)
        {
            if (currentNode == null) 
                return;
            
            AccumulatePreceding(currentNode.LeftChild, limit, targetMap);
            
            if (_keyComparer.Compare(currentNode.Key, limit) < 0)
            {
                targetMap.Insert(currentNode.Key, currentNode.Data);
                AccumulatePreceding(currentNode.RightChild, limit, targetMap);
            }
        }

        public CustomTreeMap<TKey, TValue> ExtractRange(TKey fromKey, TKey toKey)
        {
            var segmentMap = new CustomTreeMap<TKey, TValue>(_keyComparer);
            AccumulateRange(_treeRoot, fromKey, toKey, segmentMap);
            return segmentMap;
        }

        private void AccumulateRange(TreeNode<TKey, TValue> currentNode, TKey lowerBound, TKey upperBound, CustomTreeMap<TKey, TValue> storage)
        {
            if (currentNode == null) 
                return;
            
            if (_keyComparer.Compare(currentNode.Key, lowerBound) >= 0)
                AccumulateRange(currentNode.LeftChild, lowerBound, upperBound, storage);
            
            if (_keyComparer.Compare(currentNode.Key, lowerBound) >= 0 && 
                _keyComparer.Compare(currentNode.Key, upperBound) < 0)
                storage.Insert(currentNode.Key, currentNode.Data);
            
            if (_keyComparer.Compare(currentNode.Key, upperBound) < 0)
                AccumulateRange(currentNode.RightChild, lowerBound, upperBound, storage);
        }

        public CustomTreeMap<TKey, TValue> ExtractSucceeding(TKey startKey)
        {
            var resultMap = new CustomTreeMap<TKey, TValue>(_keyComparer);
            AccumulateSucceeding(_treeRoot, startKey, resultMap);
            return resultMap;
        }

        private void AccumulateSucceeding(TreeNode<TKey, TValue> currentNode, TKey threshold, CustomTreeMap<TKey, TValue> accumulator)
        {
            if (currentNode == null) 
                return;
            
            AccumulateSucceeding(currentNode.RightChild, threshold, accumulator);
            
            if (_keyComparer.Compare(currentNode.Key, threshold) > 0)
            {
                accumulator.Insert(currentNode.Key, currentNode.Data);
                AccumulateSucceeding(currentNode.LeftChild, threshold, accumulator);
            }
        }

        public TreeNode<TKey, TValue> FindPredecessor(TKey referenceKey)
        {
            var cursor = _treeRoot;
            TreeNode<TKey, TValue> candidate = null;

            while (cursor != null)
            {
                if (_keyComparer.Compare(cursor.Key, referenceKey) < 0)
                {
                    candidate = cursor;
                    cursor = cursor.RightChild;
                }
                else
                    cursor = cursor.LeftChild;
            }
            return candidate;
        }

        public TKey PredecessorKey(TKey key) => FindPredecessor(key)?.Key ?? default;

        public TreeNode<TKey, TValue> FindFloor(TKey targetKey)
        {
            var navigator = _treeRoot;
            TreeNode<TKey, TValue> bestMatch = null;

            while (navigator != null)
            {
                var comparison = _keyComparer.Compare(navigator.Key, targetKey);
                
                if (comparison == 0) 
                    return navigator;
                
                if (comparison < 0)
                {
                    bestMatch = navigator;
                    navigator = navigator.RightChild;
                }
                else
                    navigator = navigator.LeftChild;
            }
            return bestMatch;
        }

        public TKey FloorKey(TKey key) => FindFloor(key)?.Key ?? default;

        public TreeNode<TKey, TValue> FindSuccessor(TKey referenceKey)
        {
            var cursor = _treeRoot;
            TreeNode<TKey, TValue> candidate = null;

            while (cursor != null)
            {
                if (_keyComparer.Compare(cursor.Key, referenceKey) > 0)
                {
                    candidate = cursor;
                    cursor = cursor.LeftChild;
                }
                else
                    cursor = cursor.RightChild;
            }
            return candidate;
        }

        public TKey SuccessorKey(TKey key) => FindSuccessor(key)?.Key ?? default;

        public TreeNode<TKey, TValue> FindCeiling(TKey targetKey)
        {
            var navigator = _treeRoot;
            TreeNode<TKey, TValue> bestFit = null;

            while (navigator != null)
            {
                var comparison = _keyComparer.Compare(navigator.Key, targetKey);
                
                if (comparison == 0) 
                    return navigator;
                
                if (comparison > 0)
                {
                    bestFit = navigator;
                    navigator = navigator.LeftChild;
                }
                else
                    navigator = navigator.RightChild;
            }
            return bestFit;
        }

        public TKey CeilingKey(TKey key) => FindCeiling(key)?.Key ?? default;

        public KeyValuePair<TKey, TValue>? ExtractFirst()
        {
            if (_treeRoot == null) 
                return null;
            
            var smallest = FindMinNode(_treeRoot);
            Delete(smallest.Key);
            return new KeyValuePair<TKey, TValue>(smallest.Key, smallest.Data);
        }

        public KeyValuePair<TKey, TValue>? ExtractLast()
        {
            if (_treeRoot == null) 
                return null;
            
            var largest = FindMaxNode(_treeRoot);
            Delete(largest.Key);
            return new KeyValuePair<TKey, TValue>(largest.Key, largest.Data);
        }

        public KeyValuePair<TKey, TValue>? PeekFirst()
        {
            if (_treeRoot == null) 
                return null;
            
            var smallest = FindMinNode(_treeRoot);
            return new KeyValuePair<TKey, TValue>(smallest.Key, smallest.Data);
        }

        public KeyValuePair<TKey, TValue>? PeekLast()
        {
            if (_treeRoot == null) 
                return null;
            
            var largest = FindMaxNode(_treeRoot);
            return new KeyValuePair<TKey, TValue>(largest.Key, largest.Data);
        }

        private TreeNode<TKey, TValue> LocateNode(TKey targetKey)
        {
            var navigator = _treeRoot;
            
            while (navigator != null)
            {
                var comparison = _keyComparer.Compare(targetKey, navigator.Key);
                
                if (comparison == 0) 
                    return navigator;
                
                navigator = comparison < 0 ? navigator.LeftChild : navigator.RightChild;
            }
            
            return null;
        }

        private TreeNode<TKey, TValue> FindMinNode(TreeNode<TKey, TValue> startNode)
        {
            var current = startNode;
            while (current.LeftChild != null)
                current = current.LeftChild;
            return current;
        }

        private TreeNode<TKey, TValue> FindMaxNode(TreeNode<TKey, TValue> startNode)
        {
            var current = startNode;
            while (current.RightChild != null)
                current = current.RightChild;
            return current;
        }

        private void RemoveNode(TreeNode<TKey, TValue> nodeToDelete)
        {
            if (nodeToDelete.LeftChild == null && nodeToDelete.RightChild == null)
                SubstituteNode(nodeToDelete, null);
            else if (nodeToDelete.LeftChild == null)
                SubstituteNode(nodeToDelete, nodeToDelete.RightChild);
            else if (nodeToDelete.RightChild == null)
                SubstituteNode(nodeToDelete, nodeToDelete.LeftChild);
            else
            {
                var replacement = FindMinNode(nodeToDelete.RightChild);
                nodeToDelete.Key = replacement.Key;
                nodeToDelete.Data = replacement.Data;
                RemoveNode(replacement);
            }
        }

        private void SubstituteNode(TreeNode<TKey, TValue> original, TreeNode<TKey, TValue> substitute)
        {
            if (original.Ancestor == null)
                _treeRoot = substitute;
            else if (original == original.Ancestor.LeftChild)
                original.Ancestor.LeftChild = substitute;
            else
                original.Ancestor.RightChild = substitute;

            if (substitute != null)
                substitute.Ancestor = original.Ancestor;
        }
    }
}
