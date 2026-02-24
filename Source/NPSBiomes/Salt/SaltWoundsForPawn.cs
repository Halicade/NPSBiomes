using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NPSBiomes;

public static class SaltWoundsForPawn
{
    public static void SaltWounds(Pawn injuredPawn) {
        
        var pawnhealth = injuredPawn.health.hediffSet.hediffs;

        List<Hediff> hediffsToReplace = [];

        foreach (Hediff hediff in pawnhealth) {
            if (hediff.def == HediffDefOf.Cut || hediff.def == HediffDefOf.Bite || hediff.def == SaltDefOfs.Stab) {
                if (!hediff.IsTended()) {
                    if (hediff.Bleeding) {
                        hediffsToReplace.Add(hediff);
                    }
                }
            }
        }

        foreach (Hediff hediff in hediffsToReplace) {
            Hediff newSaltedWound = HediffMaker.MakeHediff(SaltDefOfs.NPS_SaltedWound, injuredPawn, hediff.Part);


            newSaltedWound.Severity = hediff.Severity;
            injuredPawn.health.RemoveHediff(hediff);
            injuredPawn.health.AddHediff(newSaltedWound);
        }

    }
}