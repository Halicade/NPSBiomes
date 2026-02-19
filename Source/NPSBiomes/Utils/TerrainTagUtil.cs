using System.Collections.Generic;
using Verse;

namespace NPSBiomes;

public static class TerrainTagUtil
{

    public static readonly HashSet<TerrainDef> Lava = [];
    public static readonly HashSet<TerrainDef> SaltTerrains = [];


    public static void IntializeTerrainTags() {
        List<TerrainDef> allTerrains = DefDatabase<TerrainDef>.AllDefsListForReading;

        foreach (var terrain in allTerrains) {
            
            if (terrain.HasTag("Lava") || terrain.HasTag("TKKN_Lava")) {
                Lava.Add(terrain);
            }

            if (terrain.HasTag("Salt")) {
                SaltTerrains.Add(terrain);
            }
            
        }
    }
}