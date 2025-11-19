using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saga
{
    public class Node
    {
        public readonly CellType cellType = CellType.BUBBLE;
        public readonly int group = -1;
        public readonly Vector3Int coord = Vector3Int.zero;
        public readonly HashSet<Node> neighbors = new();

        public Bubble bubble = null;

        // y가 짝수일때
        private static readonly Vector3Int[] evenNeighborCoords = 
        {
            new(1, 0), // 0-60
            new(0, 1), // 60-120
            new(-1, 1), // 120-180
            new(-1, 0), // 180-240
            new(-1, -1), // 240-300
            new(0, -1), // 300-360
        };

        // y가 홀수일때
        private static readonly Vector3Int[] oddNeighborCoords = evenNeighborCoords.Select(k => (k.y % 2 == 0) ? k : new Vector3Int(k.x + 1, k.y)).ToArray();

        public Node(Vector3Int coord)
        {
            this.coord = coord;
        }

        public Node(CellType cellType, Vector3Int coord, int group)
        {
            this.cellType = cellType;
            this.coord = coord;
            this.group = group;
        }

        public void OnDisable()
        {
            bubble?.OnDisable();
            bubble = null;
        }

        public void SetNeighbors(Dictionary<Vector3Int, Node> allNodes)
        {
            var addCoords = GetNeighborCoords();
            foreach (var aroundCoord in addCoords)
            {
                var key = coord + aroundCoord;
                if (!allNodes.TryGetValue(key, out var neighbor))
                {
                    continue;
                }

                ApplyNeighbor(neighbor);
            }
        }

        private void ApplyNeighbor(Node neighbor)
        {
            if (neighbors.Contains(neighbor))
            {
                return;
            }

            // 양방향 등록
            neighbors.Add(neighbor);
            neighbor.ApplyNeighbor(this);
        }

        public bool IsCeiling()
        {
            return cellType == CellType.CEILING || cellType == CellType.SPAWNER;
        }

        public bool IsEmpty()
        {
            return bubble == null;
        }

        public Vector3Int[] GetNeighborCoords()
        {
            return (coord.y % 2 == 0) ? evenNeighborCoords : oddNeighborCoords;
        }

        /// <summary>
        /// 연결된 노드를 반환
        /// </summary>
        public static List<Node> GetConnectedNodes(Node node, List<Node> list, Func<Node, bool> predicate)
        {
            list.Add(node);

            foreach (var n in node.neighbors)
            {
                if (list.Contains(n))
                {
                    continue;
                }

                if (predicate != null && !predicate.Invoke(n))
                {
                    continue;
                }

                GetConnectedNodes(n, list, predicate);
            }

            return list;
        }
    }
}