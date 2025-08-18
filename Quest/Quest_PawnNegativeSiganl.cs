using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_PawnNegativeSiganl : QuestNode
{
    [NoTranslate]
    public SlateRef<IEnumerable<string>> negativeSiganl;

    public SlateRef<bool> useCommonSiganls = true;

    public SlateRef<bool> addTag;
    [NoTranslate]
    public SlateRef<string> tagToAdd;

    [NoTranslate]
    public SlateRef<string> outSignal;

    public SlateRef<bool> outOnlyOnce;


    protected override bool TestRunInt(Slate slate)
    {
        return outSignal.GetValue(slate) is not null;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_PawnNegativeSiganl questPart_PawnNegativeSiganl = new()
        {
            negativeSiganls = GenerateNegativeSiganls(slate),
            outSignal = QuestGenUtility.HardcodedSignalWithQuestID(outSignal.GetValue(slate)),
            outOnlyOnce = outOnlyOnce.GetValue(slate)
        };
        QuestGen.quest.AddPart(questPart_PawnNegativeSiganl);
    }

    private List<string> GenerateNegativeSiganls(Slate slate)
    {
        bool addTag = this.addTag.GetValue(slate);
        string tagToAdd = this.tagToAdd.GetValue(slate);
        List<string> inSignals = negativeSiganl.GetValue(slate)?.ToList() ?? [];

        inSignals = ProcessOriginSignals(slate, inSignals, addTag, tagToAdd);

        if (useCommonSiganls.GetValue(slate))
        {
            inSignals.AddRangeUnique(GetCommonNegativeSiganls(addTag, tagToAdd));
        }

        return inSignals;
    }

    private List<string> ProcessOriginSignals(Slate slate, List<string> originSignals, bool addTag, string tagToAdd = null)
    {
        List<string> processedSignals = [];
        if (originSignals.NullOrEmpty())
        {
            return processedSignals;
        }

        addTag = addTag && !tagToAdd.NullOrEmpty();
        foreach (string signal in originSignals)
        {
            if (addTag)
            {
                processedSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID((tagToAdd + "." + signal)));
            }
            else
            {
                processedSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(signal));
            }
        }

        return processedSignals;
    }

    public static List<string> GetCommonNegativeSiganls(bool addTag, string tagToAdd = null)
    {
        addTag = addTag && !tagToAdd.NullOrEmpty();

        List<string> processedSignals = [
                 ProcessSignal("Arrested"),
                 ProcessSignal("BecameMutant"),
                 ProcessSignal("SurgeryViolation"),
                 ProcessSignal("Kidnapped"),
                 ProcessSignal("LeftBehind"),
                 ProcessSignal("Destroyed"),
             ];

        return processedSignals;

        string ProcessSignal(string signalTag)
        {
            if (addTag)
            {
                return QuestGenUtility.HardcodedSignalWithQuestID((tagToAdd + "." + signalTag));
            }
            else
            {
                return QuestGenUtility.HardcodedSignalWithQuestID(signalTag);
            }
        }
    }

}

public class QuestPart_PawnNegativeSiganl : QuestPart
{
    public List<string> negativeSiganls;

    public string outSignal;

    public bool outOnlyOnce;

    private bool outOnce;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref outSignal, "outSignal");
        Scribe_Values.Look(ref outOnlyOnce, "outOnlyOnce", defaultValue: false);
        Scribe_Values.Look(ref outOnce, "outOnce", defaultValue: false);

        Scribe_Collections.Look(ref negativeSiganls, "negativeSiganls", LookMode.Value);
    }

    public override void Cleanup()
    {
        base.Cleanup();
        negativeSiganls = null;
        outSignal = null;
        outOnlyOnce = false;
        outOnce = false;
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        if (negativeSiganls is not null && (!outOnce || !outOnlyOnce))
        {
            if (negativeSiganls.Contains(signal.tag))
            {
                outOnce = true;
                Find.SignalManager.SendSignal(new Signal(outSignal, signal.args));
            }
        }
    }
}
