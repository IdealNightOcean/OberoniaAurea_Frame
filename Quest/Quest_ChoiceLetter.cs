using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea_Frame;

public class QuestNode_ChoiceLetter : QuestNode
{
    protected static Type defaultPartClass = typeof(QuestPart_ChoiceLetter);

    public SlateRef<Type> partClass = defaultPartClass;
    protected virtual Type PartClass
    {
        get
        {
            return partClass.GetValue(QuestGen.slate) ?? defaultPartClass;
        }
    }

    [NoTranslate]
    public SlateRef<string> inSignal;
    public SlateRef<Faction> relatedFaction;
    public SlateRef<LetterDef> letterDef;

    public SlateRef<string> label;
    public SlateRef<string> text;

    public SlateRef<RulePack> labelRules;
    public SlateRef<RulePack> textRules;

    public SlateRef<int> delayTicks;

    public SlateRef<bool> filterDeadPawnsFromLookTargets;
    public SlateRef<IEnumerable<object>> lookTargets;
    public SlateRef<QuestPart.SignalListenMode?> signalListenMode;

    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_ChoiceLetter questPart_ChoiceLetter = (QuestPart_ChoiceLetter)Activator.CreateInstance(PartClass);

        questPart_ChoiceLetter.InSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
        questPart_ChoiceLetter.LetterDef = letterDef.GetValue(slate) ?? LetterDefOf.NeutralEvent;
        questPart_ChoiceLetter.RawLabel = "error";
        questPart_ChoiceLetter.RawText = "error";

        questPart_ChoiceLetter.DelayTicks = delayTicks.GetValue(slate);

        questPart_ChoiceLetter.FilterDeadPawnsFromLookTargets = filterDeadPawnsFromLookTargets.GetValue(slate);
        questPart_ChoiceLetter.LookTargets = QuestGenUtility.ToLookTargets(lookTargets, slate);
        questPart_ChoiceLetter.RelatedFaction = relatedFaction.GetValue(slate);

        questPart_ChoiceLetter.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
        questPart_ChoiceLetter.InitLetterTextRequest(label.GetValue(slate), text.GetValue(slate), labelRules.GetValue(slate), textRules.GetValue(slate));

        PostGeneratePart(questPart_ChoiceLetter);
        QuestGen.quest.AddPart(questPart_ChoiceLetter);
    }

    protected virtual void PostGeneratePart(QuestPart_ChoiceLetter questPart_ChoiceLetter) { }
}

public class QuestPart_ChoiceLetter : QuestPart
{
    protected const string RootSymbol = "root";

    public string InSignal;

    public string RawLabel;
    public string RawText;
    public LetterDef LetterDef;

    public int DelayTicks;

    public bool GetLookTargetsFromSignal;
    public LookTargets LookTargets;
    public Faction RelatedFaction;

    public bool FilterDeadPawnsFromLookTargets;

    public void InitLetterTextRequest(string label, string text, RulePack labelRules = null, RulePack textRules = null)
    {
        Slate slate = QuestGen.slate;
        QuestGen.AddTextRequest(RootSymbol, delegate (string x)
        {
            RawLabel = x;
        }, QuestGenUtility.MergeRules(labelRules, label, RootSymbol));
        QuestGen.AddTextRequest(RootSymbol, delegate (string x)
        {
            RawText = x;
        }, QuestGenUtility.MergeRules(textRules, text, RootSymbol));
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref InSignal, nameof(InSignal));

        Scribe_Values.Look(ref RawLabel, nameof(RawLabel));
        Scribe_Values.Look(ref RawText, nameof(RawText));
        Scribe_Defs.Look(ref LetterDef, nameof(LetterDef));

        Scribe_Values.Look(ref DelayTicks, nameof(DelayTicks), 0);

        Scribe_Values.Look(ref GetLookTargetsFromSignal, nameof(GetLookTargetsFromSignal), defaultValue: true);
        Scribe_Deep.Look(ref LookTargets, nameof(LookTargets));
        Scribe_References.Look(ref RelatedFaction, nameof(RelatedFaction));

        Scribe_Values.Look(ref FilterDeadPawnsFromLookTargets, nameof(FilterDeadPawnsFromLookTargets), defaultValue: false);
    }

    public override void Cleanup()
    {
        base.Cleanup();
        InSignal = null;

        RawLabel = null;
        RawText = null;
        LetterDef = null;

        LookTargets = null;
        RelatedFaction = null;
    }

    public override IEnumerable<GlobalTargetInfo> QuestLookTargets
    {
        get
        {
            foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
            {
                yield return questLookTarget;
            }
            if (LookTargets is not null)
            {
                GlobalTargetInfo globalTargetInfo = LookTargets.TryGetPrimaryTarget();
                if (globalTargetInfo.IsValid)
                {
                    yield return globalTargetInfo;
                }
            }
        }
    }

    public override IEnumerable<Faction> InvolvedFactions
    {
        get
        {
            foreach (Faction involvedFaction in base.InvolvedFactions)
            {
                yield return involvedFaction;
            }
            if (RelatedFaction is not null)
            {
                yield return RelatedFaction;
            }
        }
    }

    public override void Notify_FactionRemoved(Faction faction)
    {
        if (RelatedFaction == faction)
        {
            RelatedFaction = null;
        }
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        if (signal.tag != InSignal)
        {
            return;
        }

        LookTargets sourceTargets = null;
        if (LookTargets.IsValid())
        {
            sourceTargets = LookTargets;
        }
        else if (GetLookTargetsFromSignal)
        {
            SignalArgsUtility.TryGetLookTargets(signal.args, "SUBJECT", out sourceTargets);
        }

        LookTargets lookTargets = null;
        if (sourceTargets.IsValid())
        {
            List<GlobalTargetInfo> targetInfos = [];
            foreach (GlobalTargetInfo targetInfo in sourceTargets.targets)
            {
                if (!targetInfo.IsValid)
                {
                    continue;
                }
                if (FilterDeadPawnsFromLookTargets && targetInfo.Thing is Pawn p && p.Dead)
                {
                    continue;
                }
                targetInfos.Add(targetInfo);
            }
            if (targetInfos.Count > 0)
            {
                lookTargets = new LookTargets(targetInfos);
            }
        }

        ChoiceLetter choiceLetter = LetterMaker.MakeLetter(
            label: signal.args.GetFormattedText(RawLabel),
            text: signal.args.GetFormattedText(RawText),
            def: LetterDef,
            lookTargets: lookTargets,
            relatedFaction: RelatedFaction,
            quest: quest);

        bool letterValid = !string.IsNullOrEmpty(choiceLetter.Text);
        PostGenerateLetter(choiceLetter, out bool postValid);
        if (letterValid && postValid)
        {
            Find.LetterStack.ReceiveLetter(choiceLetter, delayTicks: DelayTicks);
        }
    }

    protected virtual void PostGenerateLetter(ChoiceLetter choiceLetter, out bool letterValid) { letterValid = true; }
}
