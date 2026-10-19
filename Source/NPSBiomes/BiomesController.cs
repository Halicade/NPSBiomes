using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace NPSBiomes;

public class BiomesController : Mod
{
    public BiomesController(ModContentPack content)
        : base(content) {
        GetSettings<BiomeSettings>();
        BiomeSettings.MedievalOverhaulActive = ModLister.GetActiveModWithIdentifier("dankpyon.medieval.overhaul") != null;

        LongEventHandler.QueueLongEvent(action: HarmonyPatches,
            textKey: null,
            doAsynchronously: true,
            exceptionHandler: null
        );
    }

    public override void DoSettingsWindowContents(Rect inRect) {
        DoWindowContents(inRect);
    }

    public override string SettingsCategory() {
        return "NPS_Biomes".Translate();
    }

    private static void HarmonyPatches() {
        TerrainTagUtil.IntializeTerrainTags();

        var harmony = new Harmony("Hali.NPS_BiomeEffects");
        harmony.PatchAll();

    }


    private Vector2 scrollPosition = Vector2.zero;

    private void DoWindowContents(Rect inRect) {
        Listing_Standard list = new Listing_Standard(GameFont.Small);
        string oasisSpawnChanceBuff = BiomeSettings.OasisSpawnChance.ToString();
        string saltFlatSpawnChanceBuff = BiomeSettings.SaltFlatSpawnChance.ToString();
        string redwoodMinTempBuff = BiomeSettings.RedwoodMinTemp.ToString();
        string redwoodMaxTempBuff = BiomeSettings.RedwoodMaxTemp.ToString();
        string redwoodMaxRainfallBuff = BiomeSettings.RedwoodMaxRainfall.ToString();
        string savannaMinTempBuff = BiomeSettings.SavannaMinTemp.ToString();
        string savannaMaxTempBuff = BiomeSettings.SavannaMaxTemp.ToString();
        string savannaMinRainfallBuff = BiomeSettings.SavannaMinRainfall.ToString();
        string savannaMaxRainfallBuff = BiomeSettings.SavannaMaxRainfall.ToString();
        string tallgrassMinTempBuff = BiomeSettings.TallgrassMinTemp.ToString();
        string tallgrassMaxTempBuff = BiomeSettings.TallgrassMaxTemp.ToString();
        string tallgrassMinRainfallBuff = BiomeSettings.TallgrassMinRainfall.ToString();
        string tallgrassMaxRainfallBuff = BiomeSettings.TallgrassMaxRainfall.ToString();
        string volcanicSpawnChanceBuff = BiomeSettings.VolcanicSpawnChance.ToString();


        list.Begin(inRect.LeftPart(0.35f));

        list.Gap();

        Text.Font = GameFont.Medium;
        list.Label("NPS_BaseGame".Translate());
        Text.Font = GameFont.Small;

        list.CheckboxLabeled(
            "NPS_ModifyBiome_Title".Translate(BiomeDefOf.AridShrubland.LabelCap),
            ref BiomeSettings.ModifyAridShrubland,
            tooltip: "NPS_ModifyBiome_Text".Translate(BiomeDefOf.AridShrubland.LabelCap));
        list.CheckboxLabeled(
            "NPS_ModifyBiome_Title".Translate(BiomeDefOf.TemperateForest.LabelCap),
            ref BiomeSettings.ModifyTemperateForest,
            tooltip: "NPS_ModifyBiome_Text".Translate(BiomeDefOf.TemperateForest.LabelCap));

        //ToggleablePatches
        list.Gap();
        Text.Font = GameFont.Medium;
        list.Label("NPS_PlantEffects".Translate());
        Text.Font = GameFont.Small;

        list.CheckboxLabeled(
            "NPS_Dandelions_Title".Translate(),
            ref BiomeSettings.DandelionChanges,
            "NPS_Dandelions_Text".Translate());

        list.CheckboxLabeled(
            "NPS_WildVegetables_Title".Translate(),
            ref BiomeSettings.WildVegetables,
            "NPS_WildVegetables_Text".Translate());

        if (ModsConfig.OdysseyActive) {
            list.CheckboxLabeled(
                "NPS_LavaReplace_Title".Translate(),
                ref BiomeSettings.LavaReplace,
                "NPS_LavaReplace_Text".Translate());
        }

        list.Gap();
/*
        list.CheckboxLabeled(
            "NPS_allowLavaEruption_title".Translate(),
            ref BiomeSettings.allowLavaEruption,
            "NPS_allowLavaEruption_text".Translate());
        list.CheckboxLabeled(
            "NPS_spawnLavaOnlyInBiome_title".Translate(),
            ref BiomeSettings.spawnLavaOnlyInBiome,
            "NPS_spawnLavaOnlyInBiome_text".Translate());
*/

        list.CheckboxLabeled(
            "NPS_SteamVentsDespawn_Title".Translate(),
            ref BiomeSettings.SteamVentsDespawn,
            "NPS_SteamVentsDespawn_Text".Translate());

        list.End();

        Rect outRect = inRect.RightPart(0.49f);
        Rect viewRect = new Rect(outRect.x + 20, outRect.y, width: outRect.width - 30f, height: 1400);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

        list.Begin(viewRect);

        Text.Font = GameFont.Medium;
        list.Label("NPS_SpawnChances".Translate());
        Text.Font = GameFont.Tiny;
        list.Label("NPS_SpawnNote".Translate().Colorize(Color.yellow));
        Text.Font = GameFont.Small;

        list.Gap();

        Text.Font = GameFont.Medium;
        list.Label(BiomeDefOf.TKKN_Oasis.LabelCap);
        Text.Font = GameFont.Small;
        list.CheckboxLabeled("NPS_AllowBiome_Title".Translate(BiomeDefOf.TKKN_Oasis.LabelCap),
            ref BiomeSettings.AllowDesertOasis,
            tooltip: "NPS_AllowBiomes_Text".Translate(BiomeDefOf.TKKN_Oasis.LabelCap));

        if (BiomeSettings.AllowDesertOasis) {
            if (list.ButtonText("NPS_ResetSettings".Translate(), widthPct: 0.4f)) {
                BiomeSettings.OasisSpawnChance = 0.006f;
            }

            list.Gap();
            list.TextFieldNumericLabeled("NPS_SpawnChance".Translate(), ref BiomeSettings.OasisSpawnChance,
                ref oasisSpawnChanceBuff);
        }

        list.GapLine();

        Text.Font = GameFont.Medium;
        list.Label(BiomeDefOf.TKKN_Desert.LabelCap);
        Text.Font = GameFont.Small;
        list.CheckboxLabeled("NPS_AllowBiome_Title".Translate(BiomeDefOf.TKKN_Desert.LabelCap),
            ref BiomeSettings.AllowDesertSaltFlats,
            tooltip: "NPS_AllowBiomes_Text".Translate(BiomeDefOf.TKKN_Desert.LabelCap));

        if (BiomeSettings.AllowDesertSaltFlats) {
            if (list.ButtonText("NPS_ResetSettings".Translate(), widthPct: 0.4f)) {
                BiomeSettings.SaltFlatSpawnChance = 0.006f;
            }

            list.Gap();
            list.TextFieldNumericLabeled("NPS_SpawnChance".Translate(), ref BiomeSettings.SaltFlatSpawnChance,
                ref saltFlatSpawnChanceBuff);
        }

        list.GapLine();

        Text.Font = GameFont.Medium;
        list.Label(BiomeDefOf.TKKN_RedwoodForest.LabelCap);
        Text.Font = GameFont.Small;
        list.CheckboxLabeled("NPS_AllowBiome_Title".Translate(BiomeDefOf.TKKN_RedwoodForest.LabelCap),
            ref BiomeSettings.AllowRedwood,
            tooltip: "NPS_AllowBiomes_Text".Translate(BiomeDefOf.TKKN_RedwoodForest.LabelCap));

        if (BiomeSettings.AllowRedwood) {
            if (list.ButtonText("NPS_ResetSettings".Translate(), widthPct: 0.4f)) {
                BiomeSettings.RedwoodMinTemp = -10f;
                BiomeSettings.RedwoodMaxTemp = 10f;
                BiomeSettings.RedwoodMaxRainfall = 1100;
            }

            list.Gap();
            list.TextFieldNumericLabeled("NPS_MinTemperature".Translate(), ref BiomeSettings.RedwoodMinTemp,
                ref redwoodMinTempBuff, min: float.MinValue);
            list.TextFieldNumericLabeled("NPS_MaxTemperature".Translate(), ref BiomeSettings.RedwoodMaxTemp,
                ref redwoodMaxTempBuff);
            list.TextFieldNumericLabeled("NPS_MaxRainfall".Translate(), ref BiomeSettings.RedwoodMaxRainfall,
                ref redwoodMaxRainfallBuff);
        }


        list.GapLine();

        Text.Font = GameFont.Medium;
        list.Label(BiomeDefOf.TKKN_Savanna.LabelCap);
        Text.Font = GameFont.Small;
        list.CheckboxLabeled("NPS_AllowBiome_Title".Translate(BiomeDefOf.TKKN_Savanna.LabelCap),
            ref BiomeSettings.AllowSavanna,
            tooltip: "NPS_AllowBiomes_Text".Translate(BiomeDefOf.TKKN_Savanna.LabelCap));

        if (BiomeSettings.AllowSavanna) {
            if (list.ButtonText("NPS_ResetSettings".Translate(), widthPct: 0.4f)) {
                BiomeSettings.SavannaMinTemp = 24;
                BiomeSettings.SavannaMaxTemp = 30;
                BiomeSettings.SavannaMinRainfall = 1400;
                BiomeSettings.SavannaMaxRainfall = 2000;
            }

            list.Gap();
            list.TextFieldNumericLabeled("NPS_MinTemperature".Translate(), ref BiomeSettings.SavannaMinTemp,
                ref savannaMinTempBuff, min: float.MinValue);
            list.TextFieldNumericLabeled("NPS_MaxTemperature".Translate(), ref BiomeSettings.SavannaMaxTemp,
                ref savannaMaxTempBuff);
            list.TextFieldNumericLabeled("NPS_MinRainfall".Translate(), ref BiomeSettings.SavannaMinRainfall,
                ref savannaMinRainfallBuff);
            list.TextFieldNumericLabeled("NPS_MaxRainfall".Translate(), ref BiomeSettings.SavannaMaxRainfall,
                ref savannaMaxRainfallBuff);
        }

        list.GapLine();

        Text.Font = GameFont.Medium;
        list.Label(BiomeDefOf.TKKN_Grasslands.LabelCap);
        Text.Font = GameFont.Small;
        list.CheckboxLabeled("NPS_AllowBiome_Title".Translate(BiomeDefOf.TKKN_Grasslands.LabelCap),
            ref BiomeSettings.AllowTallGrassPrairie,
            tooltip: "NPS_AllowBiomes_Text".Translate(BiomeDefOf.TKKN_Grasslands.LabelCap));

        if (BiomeSettings.AllowTallGrassPrairie) {
            if (list.ButtonText("NPS_ResetSettings".Translate(), widthPct: 0.4f)) {
                BiomeSettings.TallgrassMinTemp = -10;
                BiomeSettings.TallgrassMaxTemp = 22;
                BiomeSettings.TallgrassMinRainfall = 900;
                BiomeSettings.TallgrassMaxRainfall = 1300;
            }

            list.Gap();
            list.TextFieldNumericLabeled("NPS_MinTemperature".Translate(), ref BiomeSettings.TallgrassMinTemp,
                ref tallgrassMinTempBuff, min: float.MinValue);
            list.TextFieldNumericLabeled("NPS_MaxTemperature".Translate(), ref BiomeSettings.TallgrassMaxTemp,
                ref tallgrassMaxTempBuff);
            list.TextFieldNumericLabeled("NPS_MinRainfall".Translate(), ref BiomeSettings.TallgrassMinRainfall,
                ref tallgrassMinRainfallBuff);
            list.TextFieldNumericLabeled("NPS_MaxRainfall".Translate(), ref BiomeSettings.TallgrassMaxRainfall,
                ref tallgrassMaxRainfallBuff);
        }

        list.GapLine();

        Text.Font = GameFont.Medium;
        list.Label(BiomeDefOf.TKKN_VolcanicFlow.LabelCap);
        Text.Font = GameFont.Small;
        list.CheckboxLabeled("NPS_AllowBiome_Title".Translate(BiomeDefOf.TKKN_VolcanicFlow.LabelCap),
            ref BiomeSettings.AllowVolcanicFields,
            tooltip: "NPS_AllowBiomes_Text".Translate(BiomeDefOf.TKKN_VolcanicFlow.LabelCap));

        if (BiomeSettings.AllowVolcanicFields) {
            if (list.ButtonText("NPS_ResetSettings".Translate(), widthPct: 0.4f)) {
                BiomeSettings.VolcanicSpawnChance = 0.009f;
            }

            list.Gap();
            list.TextFieldNumericLabeled("NPS_SpawnChance".Translate(), ref BiomeSettings.VolcanicSpawnChance,
                ref volcanicSpawnChanceBuff);
        }

        list.GapLine();


        list.End();
        Widgets.EndScrollView();
    }
}