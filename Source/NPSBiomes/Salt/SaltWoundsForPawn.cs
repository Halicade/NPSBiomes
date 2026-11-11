using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NPSBiomes;

public static class SaltWoundsForPawn
{

    public static bool HasWoundForSalting(Pawn pawn) {
        if (pawn?.health?.hediffSet == null) {
            return false;
        }

        foreach (Hediff hediff in pawn.health.hediffSet.hediffs) {
            if (IsSaltableWound(hediff)) {
                return true;
            }
        }
        return false;
    }

    public static bool SaltWounds(Pawn injuredPawn) {

        if (injuredPawn?.health?.hediffSet == null) {
            return false;
        }

        List<Hediff> pawnhealth = injuredPawn.health.hediffSet.hediffs;

        List<Hediff> hediffsToReplace = [];
        bool healedWounds = false;

        foreach (Hediff hediff in pawnhealth) {
            if (IsSaltableWound(hediff)) {
                hediffsToReplace.Add(hediff);
            }
        }

        foreach (Hediff hediff in hediffsToReplace) {
            Hediff newSaltedWound = HediffMaker.MakeHediff(SaltDefOfs.NPS_SaltedWound, injuredPawn, hediff.Part);

            newSaltedWound.Severity = hediff.Severity;
            injuredPawn.health.RemoveHediff(hediff);
            injuredPawn.health.AddHediff(newSaltedWound);
            healedWounds = true;
        }
        return healedWounds;
    }

    private static bool IsSaltableWound(Hediff hediff) {
        return hediff != null
               && (hediff.def == HediffDefOf.Cut || hediff.def == HediffDefOf.Bite || hediff.def == SaltDefOfs.Stab)
               && !hediff.IsTended() && !hediff.IsPermanent();
    }
}