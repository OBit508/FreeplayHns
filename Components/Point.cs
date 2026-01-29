using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FreeplayHns.Components
{
    public class Point
    {
        public static List<Point> Points = new List<Point>();
        public Vector2 Position;
        public bool FleePoint => this is VentPoint;
        private List<Point> AvaiblePoints = new List<Point>();
        public List<Point> AvaibleFleePoints = new List<Point>();
        public Point(Vector2 Position, bool Connect = true, bool IgnoreColliders = false, float ConnectDistance = 0.5f)
        {
            this.Position = Position;
            if (Connect)
            {
                foreach (Point point in Points)
                {
                    if (Vector2.Distance(point.Position, Position) <= ConnectDistance && (IgnoreColliders || !IgnoreColliders && !AnythingBetween(point.Position, Position)))
                    {
                        point.AvaibleFleePoints.Add(this);
                        AvaibleFleePoints.Add(point);
                        if (!point.FleePoint)
                        {
                            AvaiblePoints.Add(point);
                        }
                        if (!FleePoint)
                        {
                            point.AvaiblePoints.Add(this);
                        }
                    }
                }
            }
            Points.Add(this);
        }
        public List<Point> GetPoints(bool fleeing)
        {
            if (fleeing)
            {
                return AvaibleFleePoints;
            }
            return AvaiblePoints;
        }
        public static Point GetClosestPoint(Vector2 position, bool ignoreColliders = false)
        {
            Point closest = null;
            float distance = float.MaxValue;
            foreach (Point point in Points)
            {
                float dis = Vector2.Distance(position, point.Position);
                if (dis < distance && (ignoreColliders || !ignoreColliders && !AnythingBetween(position, point.Position)))
                {
                    distance = dis;
                    closest = point;
                }
            }
            return closest;
        }
        public static bool AnythingBetween(Vector2 source, Vector2 target)
        {
            return PhysicsHelpers.AnythingBetween(source, target, Constants.ShipAndObjectsMask, false);
        }
        public static List<Point> FindPath(Point start, Point goal, bool fleeing)
        {
            MinHeap openSet = new MinHeap();
            Dictionary<Point, float> gScore = new Dictionary<Point, float>();
            Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();
            HashSet<Point> closed = new HashSet<Point>();
            openSet.Push(start, 0f);
            gScore[start] = 0f;
            while (openSet.Count > 0)
            {
                Point current = openSet.Pop();
                if (current == goal)
                {
                    return ReconstructPath(start, goal, cameFrom);
                }
                closed.Add(current);
                foreach (Point neighbor in current.GetPoints(fleeing))
                {
                    if (closed.Contains(neighbor))
                    {
                        continue;
                    }
                    float cost = Vector2.Distance(current.Position, neighbor.Position);
                    if (current is VentPoint && neighbor is VentPoint)
                    {
                        cost = 0.001f;
                    }
                    float tentative = gScore[current] + cost;
                    if (!gScore.TryGetValue(neighbor, out float oldScore) || tentative < oldScore)
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentative;
                        float f = tentative + Vector2.Distance(neighbor.Position, goal.Position);
                        openSet.Push(neighbor, f);
                    }
                }
            }
            return null;
        }
        private static List<Point> ReconstructPath(Point start, Point goal, Dictionary<Point, Point> cameFrom)
        {
            List<Point> path = new List<Point>();
            Point current = goal;
            while (current != start)
            {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Add(start);
            path.Reverse();
            return path;
        }
        private class MinHeap
        {
            public List<(Point item, float priority)> heap = new List<(Point item, float priority)>();
            public int Count => heap.Count;
            public void Push(Point item, float priority)
            {
                heap.Add((item, priority));
                int i = heap.Count - 1;
                while (i > 0)
                {
                    int parent = (i - 1) >> 1;
                    if (heap[i].priority >= heap[parent].priority)
                    {
                        break;
                    }
                    (heap[i], heap[parent]) = (heap[parent], heap[i]);
                    i = parent;
                }
            }
            public Point Pop()
            {
                Point root = heap[0].item;
                int last = heap.Count - 1;
                heap[0] = heap[last];
                heap.RemoveAt(last);
                int i = 0;
                while (true)
                {
                    int left = (i << 1) + 1;
                    int right = left + 1;
                    int smallest = i;
                    if (left < heap.Count && heap[left].priority < heap[smallest].priority)
                    {
                        smallest = left;
                    }
                    if (right < heap.Count && heap[right].priority < heap[smallest].priority)
                    {
                        smallest = right;
                    }
                    if (smallest == i)
                    {
                        break;
                    }
                    (heap[i], heap[smallest]) = (heap[smallest], heap[i]);
                    i = smallest;
                }
                return root;
            }
        }
    }
}
