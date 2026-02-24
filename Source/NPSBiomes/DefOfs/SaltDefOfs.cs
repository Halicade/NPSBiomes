using RimWorld;
using Verse;

namespace NPSBiomes;

[DefOf]
public class SaltDefOfs
{

    public static HediffDef Stab;
    
    public static HediffDef NPS_SaltedWound;

    public static JobDef NPS_SaltWounds;
    
    
    static SaltDefOfs() {
        DefOfHelper.EnsureInitializedInCtor(typeof(SaltDefOfs));
    }
}