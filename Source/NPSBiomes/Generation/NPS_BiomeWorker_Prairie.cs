using RimWorld;
using RimWorld.Planet;

namespace NPSBiomes;

public class NPS_BiomeWorker_Prairie : BiomeWorker
{
    public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile) {
        if (!BiomeSettings.allowTallGrassPrairie) {
            return -100f;
        }

        if (tile.WaterCovered) {
            return -100f;
        }

        if (tile.temperature < BiomeSettings.TallgrassMinTemp || 
            tile.temperature > BiomeSettings.TallgrassMaxTemp) {
            return 0f;
        }

        if (tile.rainfall < BiomeSettings.TallgrassMinRainfall || 
            tile.rainfall >= BiomeSettings.TallgrassMaxRainfall) {
            return 0f;
        }

        if (tile.hilliness != Hilliness.Flat) {
            return 0f;
        }

        return 22.5f + ((tile.temperature - 20f) * 6.2f) + ((tile.rainfall - 0f) / 100f);
    }
}