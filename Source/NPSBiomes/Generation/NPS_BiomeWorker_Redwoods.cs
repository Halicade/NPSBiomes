using RimWorld;
using RimWorld.Planet;
using Verse;

namespace NPSBiomes;

public class NPS_BiomeWorker_Redwoods : BiomeWorker
{
    public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile) {
        if (!BiomeSettings.allowRedwood) {
            return -100f;
        }

        if (tile.WaterCovered) {
            return -100f;
        }

        if (tile.temperature < BiomeSettings.RedwoodMinTemp || tile.temperature > BiomeSettings.RedwoodMaxTemp) {
            return 0f;
        }

        if (tile.rainfall < BiomeSettings.RedwoodMaxRainfall)
            return 0f;

        //MO uses the same logic for their dark forest biome
        //Generate random chance for which biome takes over
        if (BiomeSettings.MedievalOverhaulActive) {
            return Rand.ChanceSeeded(0.5f, planetTile.tileId ^ 0x0af6a64d) ? 40.5f : 39.5f;
        }

        return 40f;
    }
}