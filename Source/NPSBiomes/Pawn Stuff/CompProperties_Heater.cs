using Verse;

namespace NPSBiomes;

public class CompProperties_Heater : CompProperties
{
    public readonly int howOften = 15;
    public readonly int temperature = 6;
    
    public CompProperties_Heater()
    {
        compClass = typeof(Comp_Heater);
    }
}