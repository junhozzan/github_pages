using DG.Tweening;
using ModeFeature;
using Saga;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModeComponent
{
    /// <summary>
    /// 스포너에서 버블 생성 담당.
    /// </summary>
    public class PlayModeSpawnerComponent : ModeBaseComponent
    {
        private PlayModeWorldComponent worldCom = null;

        private readonly List<Node> spawnerNodes = new();

        private bool isLoadBubbles = false;

        public PlayModeSpawnerComponent(XComponent parent, Mode mode) : base(parent, mode)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            worldCom = GetComponent<PlayModeWorldComponent>();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            SpawnSpawner();
            SpawnBubbles();
        }

        public override void OnDisable()
        {
            isLoadBubbles = false;

            foreach (var node in spawnerNodes)
            {
                node.OnDisable();
            }
            spawnerNodes.Clear();
            base.OnDisable();
        }

        private void SpawnSpawner()
        {
            if (!mode.dataMode.TryGetFeature(out SagaBubble item))
            {
                return;
            }

            foreach (var node in worldCom.allNodes.Values)
            {
                if (node.cellType != CellType.SPAWNER)
                {
                    continue;
                }

                var bubble = new Bubble(item.spawnerBubbleID);
                bubble.Spawn(worldCom.CellToWorld(node.coord), worldCom.GetBubbleParent());
                bubble.SetNode(node);
                bubble.SetCollider(true);

                spawnerNodes.Add(node);
            }
        }

        private void SpawnBubbles()
        {
            CoroutineManager.Instance.StartRoutine(CoSpawnBubbles);
        }

        public IEnumerator CoSpawnBubbles()
        {
            isLoadBubbles = true;

            List<int> routineIDs = new();
            foreach (var node in spawnerNodes)
            {
                // 루틴을 독립적으로 실행
                routineIDs.Add(CoroutineManager.Instance.StartRoutine(CoSpawnBubblesInternal(node)));
            }

            if (routineIDs.Count > 0)
            {
                yield return new WaitUntil(() => CoroutineManager.Instance.IsRunning(routineIDs));
            }

            isLoadBubbles = false;
        }

        private IEnumerator CoSpawnBubblesInternal(Node spawnerNode)
        {
            if (!mode.dataMode.TryGetFeature(out SagaBubble item))
            {
                yield break;
            }

            // 스포너로부터 마지막 노드까지 순서대로 반환
            List<Node> nodesFromSpawner = Node.GetConnectedNodes(spawnerNode, new List<Node>(), x => spawnerNode.group == x.group);
            List<Node> tempNodes = new();
            HashSet<Node> passNodes = new();

            // 역순으로 검색
            for (int i = nodesFromSpawner.Count - 1; i >= 0; --i)
            {
                var node = nodesFromSpawner[i];
                if (node.cellType != CellType.BUBBLE)
                {
                    continue;
                }

                tempNodes.Clear();
                List<Node> connectedNodes = Node.GetConnectedNodes(node, tempNodes, 
                    x => 
                    {
                        if (x.cellType != CellType.BUBBLE)
                        {
                            return false;
                        }

                        if (node.group != x.group)
                        {
                            return false;
                        }

                        if (passNodes.Contains(x))
                        {
                            return false;
                        }

                        return true;
                    });

                if (connectedNodes.Count == 0)
                {
                    break;
                }

                passNodes.Add(node);

                Bubble bubble;
                
                float spawnDistance = 0f;

                // 가장 앞줄에 있는 버블의 인덱스 찾기
                int firstBubbleIndex = connectedNodes.FindIndex(x => !x.IsEmpty());
                if (firstBubbleIndex != -1)
                {
                    var firstNode = connectedNodes[firstBubbleIndex];
                    bubble = firstNode.bubble;
                    connectedNodes.RemoveRange(firstBubbleIndex, connectedNodes.Count - firstBubbleIndex);
                    connectedNodes.Add(firstNode);
                }
                else
                {
                    spawnDistance = 1f;
                    bubble = new Bubble(item.GetRandomAppearBubbleID());
                    connectedNodes.Add(spawnerNode);
                }

                connectedNodes.Reverse();
                var movePath = connectedNodes.Select(x => worldCom.CellToWorld(x.coord)).ToList();
                var tween = bubble.DoMoveSpawn(movePath, 10f, worldCom.GetBubbleParent());
                bubble.SetNode(node);
                bubble.SetCollider(true);

                var pathLength = tween.PathLength();
                // 다음 버블이 나올 수 있는 거리가되면 다음으로 진행
                yield return new WaitUntil(() => !tween.IsActive() || !tween.IsPlaying() || tween.ElapsedPercentage() * pathLength >= spawnDistance);
                
            }
        }

        public bool IsLoadBubbles()
        {
            return isLoadBubbles;
        }
    }
}
