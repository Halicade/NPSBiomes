using RimWorld;
using Verse;

namespace NPSBiomes;

[DefOf]
public static class ThingDefOf
{
    public static ThingDef TKKN_SteamVent;
    public static ThingDef TKKN_LavaRock;

    static ThingDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }
}