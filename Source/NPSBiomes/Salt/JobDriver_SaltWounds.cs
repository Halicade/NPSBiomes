using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace NPSBiomes;

public class JobDriver_SaltWounds : JobDriver
{

    private const int DurationTicks = 600;

    private Mote warmupMote;

    private Pawn InjuredPawn => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    private Thing Item => job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (pawn.Reserve(InjuredPawn, job, 1, -1, null, errorOnFailed)) {
            return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed);
        }

        return false;
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B)
            .FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
        Toil toil = Toils_General.Wait(DurationTicks);
        toil.WithProgressBarToilDelay(TargetIndex.A);
        toil.FailOnDespawnedOrNull(TargetIndex.A);
        toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
        toil.tickAction = delegate {
            CompUsable compUsable = Item.TryGetComp<CompUsable>();
            if (compUsable != null && warmupMote == null && compUsable.Props.warmupMote != null) {
                warmupMote = MoteMaker.MakeAttachedOverlay(InjuredPawn, compUsable.Props.warmupMote, Vector3.zero);
            }

            warmupMote?.Maintain();
        };
        yield return toil;
        yield return Toils_General.Do(TendWounds);
    }

    private void TendWounds() {
        SaltWoundsForPawn.SaltWounds(InjuredPawn);
    }
}