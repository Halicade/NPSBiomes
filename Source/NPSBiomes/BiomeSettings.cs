using Verse;

namespace NPSBiomes;

public class BiomeSettings : ModSettings
{
    //Biomes
    public static bool AllowDesertSaltFlats = true;
    public static bool AllowDesertOasis = true;
    public static bool AllowRedwood = true;
    public static bool AllowTallGrassPrairie = true;
    public static bool AllowSavanna = true;
    public static bool AllowVolcanicFields = true;
    public static bool ModifyAridShrubland = true;
    public static bool ModifyTemperateForest = true;
    public static bool LavaReplace = true;
    public static bool SpawnLavaOnlyInBiome = true;
    public static bool AllowLavaEruption = true;
    public static bool BasaltLockedToBiome = true;

    //Plant changes
    public static bool DandelionChanges = true;
    public static bool WildVegetables = true;

    public static bool MedievalOverhaulActive;

    public static bool SteamVentsDespawn = true;

    public static float OasisSpawnChance = 0.006f;
    public static float SaltFlatSpawnChance = 0.006f;

    public static float RedwoodMinTemp = -10;
    public static float RedwoodMaxTemp = 10;
    public static float RedwoodMaxRainfall = 1100;

    public static float SavannaMinTemp = 24;
    public static float SavannaMaxTemp = 30;
    public static float SavannaMinRainfall = 1400;
    public static float SavannaMaxRainfall = 2000;
    
    public static float TallgrassMinTemp = -10;
    public static float TallgrassMaxTemp = 30f;
    public static float TallgrassMinRainfall = 900;
    public static float TallgrassMaxRainfall = 1300;

    public static float VolcanicSpawnChance = 0.009f;
    
    

    public static bool GetActiveSettings(string settingName) {
        switch (settingName) {
            case "Dandelions":
                return DandelionChanges;
            case "WildVegetables":
                return WildVegetables;
            case "LavaReplace":
                return LavaReplace;
            case "BasaltLocked":
                return BasaltLockedToBiome;
            default:
                Log.Error($"NPSBiomes: Error trying to perform operation. Could not find setting named {settingName}");
                return false;
        }
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Values.Look(ref AllowDesertSaltFlats, "allowDesertSaltFlats", true);
        Scribe_Values.Look(ref AllowDesertOasis, "allowDesertOasis", true);
        Scribe_Values.Look(ref AllowRedwood, "allowRedwood", true);
        Scribe_Values.Look(ref AllowTallGrassPrairie, "allowTallGrassPrairie", true);
        Scribe_Values.Look(ref AllowSavanna, "allowSavanna", true);
        Scribe_Values.Look(ref AllowVolcanicFields, "allowVolcanicFields", true);
        Scribe_Values.Look(ref ModifyAridShrubland, "modifyAridShrubland", true);
        Scribe_Values.Look(ref ModifyTemperateForest, "modifyTemperateForest", true);
        Scribe_Values.Look(ref DandelionChanges, "dandelionChanges", true);
        Scribe_Values.Look(ref WildVegetables, "wildVegetables", true);
        Scribe_Values.Look(ref SpawnLavaOnlyInBiome, "spawnLavaOnlyInBiome", true);
        Scribe_Values.Look(ref AllowLavaEruption, "allowLavaEruption", true);
        Scribe_Values.Look(ref BasaltLockedToBiome, "basaltLockedToBiome", true);
        Scribe_Values.Look(ref LavaReplace, "lavaReplace", true);
        Scribe_Values.Look(ref SteamVentsDespawn, "steamVentsDespawn", true);

        Scribe_Values.Look(ref OasisSpawnChance, "oasisSpawnChance", 0.006f);
        Scribe_Values.Look(ref SaltFlatSpawnChance, "saltFlatSpawnChance", 0.006f);
        
        Scribe_Values.Look(ref RedwoodMinTemp, "RedwoodMinTemp", -10);
        Scribe_Values.Look(ref RedwoodMaxTemp, "RedwoodMaxTemp", 10);
        Scribe_Values.Look(ref RedwoodMaxRainfall, "RedwoodMaxRainfall", 1100);
        
        Scribe_Values.Look(ref SavannaMinTemp , "SavannaMinTemp ", 24);
        Scribe_Values.Look(ref SavannaMaxTemp, "SavannaMaxTemp", 30);
        Scribe_Values.Look(ref SavannaMinRainfall, "SavannaMinRainfall", 1400);
        Scribe_Values.Look(ref SavannaMaxRainfall, "SavannaMaxRainfall", 2000);
        
        Scribe_Values.Look(ref TallgrassMinTemp , "TallgrassMinTemp ", -10);
        Scribe_Values.Look(ref TallgrassMaxTemp, "TallgrassMaxTemp", 22);
        Scribe_Values.Look(ref TallgrassMinRainfall, "TallgrassMinRainfall", 900);
        Scribe_Values.Look(ref TallgrassMaxRainfall, "TallgrassMaxRainfall", 1300);
        
        Scribe_Values.Look(ref VolcanicSpawnChance, "VolcanicSpawnChance", 0.009f);
        
    }
}