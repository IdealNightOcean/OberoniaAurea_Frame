using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：选择信件节点。 
/// </summary>
public class QuestNode_ChoiceLetter : QuestNode
{
    /// <summary>
    /// 默认部件类型。
    /// </summary>
    protected static Type defaultPartClass = typeof(QuestPart_ChoiceLetter);

    /// <summary>
    /// 使用的部件类型。
    /// </summary>
    public SlateRef<Type> partClass = defaultPartClass;

    /// <summary>
    /// 最终的部件类型。
    /// </summary>
    protected virtual Type PartClass
    {
        get
        {
            return partClass.GetValue(QuestGen.slate) ?? defaultPartClass;
        }
    }

    /// <summary>
    /// 接收信件的信号。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> inSignal;

    /// <summary>
    /// 关联派系。
    /// </summary>
    public SlateRef<Faction> relatedFaction;

    /// <summary>
    /// 信件定义。
    /// </summary>
    public SlateRef<LetterDef> letterDef;

    /// <summary>
    /// 信件标题。
    /// </summary>
    public SlateRef<string> label;

    /// <summary>
    /// 信件正文。
    /// </summary>
    public SlateRef<string> text;

    /// <summary>
    /// 标题语法规则包。
    /// </summary>
    public SlateRef<RulePack> labelRules;

    /// <summary>
    /// 正文语法规则包。
    /// </summary>
    public SlateRef<RulePack> textRules;

    /// <summary>
    /// 信件延迟送达的刻数。
    /// </summary>
    public SlateRef<int> delayTicks;

    /// <summary>
    /// 定位目标中是否过滤已死亡角色。
    /// </summary>
    public SlateRef<bool> filterDeadPawnsFromLookTargets;

    /// <summary>
    /// 信件定位目标。
    /// </summary>
    public SlateRef<IEnumerable<object>> lookTargets;

    /// <summary>
    /// 信号监听模式。
    /// </summary>
    public SlateRef<QuestPart.SignalListenMode?> signalListenMode;

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate) => true;

    /// <inheritdoc/>
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

    /// <summary>
    /// 生成任务部件后的回调
    /// </summary>
    protected virtual void PostGeneratePart(QuestPart_ChoiceLetter questPart_ChoiceLetter) { }
}

/// <summary> 
/// 任务部件：选择信件部件。 
/// </summary>
public class QuestPart_ChoiceLetter : QuestPart
{
    /// <summary>
    /// 根符号。
    /// </summary>
    protected const string RootSymbol = "root";

    /// <summary>
    /// 触发信件生成的信号。
    /// </summary>
    public string InSignal;

    /// <summary>
    /// 信件原始标题。
    /// </summary>
    public string RawLabel;

    /// <summary>
    /// 信件原始正文。
    /// </summary>
    public string RawText;

    /// <summary>
    /// 信件定义。
    /// </summary>
    public LetterDef LetterDef;

    /// <summary>
    /// 信件延迟送达的刻数。
    /// </summary>
    public int DelayTicks;

    /// <summary>
    /// 是否从信号中获取定位目标。
    /// </summary>
    public bool GetLookTargetsFromSignal;

    /// <summary>
    /// 信件定位目标。
    /// </summary>
    public LookTargets LookTargets;

    /// <summary>
    /// 关联派系。
    /// </summary>
    public Faction RelatedFaction;

    /// <summary>
    /// 定位目标中是否过滤已死亡角色。
    /// </summary>
    public bool FilterDeadPawnsFromLookTargets;

    /// <summary>
    /// 初始化信件文本请求
    /// </summary>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override void Notify_FactionRemoved(Faction faction)
    {
        if (RelatedFaction == faction)
        {
            RelatedFaction = null;
        }
    }

    /// <inheritdoc/>
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

        bool letterValid = !String.IsNullOrEmpty(choiceLetter.Text);
        PostGenerateLetter(choiceLetter, out bool postValid);
        if (letterValid && postValid)
        {
            Find.LetterStack.ReceiveLetter(choiceLetter, delayTicks: DelayTicks);
        }
    }

    /// <summary>
    /// 生成信件后的回调。
    /// </summary>
    /// <param name="choiceLetter">生成的信件。</param>
    /// <param name="letterValid">信件是否有效。</param>
    protected virtual void PostGenerateLetter(ChoiceLetter choiceLetter, out bool letterValid) { letterValid = true; }
}
