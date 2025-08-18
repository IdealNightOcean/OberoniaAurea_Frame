using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace OberoniaAurea_Frame;

public abstract class JobDriver_TalkWithAtOnce : JobDriver
{
    protected Pawn TalkWith => TargetPawnA;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(TalkWith, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => TalkWith.DeadOrDowned);
        Toil talk = ToilMaker.MakeToil("TalkToils");
        talk.initAction = delegate
        {
            Pawn actor = talk.actor;
            if (!TalkWith.DeadOrDowned)
            {
                TalkAction(actor, TalkWith);
            }
        };
        yield return talk;
    }

    protected abstract void TalkAction(Pawn talker, Pawn talkWith);
}