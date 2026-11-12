using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevTools.Testing;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace NPSBiomes.Tests;

/// <summary>
/// Which biome a world tile gets. World generation asks every biome's worker
/// to score a tile and gives it the first biome with the highest score. These
/// tests make that pick twice for the same tile: once from the official
/// biomes alone, scored as the game scores them without this mod (vanilla's
/// pick), and once from the official biomes and this mod's, scored as they
/// score with this mod loaded. Other mods' biomes are left out of both.
///
/// The tiles come from two places: a grid of climates (temperature, rainfall,
/// terrain and swampiness), and every land tile of the world DevTools
/// generates for its test game. The grid is the same every run; the world is
/// new each run, and some biomes need the world, because their workers read
/// noise at the tile's place on the planet.
///
/// Each test starts from the settings a new game has before any settings
/// file is read, so a player's own choices do not change the result, and puts
/// the player's settings back after.
/// </summary>
[TestFixture(TestType.Playing)]
[WarningsAllowed(AllowedWarnings.EngineStub)]
internal class BiomeTests
{
    private Dictionary<FieldInfo, object> playerSettings;

    [SetUp]
    private void StartFromNewGameSettings() {
        playerSettings = SettingsFile.Snapshot();
        SettingsFile.Restore(SettingsFile.StartingValues());
    }

    [TearDown]
    private void PutBackPlayerSettings() => SettingsFile.Restore(playerSettings);

    /// <summary>Odyssey's rarer biomes, each scored within vanilla's range
    /// and so close to the biome it replaces.</summary>
    private static readonly string[] OdysseyRare = { "Glowforest", "Scarlands", "LavaField", "GlacialPlain" };

    /// <summary>
    /// Workers whose score depends on where the tile is on the planet, not
    /// only on its climate: Odyssey's noise maps and this mod's seeded
    /// chances. The climate grid leaves them out.
    /// </summary>
    private static readonly Type[] PlaceDependent = {
        typeof(BiomeWorker_GlacialPlain), typeof(BiomeWorker_LavaField), typeof(BiomeWorker_Scarlands),
        typeof(NPS_BiomeWorker_Desert), typeof(NPS_BiomeWorker_Oasis), typeof(NPS_BiomeWorker_LavaFields),
    };

    /// <summary>
    /// Where this mod changes vanilla's pick, the tile goes to one of this
    /// mod's biomes. A patch that takes score away from a vanilla biome
    /// without one of this mod's biomes taking the tile hands the tile to
    /// some other vanilla biome instead: boreal forest in a warm band, say.
    /// </summary>
    [Test]
    private void OnClimateGridOnlyThisModsBiomesTakeGround() {
        Report(Shifts(ClimateGrid(), placeDependent: false), shift => IsOurs(shift.to),
            "of the climate grid's points");
    }

    [Test]
    private void OnTheWorldOnlyThisModsBiomesTakeGround() {
        Report(Shifts(WorldTiles(), placeDependent: true), shift => IsOurs(shift.to),
            "of the world's land tiles");
    }

    /// <summary>
    /// Odyssey's rarer biomes keep the tiles vanilla gives them. This mod's
    /// biomes score well above vanilla's range, so wherever their window
    /// covers one of these tiles they would otherwise take it.
    /// </summary>
    /// <remarks>Of the four, only glowforest's worker reads nothing but the
    /// climate, so the grid covers glowforest alone. The other three read
    /// noise at the tile's place on the planet, and DevTools' test world
    /// covers 5% of the planet: too few of their tiles to fail on.</remarks>
    [Test]
    private void OnClimateGridOdysseyKeepsItsRareBiomes() {
        if (!ModsConfig.OdysseyActive) {
            Check.Skip("Odyssey is not active");
            return;
        }
        const string of = "of the climate grid's points";
        List<SurfaceTile> tiles = ClimateGrid().ToList();
        List<BiomeDef> official = Eligible(placeDependent: false).Where(Official).ToList();
        List<BiomeDef> rares = official.Where(d => OdysseyRare.Contains(d.defName)).ToList();
        if (!Check.That(rares.Count > 0, $"the grid scores at least one of {string.Join(", ", OdysseyRare)}")) {
            return;
        }
        foreach (BiomeDef rare in rares) {
            int count = tiles.Count(t => Pick(official, t, VanillaScore) == rare);
            Check.That(count > 0, $"{rare.defName} is vanilla's pick on {count} {of}, so the grid can test it");
        }
        Report(Shifts(tiles, placeDependent: false).Where(s => OdysseyRare.Contains(s.from.defName)).ToList(), _ => false, of);
    }

