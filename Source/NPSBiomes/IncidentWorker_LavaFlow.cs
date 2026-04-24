using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace NPSBiomes;

public class IncidentWorkerLavaFlow : IncidentWorker
{
    private const float FogClearRadius = 4.5f;

    private readonly HashSet<IntVec3> deepWaterCells = [];
    private readonly HashSet<IntVec3> waterCells = [];


    private TerrainDef shallowLava;
    private TerrainDef deepLava;

    private IntVec3 leftCorner;
    private IntVec3 topCorner;
    private IntVec3 rightCorner;
    private IntVec3 bottomCorner;

    private IntVec3 center;

    private readonly List<IntVec3> cornerPoints = [];
    //Event doesn't run. Too similar to Odyssey events. Keeping it cause I have nowhere else to put it
    protected override bool CanFireNowSub(IncidentParms parms) {
        if (!BiomeSettings.allowLavaEruption) {
            return false;
        }

        var map = (Map)parms.target;
        if (BiomeSettings.spawnLavaOnlyInBiome && map.Biome != BiomeDefOf.TKKN_VolcanicFlow) {
            return false;
        }

        return true;
    }

    protected override bool TryExecuteWorker(IncidentParms parms) {
        var map = (Map)parms.target;

        if (!BiomeSettings.allowLavaEruption) {
            return false;
        }

        if (BiomeSettings.spawnLavaOnlyInBiome && map.Biome != BiomeDefOf.TKKN_VolcanicFlow) {
            return false;
        }

        if (ModsConfig.OdysseyActive && BiomeSettings.lavaReplace) {
            shallowLava = RimWorld.TerrainDefOf.LavaDeep;
            deepLava = RimWorld.TerrainDefOf.LavaDeep;
        }
        else {
            shallowLava = TerrainDefOf.TKKN_Lava;
            deepLava = TerrainDefOf.TKKN_LavaDeep;
        }

        GenerateLava(map);
        Find.LetterStack.ReceiveLetter("TKKN_NPS_LavaHasEruptedNearby".Translate(),
            "TKKN_NPS_LavaHasEruptedNearbyTxt".Translate(), LetterDefOf.NeutralEvent, new TargetInfo(center, map));


        return true;
    }


    private void GenerateLava(Map map) {
        waterCells.Clear();
        deepWaterCells.Clear();
        cornerPoints.Clear();

        GenerateSpringLocations(map);
        cornerPoints.Clear();


        foreach (var water in waterCells) {
            if (!water.InBounds(map))
                continue;
            if (water.GetEdifice(map) != null)
                continue;
            map.terrainGrid.SetTerrain(water, shallowLava);
        }

        foreach (var water in deepWaterCells) {
            if (!water.InBounds(map))
                continue;
            if (water.GetEdifice(map) != null)
                continue;
            map.terrainGrid.SetTerrain(water, deepLava);
        }
    }

    private void GenerateSpringLocations(Map map) {
        if (!CellFinder.TryFindRandomCell(map,
                x => !x.Roofed(map) && !x.GetTerrain(map).IsWater && x.DistanceToEdge(map) > 25, out var springPoint)) {
            Log.Warning("Unable to find a valid spring position");
            return;
        }

        center = springPoint;
        Log.Error("Spring at " + center);
        //center = CellFinderLoose.TryFindCentralCell(map, 10, 15, x => !x.Roofed(map));

        if (!center.IsValid || !center.InBounds(map))
            return;


        leftCorner = center - new IntVec3(Rand.Range(4, 8), 0, Rand.Range(-4, 4));
        bottomCorner = center - new IntVec3(Rand.Range(-5, 5), 0, Rand.Range(4, 8));
        topCorner = center + new IntVec3(Rand.Range(-3, 8), 0, Rand.Range(4, 6));
        rightCorner = center + new IntVec3(Rand.Range(3, 7), 0, Rand.Range(-4, 5));

        cornerPoints.Add(leftCorner);
        cornerPoints.Add(GetMidpoint(leftCorner, topCorner));
        cornerPoints.Add(topCorner);
        cornerPoints.Add(GetMidpoint(topCorner, rightCorner));
        cornerPoints.Add(rightCorner);
        cornerPoints.Add(GetMidpoint(rightCorner, bottomCorner));
        cornerPoints.Add(bottomCorner);
        cornerPoints.Add(GetMidpoint(bottomCorner, leftCorner));
        // Adding left corner again so we don't accidentally loop ourselves into  oblivion
        cornerPoints.Add(leftCorner);
        cornerPoints.Add(cornerPoints[1]);

        for (int i = 0; i < cornerPoints.Count - 2; i++) {
            GetWaterCells(cornerPoints[i], cornerPoints[i + 1], cornerPoints[i + 2]);
        }
    }

    private static IntVec3 GetMidpoint(IntVec3 start, IntVec3 end) {
        //RandomX = min(x1, x2) + Math.random() * Math.abs(x2 - x1)

        //RandomY = min(y1, y2) + Math.random() * Math.abs(y2 - y1)
        var randomMid = new IntVec3(Math.Min(start.x, end.x) + (int)(Rand.Value * Math.Abs(end.x - start.x)),
            0,
            Math.Min(start.z, end.z) + (int)(Rand.Value * Math.Abs(end.z - start.z)));

        return randomMid;
    }

    private void GetWaterCells(IntVec3 start, IntVec3 mid, IntVec3 end) {
        var dist = start.DistanceTo(end);
        var interval = 1 / (dist * 20);

        var multiMid = (mid - center) * 2 + center;

        var divStart = GetDivCenterPoint(start);
        var divMid = GetDivCenterPoint(mid);
        var divEnd = GetDivCenterPoint(end);

        for (float i = 0; i < dist; i += interval) {
            var curve = GetCurve(start, multiMid, end, i);

            var deepCurve = GetCurve(divStart, divMid, divEnd, i);
            GetWaterCellsAround(curve);
            GetDeepWaterCellsAround(deepCurve);
        }
    }

    private static IntVec3 GetCurve(IntVec3 start, IntVec3 mid, IntVec3 end, float t) {
        var firstHalf = Vector3.Lerp(start.ToVector3(), mid.ToVector3(), t);
        var secondHalf = Vector3.Lerp(mid.ToVector3(), end.ToVector3(), t);
        return IntVec3.FromVector3(Vector3.Lerp(firstHalf, secondHalf, t));
    }

    private IntVec3 GetDivCenterPoint(IntVec3 cell) {
        return (cell - center) / 3 + center;
    }

    private void GetWaterCellsAround(IntVec3 start) {
        foreach (var cellsAround in GenSight.PointsOnLineOfSight(start, center)) {
            waterCells.Add(cellsAround);
        }
    }

    private void GetDeepWaterCellsAround(IntVec3 start) {
        foreach (var cellsAround in GenSight.PointsOnLineOfSight(start, center)) {
            deepWaterCells.Add(cellsAround);
        }
    }
}