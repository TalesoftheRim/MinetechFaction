using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MinetechFaction
{
    [HarmonyPatch(typeof(TileFinder), "RandomSettlementTileFor", new Type[] { typeof(Faction), typeof(bool), typeof(Predicate<PlanetTile>)})]
    public static class Patch_TileFinder_RandomSettlementTileFor
    {
        [HarmonyPrefix]
        public static bool Prefix(Faction faction, ref PlanetTile __result, bool mustBeAutoChoosable = false, Predicate<PlanetTile> extraValidator = null)
        {
            try
            {
                if (faction == null) return true;
                if (MineTechMod.Settings == null || !MineTechMod.Settings.enableMountainBias) return true;

                if (!MineTechSettlementFinder.IsMineTechFaction(faction))
                {
                    return true;
                }

                if (MineTechSettlementFinder.TryFindMountainSettlementTile(faction, out PlanetTile chosenTile, mustBeAutoChoosable, extraValidator))
                {
                    __result = chosenTile;
                    return false; // Skip vanilla placement, use our mountain-biased tile
                }
            }
            catch (Exception ex)
            {
                Log.Warning("[MineTech] Exception in mountain settlement generation patch: " + ex.Message + ". Falling back to vanilla.");
            }

            return true; // Fallback to vanilla
        }
    }
}