    /// <summary>
    /// The redwood forest's "max rainfall" setting keeps redwood off tiles
    /// wetter than it, and lets redwood onto tiles under it.
    /// </summary>
    [Test]
    private void RedwoodMaxRainfallIsAMaximum() {
        BiomeDef redwood = DefDatabase<BiomeDef>.GetNamed("TKKN_RedwoodForest");
        SurfaceTile tile = Tile(0f, 1500f, Hilliness.Flat, 0f, AnyLandTile());
        BiomeSettings.RedwoodMaxRainfall = 1100f;
        float wetter = redwood.Worker.GetScore(redwood, tile, tile.tile);
        Check.That(wetter <= 0f, $"max rainfall 1100 mm: redwood scores {wetter} on a 0 °C tile with 1500 mm (should be 0, the tile is wetter than the maximum)");
        BiomeSettings.RedwoodMaxRainfall = 2000f;
        float under = redwood.Worker.GetScore(redwood, tile, tile.tile);
        Check.That(under > 0f, $"max rainfall 2000 mm: redwood scores {under} on the same tile (should be above 0, the tile is under the maximum)");
    }

    // ---- picking --------------------------------------------------------

    private sealed class Shift
    {
        public BiomeDef from;
        public BiomeDef to;
        public int count;
        public float coldest = float.MaxValue;
        public float warmest = float.MinValue;
        public string example;
    }

    /// <summary>Every tile whose pick with this mod differs from vanilla's,
    /// grouped by vanilla's pick and this mod's.</summary>
    private static List<Shift> Shifts(IEnumerable<SurfaceTile> tiles, bool placeDependent) {
        List<BiomeDef> withUs = Eligible(placeDependent).Where(d => Official(d) || IsOurs(d)).ToList();
        List<BiomeDef> official = withUs.Where(Official).ToList();
        var shifts = new Dictionary<(BiomeDef, BiomeDef), Shift>();
        foreach (SurfaceTile tile in tiles) {
            BiomeDef vanilla = Pick(official, tile, VanillaScore);
            BiomeDef ours = Pick(withUs, tile, (d, t) => d.Worker.GetScore(d, t, t.tile));
            if (vanilla == ours) {
                continue;
            }
            if (!shifts.TryGetValue((vanilla, ours), out Shift shift)) {
                shift = new Shift { from = vanilla, to = ours, example = Describe(tile) };
                shifts[(vanilla, ours)] = shift;
            }
            shift.count++;
            shift.coldest = Math.Min(shift.coldest, tile.temperature);
            shift.warmest = Math.Max(shift.warmest, tile.temperature);
        }
        return shifts.Values.OrderByDescending(s => s.count).ToList();
    }

    /// <summary>One line per shift: a pass where <paramref name="allowed"/>
    /// holds, a failure where it does not.</summary>
    private static void Report(List<Shift> shifts, Func<Shift, bool> allowed, string of) {
        int failed = 0;
        foreach (Shift shift in shifts) {
            bool ok = allowed(shift);
            if (!ok) {
                failed++;
            }
            Check.That(ok, $"{shift.from.defName} becomes {shift.to.defName} on {shift.count} {of}, " +
                           $"{shift.coldest:F0} to {shift.warmest:F0} °C, e.g. {shift.example}");
        }
        if (failed == 0) {
            Check.Note($"{shifts.Count} kinds of change from vanilla's pick here, none of them disallowed");
        }
    }

