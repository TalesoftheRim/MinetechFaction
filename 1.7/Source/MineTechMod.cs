using HarmonyLib;
using UnityEngine;
using Verse;

namespace MinetechFaction
{
    public class MineTechMod : Mod
    {
        public static MineTechSettings Settings;

        public MineTechMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<MineTechSettings>();
            var harmony = new Harmony("Talesoftherim.Minetech.Faction");
            harmony.PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled(
                "MT_Setting_EnableMountainBias".Translate(),
                ref Settings.enableMountainBias,
                "MT_Setting_EnableMountainBias_Desc".Translate()
            );

            if (Settings.enableMountainBias)
            {
                listingStandard.Gap(12f);
                string distanceLabel = "MT_Setting_MaxMountainDistance".Translate();
                listingStandard.Label(string.Format(distanceLabel, Settings.maxMountainDistance));
                Settings.maxMountainDistance = Mathf.RoundToInt(listingStandard.Slider(Settings.maxMountainDistance, 1f, 5f));
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "MineTech - Faction";
        }
    }
}
