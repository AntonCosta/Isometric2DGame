using UnityEngine;

namespace Pathfinding
{
    public class PathNode
    {
        public Vector3Int CellPosition { get; }
        public Vector3 WorldPosition { get; }
        public bool Walkable { get; set; }

        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost => GCost + HCost;

        public PathNode Parent { get; set; }

        public PathNode(Vector3Int cellPosition, Vector3 worldPosition, bool walkable)
        {
            CellPosition = cellPosition;
            WorldPosition = worldPosition;
            Walkable = walkable;

            ResetPathData();
        }

        public void ResetPathData()
        {
            GCost = int.MaxValue;
            HCost = 0;
            Parent = null;
        }
    }
}
