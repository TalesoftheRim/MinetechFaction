using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MinetechFaction
{
    public static class MineTechSettlementFinder
    {
        public static bool IsMineTechFaction(Faction faction)
        {
            if (faction?.def == null) return false;
            string defName = faction.def.defName;
            return defName == "MT_MinetechCivil" 
                || defName == "MT_MinetechRough" 
                || defName == "MT_MinetechFaction"
                || faction.def.categoryTag == "MT_Minetech";
        }

        public static bool TryFindMountainSettlementTile(Faction faction, out PlanetTile result, bool mustBeAutoChoosable, Predicate<PlanetTile> extraValidator)
        {
            result = PlanetTile.Invalid;
            int maxDistance = MineTechMod.Settings?.maxMountainDistance ?? 3;
            maxDistance = Mathf.Clamp(maxDistance, 1, 5);

            PlanetLayer layer = Find.WorldGrid.Surface;
            int tilesCount = layer.TilesCount;
            if (tilesCount <= 0) return false;

            // Search through candidate tiles, prioritizing tiles in or near mountains
            for (int attempt = 0; attempt < 800; attempt++)
            {
                int randomIndex = Rand.Range(0, tilesCount);
                Tile worldTile = layer[randomIndex];
                PlanetTile candidate = worldTile.tile;

                if (!IsValidSettlementCandidate(candidate, worldTile, faction, mustBeAutoChoosable, extraValidator))
                {
                    continue;
                }

                // Direct Mountainous or Large Hills (distance 0) - ideal placement
                if (worldTile.hilliness == Hilliness.Mountainous || worldTile.hilliness == Hilliness.LargeHills)
                {
                    result = candidate;
                    return true;
                }

                // Check proximity within maxDistance tiles to Mountainous or Impassable
                if (HasMountainWithinDistance(candidate, maxDistance))
                {
                    result = candidate;
                    return true;
                }
            }

            // Fallback: no suitable mountain tile found in candidate search
            return false;
        }

        private static bool IsValidSettlementCandidate(PlanetTile planetTile, Tile worldTile, Faction faction, bool mustBeAutoChoosable, Predicate<PlanetTile> extraValidator)
        {
            if (!planetTile.Valid) return false;

            if (worldTile.PrimaryBiome == null || !worldTile.PrimaryBiome.canBuildBase || !worldTile.PrimaryBiome.implemented) return false;

            if (mustBeAutoChoosable && !worldTile.PrimaryBiome.canAutoChoose) return false;

            // Settlement MUST NOT be placed on Impassable mountain
            if (worldTile.hilliness == Hilliness.Impassable) return false;
            if (Find.World.Impassable(planetTile)) return false;

            // Temperature check via settlement chance curve (matches vanilla logic)
            if (faction?.def?.minSettlementTemperatureChanceCurve != null)
            {
                float minTemp = GenTemperature.MinTemperatureAtTile(planetTile);
                if (faction.def.minSettlementTemperatureChanceCurve.Evaluate(minTemp) <= 0f)
                {
                    return false;
                }
            }

            // Vanilla settlement validation (distance from other settlements, etc.)
            if (!TileFinder.IsValidTileForNewSettlement(planetTile))
            {
                return false;
            }

            // Extra validator predicate passed by caller
            if (extraValidator != null && !extraValidator(planetTile))
            {
                return false;
            }

            return true;
        }

        private static bool HasMountainWithinDistance(PlanetTile startTile, int maxDistance)
        {
            if (maxDistance <= 0) return false;

            HashSet<int> visited = new HashSet<int> { startTile.tileId };
            Queue<PlanetTile> currentQueue = new Queue<PlanetTile>();
            currentQueue.Enqueue(startTile);

            List<PlanetTile> neighbors = new List<PlanetTile>();

            for (int depth = 1; depth <= maxDistance; depth++)
            {
                int levelSize = currentQueue.Count;
                for (int i = 0; i < levelSize; i++)
                {
                    PlanetTile current = currentQueue.Dequeue();
                    neighbors.Clear();
                    Find.WorldGrid.GetTileNeighbors(current, neighbors);

                    for (int j = 0; j < neighbors.Count; j++)
                    {
                        PlanetTile neighbor = neighbors[j];
                        if (visited.Add(neighbor.tileId))
                        {
                            Hilliness h = Find.WorldGrid[neighbor].hilliness;
                            if (h == Hilliness.Mountainous || h == Hilliness.Impassable)
                            {
                                return true;
                            }
                            currentQueue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return false;
        }
    }
}
