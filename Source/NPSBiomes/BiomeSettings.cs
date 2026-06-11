using UnityEngine;
using Verse;

namespace NPSBiomes;

public class BiomeSettings : ModSettings
{
    //Biomes
    public static bool allowDesertSaltFlats = true;
    public static bool allowDesertOasis = true;
    public static bool allowRedwood = true;
    public static bool allowTallGrassPrairie = true;
    public static bool allowSavanna = true;
    public static bool allowVolcanicFields = true;
    public static bool modifyAridShrubland = true;
    public static bool modifyTemperateForest = true;
    public static bool lavaReplace = true;
    public static bool spawnLavaOnlyInBiome = true;
    public static bool allowLavaEruption = true;
    public static bool basaltLockedToBiome = true;

    //Plant changes
    public static bool dandelionChanges = true;
    public static bool wildVegetables = true;

    public static bool MedievalOverhaulActive;

    public static bool NPSWeatherActive;

    //Requires NPS to function
    public static bool steamVentsDespawn = true;

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
                return dandelionChanges;
            case "WildVegetables":
                return wildVegetables;
            case "LavaReplace":
                return lavaReplace;
            case "BasaltLocked":
                return basaltLockedToBiome;
            default:
                Log.Error($"NPSBiomes: Error trying to perform operation. Could not find setting named {settingName}");
                return false;
        }
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Values.Look(ref allowDesertSaltFlats, "allowDesertSaltFlats", true);
        Scribe_Values.Look(ref allowDesertOasis, "allowDesertOasis", true);
        Scribe_Values.Look(ref allowRedwood, "allowRedwood", true);
        Scribe_Values.Look(ref allowTallGrassPrairie, "allowTallGrassPrairie", true);
        Scribe_Values.Look(ref allowSavanna, "allowSavanna", true);
        Scribe_Values.Look(ref allowVolcanicFields, "allowVolcanicFields", true);
        Scribe_Values.Look(ref modifyAridShrubland, "modifyAridShrubland", true);
        Scribe_Values.Look(ref modifyTemperateForest, "modifyTemperateForest", true);
        Scribe_Values.Look(ref dandelionChanges, "dandelionChanges", true);
        Scribe_Values.Look(ref wildVegetables, "wildVegetables", true);
        Scribe_Values.Look(ref spawnLavaOnlyInBiome, "spawnLavaOnlyInBiome", true);
        Scribe_Values.Look(ref allowLavaEruption, "allowLavaEruption", true);
        Scribe_Values.Look(ref basaltLockedToBiome, "basaltLockedToBiome", true);
        Scribe_Values.Look(ref lavaReplace, "lavaReplace", true);
        Scribe_Values.Look(ref steamVentsDespawn, "steamVentsDespawn", true);

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