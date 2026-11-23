using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class AStarPathfinder : MonoBehaviour
    {
        [SerializeField] private PathGrid _pathGrid;

        private readonly List<PathNode> _neighbours = new();
        private readonly List<PathNode> _openSet = new();
        private readonly HashSet<PathNode> _closedSet = new();
        
        public bool FindPath(Vector3 startWorldPosition, Vector3 targetWorldPosition, List<Vector3> resultPath)
        {
            resultPath.Clear();

            var startNode = _pathGrid.GetNodeFromWorldPosition(startWorldPosition);
            var targetNode = _pathGrid.GetNodeFromWorldPosition(targetWorldPosition);

            if (startNode == null || targetNode == null)
            {
                return false;
            }

            if (!startNode.Walkable || !targetNode.Walkable)
            {
                return false;
            }

            _pathGrid.ResetNodes();

            _openSet.Clear();
            _closedSet.Clear();

            startNode.GCost = 0;
            startNode.HCost = GetDistanceCost(startNode, targetNode);

            _openSet.Add(startNode);

            while (_openSet.Count > 0)
            {
                var currentNode = GetLowestFCostNode(_openSet);

                if (currentNode == targetNode)
                {
                    BuildWorldPath(targetNode, resultPath);
                    return resultPath.Count > 0;
                }

                _openSet.Remove(currentNode);
                _closedSet.Add(currentNode);
                
                _pathGrid.GetNeighbours(currentNode, _neighbours);

                foreach (var neighbour in _neighbours)
                {
                    if (_closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    var tentativeGCost = currentNode.GCost + GetDistanceCost(currentNode, neighbour);

                    if (tentativeGCost < neighbour.GCost)
                    {
                        neighbour.Parent = currentNode;
                        neighbour.GCost = tentativeGCost;
                        neighbour.HCost = GetDistanceCost(neighbour, targetNode);

                        if (!_openSet.Contains(neighbour))
                        {
                            _openSet.Add(neighbour);
                        }
                    }
                }
            }

            return false;
        }
        
        public bool FindPathNearTarget(Vector3 startWorldPosition, Vector3 targetWorldPosition, float acceptableDistance, List<Vector3> resultPath)
        {
            resultPath.Clear();

            var startNode = _pathGrid.GetNodeFromWorldPosition(startWorldPosition);

            if (startNode == null || !startNode.Walkable)
            {
                return false;
            }

            _pathGrid.ResetNodes();

            _openSet.Clear();
            _closedSet.Clear();

            startNode.GCost = 0;
            startNode.HCost = GetWorldDistanceCost(startNode.WorldPosition, targetWorldPosition);

            _openSet.Add(startNode);

            var bestNode = startNode;
            var bestDistance = Vector3.Distance(startNode.WorldPosition, targetWorldPosition);

            while (_openSet.Count > 0)
            {
                var currentNode = GetLowestFCostNode(_openSet);

                var distanceToTarget = Vector3.Distance(currentNode.WorldPosition, targetWorldPosition);

                if (distanceToTarget < bestDistance)
                {
                    bestDistance = distanceToTarget;
                    bestNode = currentNode;
                }

                if (distanceToTarget <= acceptableDistance)
                {
                    BuildWorldPath(currentNode, resultPath);
                    return resultPath.Count > 0;
                }

                _openSet.Remove(currentNode);
                _closedSet.Add(currentNode);

                _pathGrid.GetNeighbours(currentNode, _neighbours);
                
                foreach (var neighbour in _neighbours)
                {
                    if (_closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    var tentativeGCost = currentNode.GCost + GetDistanceCost(currentNode, neighbour);

                    if (tentativeGCost < neighbour.GCost)
                    {
                        neighbour.Parent = currentNode;
                        neighbour.GCost = tentativeGCost;
                        neighbour.HCost = GetWorldDistanceCost(neighbour.WorldPosition, targetWorldPosition);

                        if (!_openSet.Contains(neighbour))
                        {
                            _openSet.Add(neighbour);
                        }
                    }
                }
            }

            if (bestNode != startNode)
            {
                BuildWorldPath(bestNode, resultPath);
                return resultPath.Count > 0;
            }

            return false;
        }
        
        private int GetWorldDistanceCost(Vector3 a, Vector3 b)
        {
            return Mathf.RoundToInt(Vector3.Distance(a, b) * 100f);
        }

        private void BuildWorldPath(PathNode targetNode, List<Vector3> resultPath)
        {
            resultPath.Clear();

            var currentNode = targetNode;

            while (currentNode != null)
            {
                resultPath.Add(currentNode.WorldPosition);
                currentNode = currentNode.Parent;
            }

            resultPath.Reverse();
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