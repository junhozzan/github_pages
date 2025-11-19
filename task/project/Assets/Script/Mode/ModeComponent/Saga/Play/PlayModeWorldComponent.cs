using ModeFeature;
using System.Collections.Generic;
using UnityEngine;
using Saga;
using System.Linq;
using DG.Tweening;
using System.Collections;

namespace ModeComponent
{
    public class PlayModeWorldComponent : ModeBaseComponent
    {
        // 생성된 노드의 정보
        public readonly Dictionary<Vector3Int, Node> allNodes = new();

        private CellSO cellSO = null;
        private Transform bubbleParent = null;
        private ObjectBase map = null;

        public PlayModeWorldComponent(XComponent parent, Mode mode) : base(parent, mode)
        {

        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (!mode.dataMode.TryGetFeature(out SagaWorld world))
            {
                return;
            }

            cellSO = FakeAddressableManager.Instance.LoadSO<CellSO>(world.cellSo);
            if (cellSO == null)
            {
                Debug.Log($"CellSO is null {world.cellSo}");
                return;
            }

            foreach (var cell in cellSO.cells)
            {
                var newNode = new Node(cell.cellType, cell.coord, cell.group);
                allNodes.Add(newNode.coord, newNode);
                newNode.SetNeighbors(allNodes);
            }

            map = ObjectManager.Instance.Pop<ObjectBase>(world.mapPrefab);
            bubbleParent = new GameObject().transform;
        }

        public override void OnDisable()
        {
            foreach (var node in allNodes.Values)
            {
                node.OnDisable();
            }
            allNodes.Clear();

            map?.SetActive(false);
            map = null;

            GameObject.Destroy(bubbleParent.gameObject);
            bubbleParent = null;

            base.OnDisable();
        }

        public Node GetNode(Vector3Int coord)
        {
            if (!allNodes.TryGetValue(coord, out var node))
            {
                allNodes.Add(coord, node = new Node(coord));
                node.SetNeighbors(allNodes);
            }

            return node;
        }

        public Vector2 CellToWorld(Vector3Int coord)
        {
            return cellSO.CellToWorld(coord) + WorldOffset();
        }

        public Vector2 WorldOffset()
        {
            return bubbleParent.position;
        }

        public Transform GetBubbleParent()
        {
            return bubbleParent;
        }

        public IEnumerator CoMoveWorld()
        {
            var candidate = allNodes.Values.Where(x => !x.IsEmpty());
            if (!candidate.Any())
            {
                yield break;
            }

            int minY = candidate.Min(x => x.coord.y);
            float posY = Mathf.Max(0f, cellSO.grid.cellSize.y * minY * -1);
            if (bubbleParent.position.y == posY)
            {
                yield break;
            }

            var tween = bubbleParent.DOMoveY(posY, 1f);
            yield return new WaitUntil(() => !tween.IsActive() || !tween.IsPlaying());
        }

        /// <summary>
        /// 천장(기준점)으로부터 연결되어있는 노드 반환
        /// </summary>
        public HashSet<Node> GetConnectedNodesFromCeiling()
        {
            var result = new List<Node>();
            var tempList = new List<Node>();
            foreach (var node in allNodes.Values)
            {
                if (!node.IsCeiling())
                {
                    continue;
                }

                tempList.Clear();
                var nodes = Node.GetConnectedNodes(node, tempList,
                    x =>
                    {
                        if (x.IsEmpty())
                        {
                            return false;
                        }

                        if (result.Contains(x))
                        {
                            return false;
                        }

                        return true;
                    });

                result.AddRange(nodes);
            }

            return result.ToHashSet();
        }

        public HashSet<Node> GetFloatingNodes()
        {
            var result = new HashSet<Node>();
            var validNodes = GetConnectedNodesFromCeiling();
            foreach (var node in allNodes.Values)
            {
                if (node.IsEmpty())
                {
                    continue;
                }

                if (validNodes.Contains(node))
                {
                    continue;
                }

                result.Add(node);
            }

            return result;
        }
    }
}
