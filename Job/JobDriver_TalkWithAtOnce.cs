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
    /// <summary>
    /// 对话对象（<see cref="Pawn"/>）。
    /// </summary>
    protected Pawn TalkWith => TargetPawnA;

    /// <inheritdoc/>
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(TalkWith, job, 1, -1, null, errorOnFailed);
    }

    /// <inheritdoc/>
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

    /// <summary>
    /// 执行对话操作。
    /// </summary>
    /// <param name="talker">发起对话的<see cref="Pawn"/>。</param>
    /// <param name="talkWith">对话对象（<see cref="Pawn"/>）。</param>
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