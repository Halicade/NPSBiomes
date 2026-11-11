using Verse;

namespace NPSBiomes;

public class LavaTile(Map map) : MapComponent(map)
{

    private bool anyLavaTerrain = false;
    private int cycleIndex = 0;

    public override void FinalizeInit() {
        base.FinalizeInit();
        foreach (var mapCell in map.AllCells) {
            var cellTerrain = mapCell.GetTerrain(map);

            if (cellTerrain == TerrainDefOf.TKKN_Lava || cellTerrain == TerrainDefOf.TKKN_LavaRock_RoughHewn || cellTerrain == RimWorld.TerrainDefOf.LavaShallow) {
                anyLavaTerrain = true;
                break;
            }
        }
    }

    public override void MapComponentTick() {
        base.MapComponentTick();
        if (!anyLavaTerrain || !BiomeSettings.SteamVentsRespawn) {
            return;
        }

        int area = map.Area;
        for (int i = 0; i < 5; i++) {
            if (cycleIndex >= area) {
                cycleIndex = 0;
            }
            IntVec3 c = map.cellsInRandomOrder.Get(cycleIndex);
            LavaCellChecks(c);
            cycleIndex++;
        }

    }

    private void LavaCellChecks(IntVec3 activeCell) {

        if (Rand.Value < 0.00001f) {

            var currentTerrain = activeCell.GetTerrain(map);
            if (currentTerrain == TerrainDefOf.TKKN_Lava || currentTerrain == RimWorld.TerrainDefOf.LavaShallow) {
                var thing = ThingMaker.MakeThing(ThingDefOf.TKKN_LavaRock);
                GenSpawn.Spawn(thing, activeCell, map);
                return;
            }

            if (currentTerrain == TerrainDefOf.TKKN_LavaRock_RoughHewn &&
                map.listerThings.ThingsOfDef(ThingDefOf.TKKN_SteamVent).Count < 10 &&
                activeCell.GetEdifice(map) == null &&
                activeCell.InNoBuildEdgeArea(map) &&
                activeCell.GetFirstBuilding(map) == null) {

                var thing = ThingMaker.MakeThing(ThingDefOf.TKKN_SteamVent);
                GenSpawn.Spawn(thing, activeCell, map);
            }
        }
    }

}