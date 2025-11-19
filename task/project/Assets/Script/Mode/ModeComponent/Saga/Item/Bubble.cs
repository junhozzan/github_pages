using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saga
{
    public class Bubble
    {
        public readonly DataSagaBubble dataBubble = null;
        public ObjectBubble obj { get; private set; } = null;
        public Node node { get; private set; } = null;

        public Bubble(int id)
        {
            this.dataBubble = DataManager.Instance.saga.GetBubble(id);
        }

        public void OnDisable()
        {
            if (node != null)
            {
                node.bubble = null;
                node = null;
            }

            obj?.SetActive(false);
            obj = null;
        }

        public void FadeOut()
        {
            obj?.FadeOut();
            obj = null;
        }

        public void DropBubble()
        {
            obj?.DropOut();
            obj = null;
        }

        public void SetCollider(bool use)
        {
            obj?.SetCollider(use);
        }

        public void Spawn(Vector2 pos, Transform parent)
        {
            if (obj == null)
            {
                obj = ObjectManager.Instance.Pop<ObjectBubble>(dataBubble.prefab);
            }

            obj.SetBubble(this);
            obj.SetParent(parent);
            obj.transform.position = pos;
        }

        public void SetNode(Node newNode)
        {
            if (node != null)
            {
                node.bubble = null;
            }

            node = newNode;
            node.bubble = this;
        }

        public Tween DoMoveSpawn(List<Vector2> path, float speed, Transform parent)
        {
            if (obj == null)
            {
                obj = ObjectManager.Instance.Pop<ObjectBubble>(dataBubble.prefab);
            }

            obj.SetBubble(this);
            obj.SetParent(parent);
            obj.transform.position = path[0];

            var distance = TotalPathToDitance(path);
            var time = distance / speed;

            var arrPath = path.Select(x => (Vector3)x).ToArray();
            var tween = obj.transform.DOPath(arrPath, time).SetEase(Ease.Linear);
            tween.ForceInit();
            return tween;
        }

        private static float TotalPathToDitance(List<Vector2> path)
        {
            float result = 0f;
            for (int i = 0; i < path.Count -1; i++)
            {
                result += Vector2.Distance(path[i], path[i + 1]);
            }

            return result;
        }
    }
}