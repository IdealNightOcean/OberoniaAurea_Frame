using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

public class LordJob_VisitColonyTalkable : LordJob_VisitColonyBase
{
    protected Pawn talkablePawn;
    protected string talkLabel;
    protected JobDef talkJob;
    protected bool canTalkNow;

    public Pawn TalkablePawn => talkablePawn;

    public LordJob_VisitColonyTalkable() : base() { }
    public LordJob_VisitColonyTalkable(Faction faction, IntVec3 chillSpot, int? durationTicks = null) : base(faction, chillSpot, durationTicks)
    { }

    public LordJob_VisitColonyTalkable(Pawn talkablePawn, JobDef talkJob, Faction faction, IntVec3 chillSpot, string talkLabel = "OAFrame_TalkWith", int? durationTicks = null) : this(faction, chillSpot, durationTicks)
    {
        this.talkablePawn = talkablePawn;
        SetTalkAction(talkablePawn, talkJob, talkLabel);
    }

    public void SetTalkAction(Pawn talkablePawn, JobDef talkJob, string talkLabel = "OAFrame_TalkWith")
    {
        this.talkablePawn = talkablePawn;
        this.talkJob = talkJob;
        this.talkLabel = talkLabel;
        SetTalkAvailable(true);
    }

    public void SetTalkAvailable(bool canTalkNow)
    {
        this.canTalkNow = canTalkNow;
    }

    public bool IsAssociateJobToPawn(JobDef jobDef, Pawn talkWith)
    {
        return talkWith == talkablePawn && jobDef == talkJob;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref talkablePawn, "talkablePawn");
        Scribe_Defs.Look(ref talkJob, "talkJob");
        Scribe_Values.Look(ref talkLabel, "talkLabel", "OAFrame_TalkWith");
        Scribe_Values.Look(ref canTalkNow, "canTalkNow", defaultValue: false);
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
            SetTalkAvailable(false);
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