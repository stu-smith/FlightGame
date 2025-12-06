using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FlightGame.Rendering.Core;

public class Octree<T> where T : IOctreeItem
{
    private const int _maxItemsPerNode = 8;
    private const int _maxDepth = 8;
    private const float _minNodeSize = 0.1f;

    private readonly OctreeNode _root;

    public Octree(float size)
    {
        if (size <= 0)
        {
            throw new ArgumentException("Size must be greater than zero.", nameof(size));
        }

        var halfSize = size * 0.5f;
        var bounds = new AxisAlignedBoundingBox(
            -halfSize, -halfSize, -halfSize,
            halfSize, halfSize, halfSize
        );
        _root = new OctreeNode(bounds, 0);
    }

    public void Insert(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var boundingBox = item.GetBoundingBox();
        _root.Insert(item, boundingBox);
    }

    public void Remove(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var boundingBox = item.GetBoundingBox();
        _root.Remove(item, boundingBox);
    }

    public List<T> Query(AxisAlignedBoundingBox bounds)
    {
        var results = new HashSet<T>();
        _root.Query(bounds, results);
        return [.. results];
    }

    public List<T> Query(Vector3 point)
    {
        var results = new HashSet<T>();
        _root.Query(point, results);
        return [.. results];
    }

    public List<T> GetAllItems()
    {
        var results = new HashSet<T>();
        _root.GetAllItems(results);
        return [.. results];
    }

    public void Clear()
    {
        _root.Clear();
    }

    private class OctreeNode(AxisAlignedBoundingBox bounds, int depth)
    {
        private readonly AxisAlignedBoundingBox _bounds = bounds;
        private readonly int _depth = depth;
        private readonly List<T> _items = [];
        private OctreeNode[]? _children;

        public void Insert(T item, AxisAlignedBoundingBox itemBounds)
        {
            if (!_bounds.Intersects(itemBounds))
            {
                return;
            }

            if (_children == null)
            {
                if (_items.Count < _maxItemsPerNode || _depth >= _maxDepth ||
                    _bounds.Width <= _minNodeSize || _bounds.Height <= _minNodeSize || _bounds.Depth <= _minNodeSize)
                {
                    _items.Add(item);
                    return;
                }

                Subdivide();
            }

            foreach (var child in _children!)
            {
                child.Insert(item, itemBounds);
            }
        }

        public void Remove(T item, AxisAlignedBoundingBox itemBounds)
        {
            if (!_bounds.Intersects(itemBounds))
            {
                return;
            }

            if (_children == null)
            {
                _items.Remove(item);
            }
            else
            {
                foreach (var child in _children)
                {
                    child.Remove(item, itemBounds);
                }

                TryMerge();
            }
        }

        public void Query(AxisAlignedBoundingBox bounds, HashSet<T> results)
        {
            if (!_bounds.Intersects(bounds))
            {
                return;
            }

            if (_children == null)
            {
                foreach (var item in _items)
                {
                    var itemBounds = item.GetBoundingBox();
                    if (bounds.Intersects(itemBounds))
                    {
                        results.Add(item);
                    }
                }
            }
            else
            {
                foreach (var child in _children)
                {
                    child.Query(bounds, results);
                }
            }
        }

        public void Query(Vector3 point, HashSet<T> results)
        {
            if (!_bounds.Contains(point))
            {
                return;
            }

            if (_children == null)
            {
                foreach (var item in _items)
                {
                    var itemBounds = item.GetBoundingBox();
                    if (itemBounds.Contains(point))
                    {
                        results.Add(item);
                    }
                }
            }
            else
            {
                foreach (var child in _children)
                {
                    child.Query(point, results);
                }
            }
        }

        public void GetAllItems(HashSet<T> results)
        {
            if (_children == null)
            {
                foreach (var item in _items)
                {
                    results.Add(item);
                }
            }
            else
            {
                foreach (var child in _children)
                {
                    child.GetAllItems(results);
                }
            }
        }

        public void Clear()
        {
            _items.Clear();
            _children = null;
        }

        private void Subdivide()
        {
            var center = _bounds.Center;

            _children = new OctreeNode[8];

            // Bottom-left-front
            _children[0] = new OctreeNode(
                new AxisAlignedBoundingBox(
                    _bounds.Min.X, _bounds.Min.Y, _bounds.Min.Z,
                    center.X, center.Y, center.Z),
                _depth + 1);

            // Bottom-right-front
            _children[1] = new OctreeNode(
                new AxisAlignedBoundingBox(
                    center.X, _bounds.Min.Y, _bounds.Min.Z,
                    _bounds.Max.X, center.Y, center.Z),
                _depth + 1);

            // Bottom-left-back
            _children[2] = new OctreeNode(
                new AxisAlignedBoundingBox(
                    _bounds.Min.X, _bounds.Min.Y, center.Z,
                    center.X, center.Y, _bounds.Max.Z),
                _depth + 1);

            // Bottom-right-back
            _children[3] = new OctreeNode(
                new AxisAlignedBoundingBox(
                    center.X, _bounds.Min.Y, center.Z,
                    _bounds.Max.X, center.Y, _bounds.Max.Z),
                _depth + 1);

            // Top-left-front
            _children[4] = new OctreeNode(
                new AxisAlignedBoundingBox(
                    _bounds.Min.X, center.Y, _bounds.Min.Z,
                    center.X, _bounds.Max.Y, center.Z),
                _depth + 1);

            // Top-right-front
            _children[5] = new OctreeNode(
                new AxisAlignedBoundingBox(
                    center.X, center.Y, _bounds.Min.Z,
                    _bounds.Max.X, _bounds.Max.Y, center.Z),
                _depth + 1);

            // Top-left-back
            _children[6] = new OctreeNode(
                new AxisAlignedBoundingBox(
                    _bounds.Min.X, center.Y, center.Z,
                    center.X, _bounds.Max.Y, _bounds.Max.Z),
                _depth + 1);

            // Top-right-back
            _children[7] = new OctreeNode(
                new AxisAlignedBoundingBox(
                    center.X, center.Y, center.Z,
                    _bounds.Max.X, _bounds.Max.Y, _bounds.Max.Z),
                _depth + 1);

            // Redistribute existing items to children
            var itemsToRedistribute = new List<T>(_items);
            _items.Clear();

            foreach (var item in itemsToRedistribute)
            {
                var itemBounds = item.GetBoundingBox();
                foreach (var child in _children)
                {
                    child.Insert(item, itemBounds);
                }
            }
        }

        private void TryMerge()
        {
            if (_children == null)
            {
                return;
            }

            var totalItems = 0;
            foreach (var child in _children)
            {
                if (child._children != null)
                {
                    return; // Can't merge if any child has children
                }
                totalItems += child._items.Count;
            }

            if (totalItems <= _maxItemsPerNode)
            {
                // Merge all children items into this node
                foreach (var child in _children)
                {
                    _items.AddRange(child._items);
                }
                _children = null;
            }
        }
    }
}
