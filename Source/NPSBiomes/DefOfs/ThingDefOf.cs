using RimWorld;
using Verse;

namespace NPSBiomes;

[DefOf]
public static class ThingDefOf
{
    public static ThingDef TKKN_SteamVent;

    static ThingDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }
}