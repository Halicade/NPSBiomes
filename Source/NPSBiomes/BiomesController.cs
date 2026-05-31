using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace NPSBiomes;

public class BiomesController : Mod
{
    public BiomesController(ModContentPack content)
        : base(content) {
        GetSettings<BiomeSettings>();
        BiomeSettings.MedievalOverhaulActive =
            ModLister.GetActiveModWithIdentifier("dankpyon.medieval.overhaul") != null;

        if (ModLister.GetActiveModWithIdentifier("hali.npsweathereffects") != null) {
            BiomeSettings.NPSWeatherActive = true;
        }
        else {
            BiomeSettings.steamVentsDespawn = false;
        }

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

        if (BiomeSettings.modifyAridShrubland) {
            harmony.Patch(
                AccessTools.Method(typeof(BiomeWorker_AridShrubland), nameof(BiomeWorker_AridShrubland.GetScore)),
                postfix: new HarmonyMethod(typeof(BiomeWorker_AridShrubland_GetScore),
                    nameof(BiomeWorker_AridShrubland_GetScore.Postfix)));
        }

        if (BiomeSettings.modifyTemperateForest) {
            harmony.Patch(
                AccessTools.Method(typeof(BiomeWorker_TemperateForest), nameof(BiomeWorker_TemperateForest.GetScore)),
                postfix: new HarmonyMethod(typeof(BiomeWorker_TemperateForest_GetScore),
                    nameof(BiomeWorker_TemperateForest_GetScore.Postfix)));
        }

        harmony.Patch(AccessTools.Method(typeof(GenSpawn), nameof(GenSpawn.Spawn),
            [
                typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool), typeof(bool)
            ]),
            postfix: new HarmonyMethod(typeof(GenSpawn_Spawn), nameof(GenSpawn_Spawn.Postfix)));

        harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.SpawnSetup)),
            postfix: new HarmonyMethod(typeof(Pawn_SpawnSetup),
                nameof(Pawn_SpawnSetup.Postfix)));
    }

    public void DoWindowContents(Rect inRect) {
        Listing_Standard list = new Listing_Standard(GameFont.Small);

        list.Begin(inRect.LeftHalf());

        list.CheckboxLabeled(
            "NPS_AllowBiome_Title".Translate(BiomeDefOf.TKKN_Desert.LabelCap),
            ref BiomeSettings.allowDesertSaltFlats,
            tooltip: "NPS_AllowBiomes_Text".Translate(BiomeDefOf.TKKN_Desert.LabelCap));
        list.CheckboxLabeled(
            "NPS_AllowBiome_Title".Translate(BiomeDefOf.TKKN_Oasis.LabelCap),
            ref BiomeSettings.allowDesertOasis,
            tooltip: "NPS_AllowBiomes_Text".Translate(BiomeDefOf.TKKN_Oasis.LabelCap));
        list.CheckboxLabeled(
            "NPS_AllowBiome_Title".Translate(BiomeDefOf.TKKN_RedwoodForest.LabelCap),
            ref BiomeSettings.allowRedwood,
            tooltip: "NPS_AllowBiomes_Text".Translate(BiomeDefOf.TKKN_RedwoodForest.LabelCap));
        list.CheckboxLabeled(
            "NPS_AllowBiome_Title".Translate(BiomeDefOf.TKKN_Grasslands.LabelCap),
            ref BiomeSettings.allowTallGrassPrairie,
            tooltip: "NPS_AllowBiomes_Text".Translate(BiomeDefOf.TKKN_Grasslands.LabelCap));
        list.CheckboxLabeled(
            "NPS_AllowBiome_Title".Translate(BiomeDefOf.TKKN_Savanna.LabelCap),
            ref BiomeSettings.allowSavanna,
            tooltip: "NPS_AllowBiomes_Text".Translate(BiomeDefOf.TKKN_Savanna.LabelCap));
        list.CheckboxLabeled(
            "NPS_AllowBiome_Title".Translate(BiomeDefOf.TKKN_VolcanicFlow.LabelCap),
            ref BiomeSettings.allowVolcanicFields,
            tooltip: "NPS_AllowBiomes_Text".Translate(BiomeDefOf.TKKN_VolcanicFlow.LabelCap));
        list.Gap();
        list.CheckboxLabeled(
            "NPS_ModifyBiome_Title".Translate(BiomeDefOf.AridShrubland.LabelCap),
            ref BiomeSettings.modifyAridShrubland,
            tooltip: "NPS_ModifyBiome_Text".Translate(BiomeDefOf.AridShrubland.LabelCap));
        list.CheckboxLabeled(
            "NPS_ModifyBiome_Title".Translate(BiomeDefOf.TemperateForest.LabelCap),
            ref BiomeSettings.modifyTemperateForest,
            tooltip: "NPS_ModifyBiome_Text".Translate(BiomeDefOf.TemperateForest.LabelCap));

        //ToggleablePatches
        list.Gap();
        list.CheckboxLabeled(
            "NPS_Dandelions_Title".Translate(),
            ref BiomeSettings.dandelionChanges,
            "NPS_Dandelions_Text".Translate());

        list.CheckboxLabeled(
            "NPS_WildVegetables_Title".Translate(),
            ref BiomeSettings.wildVegetables,
            "NPS_WildVegetables_Text".Translate());

        if (ModsConfig.OdysseyActive) {
            list.CheckboxLabeled(
                "NPS_LavaReplace_Title".Translate(),
                ref BiomeSettings.lavaReplace,
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
        if (BiomeSettings.NPSWeatherActive) {
            list.CheckboxLabeled(
                "NPS_SteamVentsDespawn_Title".Translate(),
                ref BiomeSettings.steamVentsDespawn,
                "NPS_SteamVentsDespawn_Text".Translate());
        }

        list.End();
    }
}