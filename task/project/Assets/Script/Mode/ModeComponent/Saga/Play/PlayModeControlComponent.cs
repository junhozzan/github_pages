using DG.Tweening;
using ModeFeature;
using Saga;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModeComponent
{
    public class PlayModeControlComponent : ModeBaseComponent, IXUpdate
    {
        public interface IResponser
        {
            void AddShotCount(int count);
            void TakeDamage(int damage);
            void HandleResult();
        }

        private PlayModeWorldComponent worldCom = null;
        private PlayModeSpawnerComponent spawnerCom = null;
        private ModeInputComponent inputCom = null;

        private bool canControl = true;
        private bool isTouched = false;

        private Vector3Int? shotCoord = null;
        private readonly List<Vector2> shotPath = new();
        private readonly List<Bubble> reservedBubbles = new();

        private readonly List<Vector3Int> shadowBubbleCoords = new();
        private readonly List<ObjectBase> shadowBubbles = new();
        private ObjectLineRenderer rayRenderer = null;


        private IResponser responser = null;

        private static readonly Vector2[] reservedBubblePositions = new Vector2[3]
        {
            new Vector2(0f, -6f),
            new Vector2(1f, -7.7f),
            new Vector2(-1f, -7.7f),
        };

        public PlayModeControlComponent(XComponent parent, Mode mode) : base(parent, mode)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            worldCom = GetComponent<PlayModeWorldComponent>();
            spawnerCom = GetComponent<PlayModeSpawnerComponent>();
            inputCom = GetComponent<ModeInputComponent>();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            ReserveShotBubbles();
        }

        public override void OnDisable()
        {
            canControl = true;
            isTouched = false;
            shotCoord = null;

            foreach (var bubble in reservedBubbles)
            {
                bubble.OnDisable();
            }
            reservedBubbles.Clear();

            base.OnDisable();
        }

        public void SetResponser(IResponser responser)
        {
            this.responser = responser;
        }

        void IXUpdate.XUpdate(float dt)
        {
            UpdateShooterControl();
        }

        private void UpdateShooterControl()
        {
            if (!CanControl())
            {
                return;
            }

            if (!isTouched)
            {
                if (inputCom.IsTouchDown(out Vector2 pos) && !inputCom.IsTouchedUI())
                {
                    isTouched = true;
                }
            }
            else
            {
                if (inputCom.IsDragging(out Vector2 pos))
                {
                    Vector2 rayPos = reservedBubblePositions[0];
                    Vector2 dir = (pos - rayPos).normalized;

                    // 레이 최소 기울기값 설정
                    if (Vector2.Dot(dir, Vector2.up) >= Mathf.Cos(80 * Mathf.Deg2Rad))
                    {
                        shotPath.Clear();
                        shotPath.Add(rayPos);

                        // 최소한의 벽 반사 횟수 5
                        for (int i = 0; i < 5; ++i)
                        {
                            var hit = Physics2D.Raycast(rayPos, dir, 20f, LayerMask.GetMask("Hit"));
                            if (hit.collider != null)
                            {
                                shotPath.Add(hit.point);

                                if (hit.collider.TryGetComponent(out ObjectBubble objBubble))
                                {
                                    var validNode = GetRayEmptyNeighbor(hit.point, objBubble.bubble.node);
                                    if (validNode == null)
                                    {
                                        Debug.Log("Node is null");
                                        continue;
                                    }

                                    shotCoord = validNode.coord;
                                    break;
                                }
                                else
                                {
                                    // 충돌된 객체에 다시 충돌하지 않도록 살짝 띄운다.
                                    rayPos = hit.point - dir * 0.01f;
                                    dir = Vector2.Reflect(dir, hit.normal);
                                }
                            }
                            else
                            {
                                // 화면 밖으로 넘어갔을때.
                                shotPath.Add(rayPos + dir * 20);
                                shotCoord = null;
                                break;
                            }
                        }
                    }

                    ShowLayRenderer();
                    ShowShadowBubble();
                }
                else if (inputCom.IsTouchUp(out _))
                {
                    Shot();
                    HideController();
                    isTouched = false;
                    shotCoord = null;
                    shotPath.Clear();
                }
            }
        }

        public Node GetRayEmptyNeighbor(Vector2 rayPos, Node node)
        {
            Node result = null;
            var neighborCoords = node.GetNeighborCoords();
            float distance = float.MaxValue;
            foreach (var neighborCoord in neighborCoords)
            {
                var _node = worldCom.GetNode(node.coord + neighborCoord);
                if (!_node.IsEmpty())
                {
                    continue;
                }

                var nodePos = worldCom.CellToWorld(_node.coord);
                var sqrDistance = (rayPos - nodePos).sqrMagnitude;
                if (sqrDistance >= distance)
                {
                    continue;
                }

                distance = sqrDistance;
                result = _node;
            }

            return result;
        }

        private void ShowShadowBubble()
        {
            foreach (var obj in shadowBubbles)
            {
                obj.SetActive(false);
            }

            shadowBubbles.Clear();
            shadowBubbleCoords.Clear();

            if (!shotCoord.HasValue)
            {
                return;
            }

            shadowBubbleCoords.Clear();

            var targetCoord = shotCoord.Value;
            shadowBubbleCoords.Add(targetCoord);

            var bubble = reservedBubbles[0];
            if (!string.IsNullOrEmpty(bubble.dataBubble.explosionSO))
            {
                var so = FakeAddressableManager.Instance.LoadSO<ExplosionSO>(bubble.dataBubble.explosionSO);
                if (so != null)
                {
                    var coords = (targetCoord.y % 2 == 0) ? so.evenCoords : so.oddCoords;
                    foreach (var coord in coords)
                    {
                        shadowBubbleCoords.Add(coord + targetCoord);
                    }
                }
            }

            for (int i = 0; i < shadowBubbleCoords.Count; ++i)
            {
                var obj = ObjectManager.Instance.Pop<ObjectBase>(SagaGameData.Instance.shadowPrefab);
                obj.transform.position = worldCom.CellToWorld(shadowBubbleCoords[i]);

                shadowBubbles.Add(obj);
            }
        }

        private void ShowLayRenderer()
        {
            if (shotPath.Count == 0)
            {
                rayRenderer?.SetActive(false);
                rayRenderer = null;
                return;
            }
            
            if (rayRenderer == null)
            {
                rayRenderer = ObjectManager.Instance.Pop<ObjectLineRenderer>("pf_object_linerenderer");
            }

            rayRenderer.SetPositions(shotPath);
        }

        private void HideController()
        {
            foreach (var obj in shadowBubbles)
            {
                obj.SetActive(false);
            }

            shadowBubbles.Clear();
            shadowBubbleCoords.Clear();

            rayRenderer?.SetActive(false);
            rayRenderer = null;
        }

        private void Shot()
        {
            if (!shotCoord.HasValue)
            {
                return;
            }

            CoroutineManager.Instance.StartRoutine(CoShot);
        }

        private IEnumerator CoShot()
        {
            canControl = false;

            var coord = shotCoord.Value;
            var node = worldCom.GetNode(coord);

            // 노드 위치로 보정
            shotPath[^1] = worldCom.CellToWorld(coord);

            var bubble = reservedBubbles[0];
            reservedBubbles.RemoveAt(0);

            var tween = bubble.DoMoveSpawn(shotPath, 100f, worldCom.GetBubbleParent());
            bubble.SetNode(node);
            bubble.SetCollider(true);
            yield return new WaitUntil(() => !tween.IsActive() || !tween.IsPlaying());

            List<int> routineIDs = new();
            if (TryMatch3(node, out var match3Nodes))
            {
                routineIDs.Add(CoroutineManager.Instance.StartRoutine(CoPopMatch3(match3Nodes)));
            }

            if (TryStepMine(node, out var mineNodes))
            {
                routineIDs.Add(CoroutineManager.Instance.StartRoutine(CoPopMine(mineNodes)));
            }

            if (routineIDs.Count > 0)
            {
                yield return new WaitUntil(() => CoroutineManager.Instance.IsRunning(routineIDs));
                yield return CoRemoveFloatingBubbles();
                yield return spawnerCom.CoSpawnBubbles();
            }

            yield return worldCom.CoMoveWorld();

            ReserveShotBubbles();

            // 모든 처리가 완료된 후 카운트 증가.
            responser?.AddShotCount(1);
            responser?.HandleResult();

            canControl = true;
        }

        private bool TryMatch3(Node node, out List<Node> nodes)
        {
            if (node.IsEmpty())
            {
                nodes = null;
                return false;
            }

            nodes = Node.GetConnectedNodes(node, new List<Node>(), 
                x => 
                {
                    if (x.IsEmpty())
                    {
                        return false;
                    }

                    return node.bubble.dataBubble.matchIDs.Contains(x.bubble.dataBubble.id);
                });

            return nodes.Count >= node.bubble.dataBubble.minMatchCount;
        }

        private bool TryStepMine(Node node, out List<Node> nodes)
        {
            nodes = new();
            foreach (var neighbor in node.neighbors)
            {
                if (!neighbor.IsEmpty() && neighbor.bubble.dataBubble.isMine)
                {
                    nodes.Add(neighbor);
                }
            }

            return nodes.Count > 0;
        }

        private IEnumerator CoPopMatch3(List<Node> nodes)
        {
            foreach (var node in nodes)
            {
                yield return PopBubble(node.bubble);
            }
        }

        private IEnumerator CoPopMine(List<Node> nodes)
        {
            List<int> ids = new();
            foreach (var node in nodes)
            {
                ids.Add(CoroutineManager.Instance.StartRoutine(PopBubble(node.bubble)));
            }

            yield return new WaitUntil(() => CoroutineManager.Instance.IsRunning(ids));
        }

        private IEnumerator PopBubble(Bubble bubble)
        {
            var data = bubble.dataBubble;
            var coord = bubble.node.coord;
            if (!string.IsNullOrEmpty(data.popEffect))
            {
                var obj = ObjectManager.Instance.Pop<ObjectBase>(data.popEffect);
                obj.transform.position = worldCom.CellToWorld(coord);
            }

            if (data.damage > 0)
            {
                responser?.TakeDamage(data.damage);
            }

            if (!string.IsNullOrEmpty(data.explosionSO))
            {
                // 폭발 딜레이
                yield return CoroutineManager.Instance.GetWaitForSeconds(0.04f);

                var so = FakeAddressableManager.Instance.LoadSO<ExplosionSO>(data.explosionSO);
                bubble.OnDisable();

                var coords = (coord.y % 2 == 0) ? so.evenCoords : so.oddCoords;
                foreach (var _coord in coords)
                {
                    var targetNode = worldCom.GetNode(coord + _coord);
                    if (targetNode.IsEmpty())
                    {
                        continue;
                    }

                    yield return PopBubble(targetNode.bubble);
                }
            }
            else
            {
                bubble.FadeOut();
                bubble.OnDisable();
            }

            yield return worldCom.CoMoveWorld();
        }


        private IEnumerator CoRemoveFloatingBubbles()
        {
            var floatingNodes = worldCom.GetFloatingNodes();
            foreach (var node in floatingNodes)
            {
                node.bubble.DropBubble();
                node.bubble.OnDisable();
            }

            yield break;
        }

        private void ReserveShotBubbles()
        {
            if (!mode.dataMode.TryGetFeature(out SagaBubble item))
            {
                return;
            }

            for (int i = reservedBubbles.Count; i < 2; ++i)
            {
                var bubble = new Bubble(item.GetRandomShotBubbleID());
                reservedBubbles.Add(bubble);
            }

            for (int i = 0; i < reservedBubbles.Count; ++i)
            {
                var bubble = reservedBubbles[i];
                bubble.Spawn(reservedBubblePositions[i], null);

                // 발사체는 Raycast 미감지를 위해 콜라이더를 비활성화 한다.
                bubble.SetCollider(false);
            }
        }

        private bool CanControl()
        {
            return canControl && !spawnerCom.IsLoadBubbles();
        }
    }
}
