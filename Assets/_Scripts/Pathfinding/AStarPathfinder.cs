using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class AStarPathfinder : MonoBehaviour
    {
        [SerializeField] private PathGrid _pathGrid;

        private readonly List<PathNode> _neighbours = new List<PathNode>();
        
        public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 targetWorldPosition)
        {
            var startNode = _pathGrid.GetNodeFromWorldPosition(startWorldPosition);
            var targetNode = _pathGrid.GetNodeFromWorldPosition(targetWorldPosition);

            if (startNode == null || targetNode == null)
                return null;

            if (!startNode.Walkable || !targetNode.Walkable)
                return null;

            _pathGrid.ResetNodes();

            var openSet = new List<PathNode>();
            var closedSet = new HashSet<PathNode>();

            startNode.GCost = 0;
            startNode.HCost = GetDistanceCost(startNode, targetNode);

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                var currentNode = GetLowestFCostNode(openSet);

                if (currentNode == targetNode)
                    return BuildWorldPath(targetNode);

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                
                _pathGrid.GetNeighbours(currentNode, _neighbours);

                foreach (var neighbour in _neighbours)
                {
                    if (closedSet.Contains(neighbour))
                        continue;

                    int tentativeGCost = currentNode.GCost + GetDistanceCost(currentNode, neighbour);

                    if (tentativeGCost < neighbour.GCost)
                    {
                        neighbour.Parent = currentNode;
                        neighbour.GCost = tentativeGCost;
                        neighbour.HCost = GetDistanceCost(neighbour, targetNode);

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }

            return null;
        }
        
        public List<Vector3> FindPathNearTarget(Vector3 startWorldPosition, Vector3 targetWorldPosition, float acceptableDistance)
        {
            var startNode = _pathGrid.GetNodeFromWorldPosition(startWorldPosition);

            if (startNode == null || !startNode.Walkable)
                return null;

            _pathGrid.ResetNodes();

            var openSet = new List<PathNode>();
            var closedSet = new HashSet<PathNode>();

            startNode.GCost = 0;
            startNode.HCost = GetWorldDistanceCost(startNode.WorldPosition, targetWorldPosition);

            openSet.Add(startNode);

            var bestNode = startNode;
            var bestDistance = Vector3.Distance(startNode.WorldPosition, targetWorldPosition);

            while (openSet.Count > 0)
            {
                var currentNode = GetLowestFCostNode(openSet);

                var distanceToTarget = Vector3.Distance(currentNode.WorldPosition, targetWorldPosition);

                if (distanceToTarget < bestDistance)
                {
                    bestDistance = distanceToTarget;
                    bestNode = currentNode;
                }

                if (distanceToTarget <= acceptableDistance)
                {
                    return BuildWorldPath(currentNode);
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                _pathGrid.GetNeighbours(currentNode, _neighbours);
                
                foreach (var neighbour in _neighbours)
                {
                    if (closedSet.Contains(neighbour))
                        continue;

                    var tentativeGCost = currentNode.GCost + GetDistanceCost(currentNode, neighbour);

                    if (tentativeGCost < neighbour.GCost)
                    {
                        neighbour.Parent = currentNode;
                        neighbour.GCost = tentativeGCost;
                        neighbour.HCost = GetWorldDistanceCost(neighbour.WorldPosition, targetWorldPosition);

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }

            if (bestNode != startNode)
            {
                return BuildWorldPath(bestNode);
            }

            return null;
        }
        
        private int GetWorldDistanceCost(Vector3 a, Vector3 b)
        {
            return Mathf.RoundToInt(Vector3.Distance(a, b) * 100f);
        }

        private List<Vector3> BuildWorldPath(PathNode targetNode)
        {
            var path = new List<Vector3>();
            var currentNode = targetNode;

            while (currentNode != null)
            {
                path.Add(currentNode.WorldPosition);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        private PathNode GetLowestFCostNode(List<PathNode> nodes)
        {
            var lowestNode = nodes[0];

            for (int i = 1; i < nodes.Count; i++)
            {
                var node = nodes[i];

                if (node.FCost < lowestNode.FCost)
                {
                    lowestNode = node;
                }
                else if (node.FCost == lowestNode.FCost && node.HCost < lowestNode.HCost)
                {
                    lowestNode = node;
                }
            }

            return lowestNode;
        }

        private int GetDistanceCost(PathNode a, PathNode b)
        {
            var xDistance = Mathf.Abs(a.CellPosition.x - b.CellPosition.x);
            var yDistance = Mathf.Abs(a.CellPosition.y - b.CellPosition.y);

            if (_pathGrid.AllowDiagonalMovement)
            {
                var diagonalDistance = Mathf.Min(xDistance, yDistance);
                var straightDistance = Mathf.Abs(xDistance - yDistance);

                return 14 * diagonalDistance + 10 * straightDistance;
            }

            return 10 * (xDistance + yDistance);
        }
    }
}