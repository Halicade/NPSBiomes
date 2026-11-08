using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace NPSBiomes;

[HarmonyPatch(typeof(BiomeWorker_AridShrubland), nameof(BiomeWorker_AridShrubland.GetScore))]
public static class BiomeWorker_AridShrubland_GetScore
{
    static bool Prepare() => BiomeSettings.ModifyAridShrubland;

    public static void Postfix(Tile tile, ref float __result) {
        if (Math.Abs(__result - -100f) < 0.001f) {
            return;
        }
        if (__result == 0) {
            return;
        }

        if (tile.rainfall >= 1200f || tile.temperature < 23f) {
            __result = 0f;
            return;
        }
        __result = 22.5f + (tile.temperature - 20f) * 2.2000000476837158f +
                   (tile.rainfall - 600f) / 100f;

    }
}