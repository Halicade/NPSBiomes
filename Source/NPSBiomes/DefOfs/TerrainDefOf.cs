using RimWorld;
using Verse;

namespace NPSBiomes;

[DefOf]
public static class TerrainDefOf
{
    
    public static TerrainDef TKKN_Lava;
    public static TerrainDef TKKN_LavaDeep;
    public static TerrainDef TKKN_LavaRock_RoughHewn;

    static TerrainDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(TerrainDefOf));
    }
}