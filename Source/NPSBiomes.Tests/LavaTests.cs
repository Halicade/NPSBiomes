using System.Linq;
using System.Reflection;
using DevTools.Testing;
using RimWorld;
using Verse;

namespace NPSBiomes.Tests;

/// <summary>
/// The lava map component's per-cell check, which now and then turns lava
/// into basalt rock and puts a steam vent on rough-hewn basalt. Each test runs
/// the check once on one cell, with the random roll seeded so the check's
/// 1-in-100,000 chance comes up.
/// </summary>
[TestFixture(TestType.Playing)]
[WarningsAllowed(AllowedWarnings.EngineStub)]
internal class LavaTests
{
    private static readonly CellRect Area = new CellRect(40, 20, 12, 6);

    private static int passingSeed = -1;

    /// <summary>Clears the fixture's corner, and every steam vent on the map,
    /// since the check adds a vent only while the map has fewer than 10. The
    /// vents are not put back: the map belongs to DevTools' test game, which
    /// ends with the run.</summary>
    [OneTimeSetUp]
    private void ClearBefore() {
        Map map = Ground.Map;
        Ground.Clear(map, Area);
        foreach (Thing vent in map.listerThings.ThingsOfDef(Ground.Def("TKKN_SteamVent")).ToList()) {
            vent.Destroy(DestroyMode.Vanish);
        }
    }

    [OneTimeTearDown]
    private void ClearAfter() => Ground.Clear(Ground.Map, Area);

    /// <summary>A vent never appears under a building: a vent spawned there
    /// would heat the room it stands in whenever it sprays.</summary>
    [TestCase("Wall")]
    [TestCase("Stool")]
    private void NoVentUnderABuilding(string building) {
        Map map = Ground.Map;
        IntVec3 cell = new IntVec3(42, 0, 22);
        Ground.Clear(map, CellRect.SingleCell(cell));
        map.terrainGrid.SetTerrain(cell, Ground.Terrain("TKKN_LavaRock_RoughHewn"));
        Thing thing = Ground.Build(Ground.Def(building), cell, map);

        RunCheck(map, cell);

        Check.That(Vent(map, cell) == null, $"no steam vent appears under the {building.ToLower()}");
        Check.That(!thing.Destroyed, $"the {building.ToLower()} still stands");
        Ground.Clear(map, CellRect.SingleCell(cell));
    }

    /// <summary>The check's own case, so the test above is not passing
    /// because the check never spawns anything.</summary>
    [Test]
    private void VentOnBareBasalt() {
        Map map = Ground.Map;
        IntVec3 cell = new IntVec3(44, 0, 22);
        Ground.Clear(map, CellRect.SingleCell(cell));
        map.terrainGrid.SetTerrain(cell, Ground.Terrain("TKKN_LavaRock_RoughHewn"));

        RunCheck(map, cell);

        Check.That(Vent(map, cell) != null, "a steam vent appears on bare rough-hewn basalt");
        Ground.Clear(map, CellRect.SingleCell(cell));
    }

    /// <summary>Lava turning to rock does not destroy what lies on it.</summary>
    [Test]
    private void RockSparesAnItemOnLava() {
        Map map = Ground.Map;
        IntVec3 cell = new IntVec3(46, 0, 22);
        Ground.Clear(map, CellRect.SingleCell(cell));
        map.terrainGrid.SetTerrain(cell, Ground.Terrain("TKKN_Lava"));
        Thing steel = ThingMaker.MakeThing(RimWorld.ThingDefOf.Steel);
        steel.stackCount = 10;
        GenSpawn.Spawn(steel, cell, map);

        RunCheck(map, cell);

        Check.That(!steel.Destroyed, $"the 10 steel on the lava survive{(steel.Destroyed ? "" : $", at {steel.PositionHeld}")}");
        Ground.Clear(map, CellRect.SingleCell(cell));
    }

    /// <summary>The check's own case on lava.</summary>
    [Test]
    private void RockOnBareLava() {
        Map map = Ground.Map;
        IntVec3 cell = new IntVec3(48, 0, 22);
        Ground.Clear(map, CellRect.SingleCell(cell));
        map.terrainGrid.SetTerrain(cell, Ground.Terrain("TKKN_Lava"));

        RunCheck(map, cell);

        Check.That(cell.GetThingList(map).Any(t => t.def.defName == "TKKN_LavaRock"), "basalt rock appears on bare lava");
        Ground.Clear(map, CellRect.SingleCell(cell));
    }

    private static Thing Vent(Map map, IntVec3 cell) => cell.GetThingList(map).FirstOrDefault(t => t.def.defName == "TKKN_SteamVent");

    /// <summary>
    /// Runs LavaTile's private per-cell check once on <paramref name="cell"/>,
    /// under a random seed whose first roll passes its 1-in-100,000 chance.
    /// </summary>
    private static void RunCheck(Map map, IntVec3 cell) {
        if (passingSeed < 0) {
            for (int seed = 0; seed < 50_000_000; seed++) {
                Rand.PushState(seed);
                float roll = Rand.Value;
                Rand.PopState();
                if (roll < 0.00001f) {
                    passingSeed = seed;
                    break;
                }
            }
        }
        Check.That(passingSeed >= 0, $"a seed whose first roll passes the check ({passingSeed})");
        Check.That(map.listerThings.ThingsOfDef(Ground.Def("TKKN_SteamVent")).Count < 10, "the map has fewer than 10 vents, so the check may add one");
        LavaTile lavaTile = map.GetComponent<LavaTile>();
        MethodInfo check = typeof(LavaTile).GetMethod("LavaCellChecks", BindingFlags.NonPublic | BindingFlags.Instance);
        if (!Check.That(check != null, "LavaTile has a private LavaCellChecks(IntVec3) for this test to call")) {
            return;
        }
        Rand.PushState(passingSeed);
        try {
            check.Invoke(lavaTile, new object[] { cell });
        }
        finally {
            Rand.PopState();
        }
    }
}
