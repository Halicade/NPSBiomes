using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace NPSBiomes;

[HarmonyPatch(typeof(BiomeWorker_TemperateForest), nameof(BiomeWorker_TemperateForest.GetScore))]
public static class BiomeWorker_TemperateForest_GetScore
{
    static bool Prepare() => BiomeSettings.ModifyTemperateForest;

    public static void Postfix(Tile tile, ref float __result) {
        if (Math.Abs(__result - -100f) < 0.001f) {
            return;
        }
        if (__result == 0) {
            return;
        }

        if (tile.rainfall < 1200f) {
            __result = 0f;
            return;
        }

        __result = 15f + (tile.temperature - 7f) + (tile.rainfall - 600f) / 180f;
    }
}