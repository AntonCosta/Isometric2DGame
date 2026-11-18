using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Pathfinding
{
    public class PathGrid : MonoBehaviour
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap[] _walkableTilemaps;

        [Header("Obstacles")]
        [SerializeField] private LayerMask _obstacleLayer;
        [SerializeField] private float _obstacleCheckRadius = 0.2f;

        [Header("Movement")]
        [SerializeField] private bool _allowDiagonalMovement = true;
        [SerializeField] private bool _preventDiagonalCornerCutting = true;

        [Header("Debug")]
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField] private float _gizmoSize = 0.08f;

        private readonly Dictionary<Vector3Int, PathNode> _nodes = new();

        public bool AllowDiagonalMovement => _allowDiagonalMovement;

        private void Awake()
        {
            BuildGrid();
        }

        public void BuildGrid()
        {
            _nodes.Clear();

            if (_walkableTilemaps == null || _walkableTilemaps.Length == 0)
            {
                Debug.LogError("PathGrid is missing a ground Tilemap reference.", this);
                return;
            }

            foreach (var tilemap in _walkableTilemaps)
            {
                if (tilemap == null)
                    continue;

                var bounds = tilemap.cellBounds;

                foreach (var cellPosition in bounds.allPositionsWithin)
                {
                    if (!tilemap.HasTile(cellPosition))
                        continue;

                    if (_nodes.ContainsKey(cellPosition))
                        continue;

                    var worldPosition = tilemap.GetCellCenterWorld(cellPosition);

                    bool blockedByObstacle = Physics2D.OverlapCircle(
                        worldPosition,
                        _obstacleCheckRadius,
                        _obstacleLayer
                    );

                    var walkable = !blockedByObstacle;

                    var node = new PathNode(cellPosition, worldPosition, walkable);
                    _nodes.Add(cellPosition, node);
                }
            }
        }

        public PathNode GetNodeFromWorldPosition(Vector3 worldPosition)
        {
            foreach (var tilemap in _walkableTilemaps)
            {
                if (tilemap == null)
                    continue;

                var cellPosition = tilemap.WorldToCell(worldPosition);

                if (_nodes.TryGetValue(cellPosition, out PathNode node))
                    return node;
            }

            return null;
        }

        public PathNode GetNodeFromCellPosition(Vector3Int cellPosition)
        {
            _nodes.TryGetValue(cellPosition, out PathNode node);
            return node;
        }

        public List<PathNode> GetNeighbours(PathNode node)
        {
            List<PathNode> neighbours = new List<PathNode>();

            AddNeighbour(neighbours, node.CellPosition, new Vector3Int(1, 0, 0));
            AddNeighbour(neighbours, node.CellPosition, new Vector3Int(-1, 0, 0));
            AddNeighbour(neighbours, node.CellPosition, new Vector3Int(0, 1, 0));
            AddNeighbour(neighbours, node.CellPosition, new Vector3Int(0, -1, 0));

            if (_allowDiagonalMovement)
            {
                AddNeighbour(neighbours, node.CellPosition, new Vector3Int(1, 1, 0));
                AddNeighbour(neighbours, node.CellPosition, new Vector3Int(1, -1, 0));
                AddNeighbour(neighbours, node.CellPosition, new Vector3Int(-1, 1, 0));
                AddNeighbour(neighbours, node.CellPosition, new Vector3Int(-1, -1, 0));
            }

            return neighbours;
        }

        private void AddNeighbour(List<PathNode> neighbours, Vector3Int originCell, Vector3Int offset)
        {
            var neighbourCell = originCell + offset;
            var neighbour = GetNodeFromCellPosition(neighbourCell);

            if (neighbour == null || !neighbour.Walkable)
                return;

            if (_preventDiagonalCornerCutting && Mathf.Abs(offset.x) == 1 && Mathf.Abs(offset.y) == 1)
            {
                var horizontal = GetNodeFromCellPosition(originCell + new Vector3Int(offset.x, 0, 0));
                var vertical = GetNodeFromCellPosition(originCell + new Vector3Int(0, offset.y, 0));

                if (horizontal == null || vertical == null)
                    return;

                if (!horizontal.Walkable || !vertical.Walkable)
                    return;
            }

            neighbours.Add(neighbour);
        }

        public void ResetNodes()
        {
            foreach (var node in _nodes.Values)
            {
                node.ResetPathData();
            }
        }

        public List<PathNode> GetAllWalkableNodes()
        {
            var walkableNodes = new List<PathNode>();

            foreach (var node in _nodes.Values)
            {
                if (node.Walkable)
                    walkableNodes.Add(node);
            }

            return walkableNodes;
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos || _nodes == null)
                return;

            foreach (var node in _nodes.Values)
            {
                Gizmos.color = node.Walkable ? Color.green : Color.red;
                Gizmos.DrawSphere(node.WorldPosition, _gizmoSize);
            }
        }
    }
}
