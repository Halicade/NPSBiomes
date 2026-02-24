using RimWorld;
using Verse;
using Verse.AI;

namespace NPSBiomes;

public class CompProperties_SaltWounds : CompProperties
{
    public CompProperties_SaltWounds() {
        compClass = typeof(CompTargetEffect_SaltWounds);
    }
}

public class CompTargetEffect_SaltWounds : CompTargetEffect
{
    public CompProperties_TargetEffectResurrect Props => (CompProperties_TargetEffectResurrect)props;

    public override void DoEffectOn(Pawn user, Thing target) {
        if (user.IsColonistPlayerControlled) {
            Job job = JobMaker.MakeJob(SaltDefOfs.NPS_SaltWounds, target, parent);
            job.count = 1;
            job.playerForced = true;
            user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }
    }
}