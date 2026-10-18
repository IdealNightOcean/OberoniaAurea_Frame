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
    /// <summary>
    /// 可对话的<see cref="Pawn"/>。
    /// </summary>
    protected Pawn talkablePawn;
    /// <summary>
    /// 对话的翻译标签。
    /// </summary>
    protected string talkLabel;
    /// <summary>
    /// 对话时执行的<see cref="JobDef"/>。
    /// </summary>
    protected JobDef talkJob;
    /// <summary>
    /// 当前是否允许对话。
    /// </summary>
    protected bool canTalkNow;

    /// <summary>
    /// 获取可对话的<see cref="Pawn"/>。
    /// </summary>
    public Pawn TalkablePawn => talkablePawn;
    /// <summary>
    /// 获取当前是否允许对话。
    /// </summary>
    public bool CanTalkNow => canTalkNow && !dismissed;

    /// <summary>
    /// 初始化可对话的访问殖民地职责。
    /// </summary>
    public LordJob_VisitColonyTalkable() : base() { }
    /// <summary>
    /// 根据派系、停留位置和持续时长初始化可对话的访问殖民地职责。
    /// </summary>
    /// <param name="faction">访问殖民地的派系。</param>
    /// <param name="chillSpot">访问期间的停留位置。</param>
    /// <param name="durationTicks">访问持续的世界刻度数。</param>
    public LordJob_VisitColonyTalkable(Faction faction, IntVec3 chillSpot, int? durationTicks = null) : base(faction, chillSpot, durationTicks)
    { }

    /// <summary>
    /// 根据可对话的<see cref="Pawn"/>和对话工作初始化可对话的访问殖民地职责。
    /// </summary>
    /// <param name="talkablePawn">需要对话的<see cref="Pawn"/>。</param>
    /// <param name="talkJob">对话时执行的<see cref="JobDef"/>。</param>
    /// <param name="faction">访问殖民地的派系。</param>
    /// <param name="chillSpot">访问期间的停留位置。</param>
    /// <param name="talkLabel">对话的翻译标签。</param>
    /// <param name="durationTicks">访问持续的世界刻度数。</param>
    public LordJob_VisitColonyTalkable(Pawn talkablePawn, JobDef talkJob, Faction faction, IntVec3 chillSpot, string talkLabel = "OAFrame_TalkWith", int? durationTicks = null) : this(faction, chillSpot, durationTicks)
    {
        this.talkablePawn = talkablePawn;
        SetTalkAction(talkablePawn, talkJob, talkLabel);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override void Cleanup()
    {
        base.Cleanup();
        talkablePawn = null;
        talkJob = null;
        talkLabel = null;
        canTalkNow = false;
    }

    /// <inheritdoc/>
    public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
    {
        base.Notify_PawnLost(p, condition);
        if (p == talkablePawn)
        {
            DisableTalk(dismiss: true);
        }
    }

    /// <inheritdoc/>
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