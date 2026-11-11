using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NPSBiomes;

public class CompTargetable_CanBeSalted : CompTargetable_SinglePawn
{

    protected override TargetingParameters GetTargetingParameters()
    {
        return new TargetingParameters
        {
            canTargetPawns = true,
            canTargetBuildings = false,
            validator = target => {
                if (target.Thing is Pawn pawn) {
                    return SaltWoundsForPawn.HasWoundForSalting(pawn);
                }
                return false;
            }
        };
    }

}