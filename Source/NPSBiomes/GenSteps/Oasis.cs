using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace NPSBiomes.GenSteps;

public class Oasis : GenStep
{
    public override int SeedPart => 7899489;

    private readonly HashSet<IntVec3> waterCells = [];
    private readonly HashSet<IntVec3> deepWaterCells = [];
    private readonly HashSet<IntVec3> soilLine = [];
    private readonly HashSet<IntVec3> soilCells = [];

    public TerrainDef waterTerrain;
    public TerrainDef deepWaterTerrain;
    public TerrainDef soilTerrain;


    private IntVec3 leftCorner;
    private IntVec3 topCorner;
    private IntVec3 rightCorner;
    private IntVec3 bottomCorner;

    private IntVec3 center;

    private readonly List<IntVec3> cornerPoints = [];


    public override void Generate(Map map, GenStepParams parms) {
        waterCells.Clear();
        deepWaterCells.Clear();
        soilLine.Clear();
        soilCells.Clear();
        cornerPoints.Clear();

        CellFinder.TryFindRandomCellNear(map.Center, map, 40, x => !x.Roofed(map), out center);

        if (!center.IsValid || !center.InBounds(map)) {
            return;
        }

        leftCorner = center - new IntVec3(Rand.Range(17, 25), 0, Rand.Range(-13, 13));
        bottomCorner = center - new IntVec3(Rand.Range(-13, 12), 0, Rand.Range(18, 25));
        topCorner = center + new IntVec3(Rand.Range(-12, 13), 0, Rand.Range(18, 27));
        rightCorner = center + new IntVec3(Rand.Range(20, 24), 0, Rand.Range(-12, 13));

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
            GetDeepWaterCells(cornerPoints[i], cornerPoints[i + 1], cornerPoints[i + 2]);
            
        }

        EncircleSoilCells();


        foreach (var water in waterCells) {
            if (!water.InBounds(map))
                continue;
            if (water.GetEdifice(map) != null)
                continue;
            map.terrainGrid.SetTerrain(water, waterTerrain);
        }

        foreach (var water in deepWaterCells) {
            if (!water.InBounds(map))
                continue;
            if (water.GetEdifice(map) != null)
                continue;
            map.terrainGrid.SetTerrain(water, deepWaterTerrain);
        }

        foreach (var soil in soilCells) {
            if (!soil.InBounds(map))
                continue;
            if (soil.GetEdifice(map) != null)
                continue;
            if (soil.GetTerrain(map).IsWater)
                continue;
            map.terrainGrid.SetTerrain(soil, soilTerrain);
        }
        waterCells.Clear();
        deepWaterCells.Clear();
        soilLine.Clear();
        soilCells.Clear();
        cornerPoints.Clear();
    }

    private void EncircleSoilCells() {
        foreach (var cell in soilLine) {
            if (Rand.Chance(0.45f)) {
                foreach (var circledCell in GenRadial.RadialCellsAround(cell, Rand.Range(0, 15), true)) {
                    soilCells.Add(circledCell);
                }
            }
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

    private void GetDeepWaterCells(IntVec3 start, IntVec3 mid, IntVec3 end) {
        IntVec3 randOffset = new IntVec3(Rand.Range(-5, 5), 0, Rand.Range(-5, 5));
        var halfStart = GetDivCenterPoint(start) + randOffset;
        var halfMid = GetDivCenterPoint(mid) + randOffset;
        var halfEnd = GetDivCenterPoint(end) + randOffset;


        var dist = halfStart.DistanceTo(halfEnd);
        var interval = 1 / (dist * 20);

        for (float i = 0; i < dist; i += interval)
            GetDeepWaterCellsAround(GetCurve(halfStart, halfMid, halfEnd, i));
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

    private IntVec3 GetDivCenterPoint(IntVec3 cell) {
        return (cell - center) / 3 + center;
    }
    
    private static IntVec3 GetCurve(IntVec3 start, IntVec3 mid, IntVec3 end, float t) {
        var firstHalf = Vector3.Lerp(start.ToVector3(), mid.ToVector3(), t);
        var secondHalf = Vector3.Lerp(mid.ToVector3(), end.ToVector3(), t);
        return IntVec3.FromVector3(Vector3.Lerp(firstHalf, secondHalf, t));
    }

    private void GetWaterCellsAround(IntVec3 start) {
        soilLine.Add(start);
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