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
        var bounds = new BoundingBox(
            new Vector3(-halfSize, -halfSize, -halfSize),
            new Vector3(halfSize, halfSize, halfSize)
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

    public List<T> Query(BoundingBox bounds)
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

    public List<T> Query(BoundingFrustum frustum)
    {
        var results = new HashSet<T>();
        _root.Query(frustum, results);
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

    private class OctreeNode(BoundingBox bounds, int depth)
    {
        private readonly BoundingBox _bounds = bounds;
        private readonly int _depth = depth;
        private readonly List<T> _items = [];
        private OctreeNode[]? _children;

        public void Insert(T item, BoundingBox itemBounds)
        {
            if (!_bounds.Intersects(itemBounds))
            {
                return;
            }

            if (_children == null)
            {
                var size = _bounds.Max - _bounds.Min;
                if (_items.Count < _maxItemsPerNode || _depth >= _maxDepth ||
                    size.X <= _minNodeSize || size.Y <= _minNodeSize || size.Z <= _minNodeSize)
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

        public void Remove(T item, BoundingBox itemBounds)
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

        public void Query(BoundingBox bounds, HashSet<T> results)
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
            if (_bounds.Contains(point) == ContainmentType.Disjoint)
            {
                return;
            }

            if (_children == null)
            {
                foreach (var item in _items)
                {
                    var itemBounds = item.GetBoundingBox();

                    if (itemBounds.Contains(point) == ContainmentType.Disjoint)
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

        private void Contains(ref BoundingFrustum boundingFrustum, ref BoundingBox box, out ContainmentType result)
        {
            var intersects = false;
            Plane[] planes = [
                boundingFrustum.Near,
                    //boundingFrustum.Far,
                    boundingFrustum.Left,
                    boundingFrustum.Right,
                    boundingFrustum.Top,
                    boundingFrustum.Bottom
            ];

            for (var i = 0; i < planes.Length; ++i)
            {
                var planeIntersectionType = default(PlaneIntersectionType);

                box.Intersects(ref planes[i], out planeIntersectionType);
                switch (planeIntersectionType)
                {
                    case PlaneIntersectionType.Front:
                        result = ContainmentType.Disjoint;
                        return;
                    case PlaneIntersectionType.Intersecting:
                        intersects = true;
                        break;
                }
            }
            result = intersects ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        bool Intersects(ref BoundingFrustum boundingFrustum, BoundingBox box)
        {
            var result = false;
            this.Intersects(ref boundingFrustum, ref box, out result);
            return result;
        }

        void Intersects(ref BoundingFrustum boundingFrustum, ref BoundingBox box, out bool result)
        {
            var containment = default(ContainmentType);
            Contains(ref boundingFrustum, ref box, out containment);
            result = containment != ContainmentType.Disjoint;
        }

        public void Query(BoundingFrustum frustum, HashSet<T> results)
        {
            // Check if the node's bounds intersect with the frustum
            if (!frustum.Intersects(_bounds))
            {
                return;
            }

            if (_children == null)
            {
                // Leaf node: check each item's bounding box against the frustum
                foreach (var item in _items)
                {
                    var itemBounds = item.GetBoundingBox();

                    if (!Intersects(ref frustum, itemBounds))
                    {
                        continue;
                    }

                    results.Add(item);
                }
            }
            else
            {
                // Internal node: recursively query children
                foreach (var child in _children)
                {
                    child.Query(frustum, results);
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
            var center = _bounds.Center();

            _children = new OctreeNode[8];

            // Bottom-left-front
            _children[0] = new OctreeNode(
                new BoundingBox(
                    _bounds.Min,
                    center),
                _depth + 1);

            // Bottom-right-front
            _children[1] = new OctreeNode(
                new BoundingBox(
                    new Vector3(center.X, _bounds.Min.Y, _bounds.Min.Z),
                    new Vector3(_bounds.Max.X, center.Y, center.Z)),
                _depth + 1);

            // Bottom-left-back
            _children[2] = new OctreeNode(
                new BoundingBox(
                    new Vector3(_bounds.Min.X, _bounds.Min.Y, center.Z),
                    new Vector3(center.X, center.Y, _bounds.Max.Z)),
                _depth + 1);

            // Bottom-right-back
            _children[3] = new OctreeNode(
                new BoundingBox(
                    new Vector3(center.X, _bounds.Min.Y, center.Z),
                    new Vector3(_bounds.Max.X, center.Y, _bounds.Max.Z)),
                _depth + 1);

            // Top-left-front
            _children[4] = new OctreeNode(
                new BoundingBox(
                    new Vector3(_bounds.Min.X, center.Y, _bounds.Min.Z),
                    new Vector3(center.X, _bounds.Max.Y, center.Z)),
                _depth + 1);

            // Top-right-front
            _children[5] = new OctreeNode(
                new BoundingBox(
                    new Vector3(center.X, center.Y, _bounds.Min.Z),
                    new Vector3(_bounds.Max.X, _bounds.Max.Y, center.Z)),
                _depth + 1);

            // Top-left-back
            _children[6] = new OctreeNode(
                new BoundingBox(
                    new Vector3(_bounds.Min.X, center.Y, center.Z),
                    new Vector3(center.X, _bounds.Max.Y, _bounds.Max.Z)),
                _depth + 1);

            // Top-right-back
            _children[7] = new OctreeNode(
                new BoundingBox(
                    center,
                    _bounds.Max),
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