    /// <summary>The same pick world generation makes: the first biome in
    /// def order with the highest score.</summary>
    private static BiomeDef Pick(List<BiomeDef> defs, SurfaceTile tile, Func<BiomeDef, SurfaceTile, float> score) {
        BiomeDef best = null;
        float top = 0f;
        foreach (BiomeDef def in defs) {
            float s = score(def, tile);
            if (best == null || s > top) {
                best = def;
                top = s;
            }
        }
        return best;
    }

    /// <summary>
    /// The score vanilla gives. This mod patches the temperate forest and
    /// arid shrubland workers; their own static helpers, which the patches
    /// leave alone, give the score without the patch. A patch on any other
    /// vanilla worker would count on both sides and go untested, so it needs
    /// a case here too.
    /// </summary>
    private static float VanillaScore(BiomeDef def, SurfaceTile tile) {
        if (def.workerClass == typeof(BiomeWorker_TemperateForest)) {
            return tile.WaterCovered ? -100f : BiomeWorker_TemperateForest.Allowed(tile) ? BiomeWorker_TemperateForest.Score(tile) : 0f;
        }
        if (def.workerClass == typeof(BiomeWorker_AridShrubland)) {
            return tile.WaterCovered ? -100f : BiomeWorker_AridShrubland.Allowed(tile) ? BiomeWorker_AridShrubland.Score(tile) : 0f;
        }
        return def.Worker.GetScore(def, tile, tile.tile);
    }

    private static List<BiomeDef> Eligible(bool placeDependent) {
        return DefDatabase<BiomeDef>.AllDefsListForReading
            .Where(d => d.implemented && d.generatesNaturally && d.Worker.CanPlaceOnLayer(d, Find.WorldGrid.Surface))
            .Where(d => placeDependent || !PlaceDependent.Contains(d.workerClass))
            .ToList();
    }

    private static bool Official(BiomeDef def) => def.modContentPack != null && def.modContentPack.IsOfficialMod;

    private static bool IsOurs(BiomeDef def) => def.modContentPack == LoadedModManager.GetMod<BiomesController>().Content;

    // ---- tiles ----------------------------------------------------------

    private static readonly Hilliness[] Terrains = { Hilliness.Flat, Hilliness.SmallHills, Hilliness.LargeHills, Hilliness.Mountainous };

    /// <summary>
    /// Land tiles from -40 to 45 °C a degree apart, 0 to 5000 mm of rain
    /// 50 mm apart, each terrain from flat to mountainous, dry and swampy.
    /// </summary>
    private static IEnumerable<SurfaceTile> ClimateGrid() {
        PlanetTile place = AnyLandTile();
        for (int temperature = -40; temperature <= 45; temperature++) {
            for (int rainfall = 0; rainfall <= 5000; rainfall += 50) {
                foreach (Hilliness terrain in Terrains) {
                    foreach (float swampiness in new[] { 0f, 0.7f }) {
                        yield return Tile(temperature, rainfall, terrain, swampiness, place);
                    }
                }
            }
        }
    }

    private static IEnumerable<SurfaceTile> WorldTiles() {
        return Find.WorldGrid.Tiles.Where(t => !t.WaterCovered);
    }

    private static PlanetTile AnyLandTile() => WorldTiles().First().tile;

    private static SurfaceTile Tile(float temperature, float rainfall, Hilliness terrain, float swampiness, PlanetTile place) {
        return new SurfaceTile {
            temperature = temperature, rainfall = rainfall, hilliness = terrain,
            swampiness = swampiness, elevation = 100f, tile = place,
        };
    }

    private static string Describe(SurfaceTile tile) {
        string swamp = tile.swampiness > 0f ? $", swampiness {tile.swampiness:F1}" : "";
        return $"{tile.temperature:F1} °C, {tile.rainfall:F0} mm, {tile.hilliness}{swamp}";
    }
}
