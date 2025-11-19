using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModeFeature
{
    public class SagaBubble : ModeFeatureBase
    {
        public readonly List<int> shotBubbleIDs;
        public readonly List<int> appearBubbleIDs;
        public readonly int spawnerBubbleID;

        public SagaBubble(TXMLElement e) : base(e)
        {
            this.shotBubbleIDs = e.GetChildren("ShotBubbleID").Select(x => int.Parse(x.text)).ToList();
            this.appearBubbleIDs = e.GetChildren("AppearBubbleID").Select(x => int.Parse(x.text)).ToList();
            this.spawnerBubbleID = e.GetChildInt("SpawnerBubbleID");
        }

        public int GetRandomShotBubbleID()
        {
            return shotBubbleIDs[Random.Range(0, shotBubbleIDs.Count)];
        }

        public int GetRandomAppearBubbleID()
        {
            return appearBubbleIDs[Random.Range(0, appearBubbleIDs.Count)];
        }

        public override void Verify(DataManager manager)
        {
            base.Verify(manager);

            List<int> tempBubbleIDs = new();
            tempBubbleIDs.AddRange(shotBubbleIDs);
            tempBubbleIDs.AddRange(appearBubbleIDs);
            tempBubbleIDs.Add(spawnerBubbleID);

            foreach (var bubbleID in tempBubbleIDs)
            {
                if (manager.saga.GetBubble(bubbleID) == null)
                {
                    Debug.Log($"DataSataBubble is null ({bubbleID})");
                }
            }
        }
    }
}
