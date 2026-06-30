using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

/// <summary>
/// 可对话的访问殖民地职责。
/// </summary>
public class LordJob_VisitColonyTalkable : LordJob_VisitColonyBase, ILordJobWithTalk
{
    protected Pawn talkablePawn;
    protected string talkLabel;
    protected JobDef talkJob;
    protected bool canTalkNow;

    public Pawn TalkablePawn => talkablePawn;
    public bool CanTalkNow => canTalkNow && !dismissed;

    public LordJob_VisitColonyTalkable() : base() { }
    public LordJob_VisitColonyTalkable(Faction faction, IntVec3 chillSpot, int? durationTicks = null) : base(faction, chillSpot, durationTicks)
    { }

    /// <summary>
    /// 初始化可对话的访问殖民地职责。
    /// </summary>
    public LordJob_VisitColonyTalkable(Pawn talkablePawn, JobDef talkJob, Faction faction, IntVec3 chillSpot, string talkLabel = "OAFrame_TalkWith", int? durationTicks = null) : this(faction, chillSpot, durationTicks)
    {
        this.talkablePawn = talkablePawn;
        SetTalkAction(talkablePawn, talkJob, talkLabel);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref talkablePawn, nameof(talkablePawn));
        Scribe_Defs.Look(ref talkJob, nameof(talkJob));
        Scribe_Values.Look(ref talkLabel, nameof(talkLabel), "OAFrame_TalkWith");
        Scribe_Values.Look(ref canTalkNow, nameof(canTalkNow), defaultValue: false);
    }

    /// <summary>
    /// 设置对话行为。
    /// </summary>
    public void SetTalkAction(Pawn talkablePawn, JobDef talkJob, string talkLabel = "OAFrame_TalkWith", bool initTalkActive = true)
    {
        this.talkJob = talkJob;
        this.talkLabel = talkLabel;
        if (initTalkActive)
        {
            EnableTalk(talkablePawn);
        }
        else
        {
            this.talkablePawn = talkablePawn;
        }
    }

    /// <summary>
    /// 检查是否可以与指定<see cref="Pawn"/>对话。
    /// </summary>
    public bool CanTalkWith(Pawn p)
    {
        return canTalkNow && talkablePawn == p;
    }

    /// <summary>
    /// 启用与指定<see cref="Pawn"/>的对话功能。
    /// </summary>
    public void EnableTalk(Pawn p)
    {
        if (p is null)
        {
            Log.Error("Attempt to enable a LordJobWithTalk with a null Pawn.");
            return;
        }
        talkablePawn = p;
        canTalkNow = true;
    }
    /// <summary>
    /// 启用对话功能。
    /// </summary>
    public void EnableTalk() => EnableTalk(talkablePawn);

    /// <summary>
    /// 禁用对话功能。
    /// </summary>
    public void DisableTalk(bool dismiss)
    {
        canTalkNow = false;
        if (!this.dismissed && dismiss)

        {
            this.dismissed = true;
            if (lord is not null && !lord.ownedPawns.NullOrEmpty())
            {
                foreach (Pawn p in lord.ownedPawns)
                {
                    p.pather?.ResetToCurrentPosition();
                }
            }
        }
    }

    /// <summary>
    /// 检查指定工作是否与目标<see cref="Pawn"/>关联。
    /// </summary>
    public bool IsAssociateJobToPawn(JobDef jobDef, Pawn talkWith)
    {
        return talkWith == talkablePawn && jobDef == talkJob;
    }

    public override void Cleanup()
    {
        base.Cleanup();
        talkablePawn = null;
        talkJob = null;
        talkLabel = null;
        canTalkNow = false;
    }

    public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
    {
        base.Notify_PawnLost(p, condition);
        if (p == talkablePawn)
        {
            DisableTalk(dismiss: true);
        }
    }

    public override IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn target, Pawn forPawn)
    {
        if (canTalkNow && target == talkablePawn)
        {
            yield return FloatMenuUtility.DecoratePrioritizedTask(option: new FloatMenuOption(label: talkLabel.Translate(talkablePawn.LabelShort),
                                                                                              action: delegate { Interact(target, forPawn, talkJob); },
                                                                                              priority: MenuOptionPriority.InitiateSocial,
                                                                                              mouseoverGuiAction: null, revalidateClickTarget: target),
                                                                  pawn: forPawn,
                                                                  target: target);
        }
    }

    private static void Interact(Pawn target, Pawn forPawn, JobDef talkJob)
    {
        Job job = JobMaker.MakeJob(talkJob, target);
        job.playerForced = true;
        forPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
    }
}