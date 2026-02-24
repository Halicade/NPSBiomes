using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NPSBiomes;

public class IngestionOutcomeDoer_SaltWounds : IngestionOutcomeDoer
{
    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount) {
        SaltWoundsForPawn.SaltWounds(pawn);
    }
}