using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NPSBiomes;

public class PlaceWorker_OnSteamVent : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        var possibleSteamVent = map.thingGrid.ThingAt(loc, ThingDefOf.TKKN_SteamVent);
        if (possibleSteamVent == null || possibleSteamVent.Position != loc)
        {
            return "TKKN_NPS_MustPlaceOnSteamVent".Translate();
        }

        return true;
    }

    public override bool ForceAllowPlaceOver(BuildableDef otherDef)
    {
        return otherDef == ThingDefOf.TKKN_SteamVent;
    }
    
    
    public override void DrawMouseAttachments(BuildableDef def)
    {
        List<Thing> list = Find.CurrentMap.listerThings.ThingsOfDef(ThingDefOf.TKKN_SteamVent);
        for (int i = 0; i < list.Count; i++)
        {
            TargetHighlighter.Highlight(list[i]);
        }
    }
    
}