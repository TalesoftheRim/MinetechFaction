using UnityEngine;
using Verse;

namespace MinetechFaction
{
    public class MineTechSettings : ModSettings
    {
        public bool enableMountainBias = true;
        public int maxMountainDistance = 3;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableMountainBias, "enableMountainBias", true);
            Scribe_Values.Look(ref maxMountainDistance, "maxMountainDistance", 3);
        }
    }
}
