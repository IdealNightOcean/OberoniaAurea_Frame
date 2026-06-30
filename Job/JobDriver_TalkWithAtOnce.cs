using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

/// <summary>
/// 对话的工作驱动抽象类。
/// </summary>
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

    /// <summary>
    /// 启用职责中的对话功能。
    /// </summary>
    public static void EnableLordJobTalk(Pawn talkWith)
    {
        if (talkWith.GetLord()?.LordJob is LordJob_VisitColonyTalkable talkLordJob)
        {
            talkLordJob.EnableTalk(talkWith);
        }
    }

    /// <summary>
    /// 禁用职责中的对话功能。
    /// </summary>
    public static void DisableLordJobTalk(Pawn talkWith, bool dismiss)
    {
        if (talkWith.GetLord()?.LordJob is LordJob_VisitColonyTalkable talkLordJob && talkLordJob.CanTalkWith(talkWith))
        {
            talkLordJob.DisableTalk(dismiss);
        }
    }
}