using System.Collections.Frozen;
using System.Collections.Generic;
using Verse;

namespace NPSBiomes;

public static class TerrainTagUtil
{
    
    private static readonly HashSet<TerrainDef> HashLava = [];
    private static readonly HashSet<TerrainDef> HashSalt = [];
    
    public static FrozenSet<TerrainDef> Lava = [];
    public static FrozenSet<TerrainDef> SaltTerrains = [];


    public static void IntializeTerrainTags() {
        List<TerrainDef> allTerrains = DefDatabase<TerrainDef>.AllDefsListForReading;

        foreach (var terrain in allTerrains) {
            
            if (terrain.HasTag("Lava") || terrain.HasTag("TKKN_Lava")) {
                HashLava.Add(terrain);
            }

            if (terrain.HasTag("Salt")) {
                HashSalt.Add(terrain);
            }
            
        }
        
        Lava = HashLava.ToFrozenSet();
        SaltTerrains = HashSalt.ToFrozenSet();
        // Don't need the original dicts
        HashLava.Clear();
        HashSalt.Clear();
    }
}