using RimWorld;
using RimWorld.Planet;

namespace NPSBiomes;

public class NPS_BiomeWorker_Savanna : BiomeWorker
{
    public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile) {
        if (!BiomeSettings.allowSavanna) {
            return -100f;
        }

        //keep this the same, just make it fail more often. When it fails, shrubland will be rendered, instead.
        if (tile.WaterCovered) {
            return -100f;
        }

        if (tile.temperature < BiomeSettings.SavannaMinTemp ||
            tile.temperature > BiomeSettings.SavannaMaxTemp) {
            return 0f;
        }

        if (tile.rainfall < BiomeSettings.SavannaMinRainfall ||
            tile.rainfall >= BiomeSettings.SavannaMaxRainfall) {
            return 0f;
        }

        return 22.5f + (tile.temperature - 7f) + 30 + tile.rainfall / 180f;
    }
}