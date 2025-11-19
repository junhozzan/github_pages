
using ModeFeature;
using System;
using System.Collections.Generic;

namespace ModeComponent
{
    public class PlayBossModeMainComponent : ModeBaseComponent, PlayModeControlComponent.IResponser
    {
        private SagaBossSystem bossSystem = null;
        private PlayModeWorldComponent worldCom = null;
        private PlayModeControlComponent controlCom = null;
        private PlayBossModeUIComponent uiCom = null;

        private readonly List<ObjectBase> bosses = new();

        private int shotCount = 0;
        private int hp = 0;

        public PlayBossModeMainComponent(XComponent parent, Mode mode) : base(parent, mode)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            bossSystem = mode.dataMode.TryGetFeature(out SagaBossSystem v) ? v : null;
            worldCom = GetComponent<PlayModeWorldComponent>();
            uiCom = GetComponent<PlayBossModeUIComponent>();
            controlCom = GetComponent<PlayModeControlComponent>();
            controlCom.SetResponser(this);
        }

        public override void OnEnable()
        {
            base.OnEnable();

            foreach (var node in worldCom.allNodes.Values)
            {
                if (node.cellType != CellType.BOSS)
                {
                    continue;
                }
             
                var boss = ObjectManager.Instance.Pop<ObjectBase>(bossSystem.bossPrefab);
                boss.transform.position = worldCom.CellToWorld(node.coord);
                boss.transform.SetParent(worldCom.GetBubbleParent());

                bosses.Add(boss);
            }

            shotCount = 0;
            hp = bossSystem.bossHp;
            RefreshHp();
            RefreshShotCount();
        }

        public override void OnDisable()
        {
            foreach (var boss in bosses)
            {
                boss.SetActive(false);
            }
            bosses.Clear();

            base.OnDisable();
        }

        public bool IsClear()
        {
            return hp <= 0;
        }

        public bool IsFailure()
        {
            return shotCount >= bossSystem.limitShot;
        }

        void PlayModeControlComponent.IResponser.AddShotCount(int count)
        {
            shotCount += count;
            RefreshShotCount();
        }

        void PlayModeControlComponent.IResponser.TakeDamage(int damage)
        {
            hp = Math.Max(hp - damage, 0);
            RefreshHp();
        }

        private void RefreshHp()
        {
            uiCom.SetHpGauag(hp / (float)bossSystem.bossHp);
        }

        private void RefreshShotCount()
        {
            uiCom.SetLimitCount(Math.Max(bossSystem.limitShot - shotCount, 0));
        }

        void PlayModeControlComponent.IResponser.HandleResult()
        {
            if (IsClear())
            {
                uiCom.ShowResult("승리");
            }
            else if (IsFailure())
            {
                uiCom.ShowResult("패배");
            }
        }
    }
}
