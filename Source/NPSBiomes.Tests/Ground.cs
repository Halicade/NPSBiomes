using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace NPSBiomes.Tests;

/// <summary>
/// Building blocks for the tests that use DevTools' test map. The fixtures of
/// one run share the map, so each works in its own corner, clearing it before
/// it builds and again when it finishes.
/// </summary>
internal static class Ground
{
    public static Map Map => Find.CurrentMap;

    public static ThingDef Def(string defName) => DefDatabase<ThingDef>.GetNamed(defName);

    public static TerrainDef Terrain(string defName) => DefDatabase<TerrainDef>.GetNamed(defName);

    /// <summary>Bare soil with nothing on it but pawns, no roof and no
    /// fog.</summary>
    public static void Clear(Map map, CellRect area) {
        foreach (IntVec3 c in area.ClipInsideMap(map)) {
            foreach (Thing thing in c.GetThingList(map).ToList()) {
                if (thing is not Pawn && !thing.Destroyed) {
                    thing.Destroy(DestroyMode.Vanish);
                }
            }
            map.terrainGrid.SetTerrain(c, RimWorld.TerrainDefOf.Soil);
            map.roofGrid.SetRoof(c, null);
            map.fogGrid.Unfog(c);
        }
    }

    /// <summary>A building of the player's, made of wood if it takes a
    /// material.</summary>
    public static Thing Build(ThingDef def, IntVec3 c, Map map) {
        Thing thing = ThingMaker.MakeThing(def, def.MadeFromStuff ? RimWorld.ThingDefOf.WoodLog : null);
        thing.SetFaction(Faction.OfPlayer);
        return GenSpawn.Spawn(thing, c, map);
    }

    /// <summary>A new colonist of the player's, fed and rested, who does no
    /// work of their own accord.</summary>
    public static Pawn SpawnColonist(Map map, IntVec3 c) {
        Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
        GenSpawn.Spawn(pawn, c, map);
        pawn.workSettings?.EnableAndInitializeIfNotAlreadyInitialized();
        pawn.workSettings?.DisableAll();
        foreach (Need need in new Need[] { pawn.needs?.food, pawn.needs?.rest, pawn.needs?.joy }) {
            if (need != null) {
                need.CurLevel = need.MaxLevel;
            }
        }
        return pawn;
    }

    public static void Remove(IEnumerable<Pawn> pawns) {
        foreach (Pawn pawn in pawns.ToList()) {
            if (pawn != null && !pawn.Destroyed) {
                pawn.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
