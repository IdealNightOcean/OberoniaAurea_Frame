using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_DestroyWorldObject : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;

    public SlateRef<IEnumerable<WorldObject>> worldObjects;

    public SlateRef<bool> destroyOnCleanup = true;
    public SlateRef<bool> onlyDestroyOnCleanup = false;

    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_DestroyWorldObject questPart_DestroyWorldObject = new()
        {
            InSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"),
            DestroyOnCleanup = destroyOnCleanup.GetValue(slate),
            OnlyDestroyOnCleanup = onlyDestroyOnCleanup.GetValue(slate)
        };
        questPart_DestroyWorldObject.InitWorldObjects(worldObjects.GetValue(slate));
        QuestGen.quest.AddPart(questPart_DestroyWorldObject);
    }
}

public class QuestPart_DestroyWorldObject : QuestPart
{
    public bool DestroyOnCleanup = true;
    public bool OnlyDestroyOnCleanup = false;

    public string InSignal;

    public List<WorldObject> worldObjects;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref InSignal, nameof(InSignal));
        Scribe_Values.Look(ref DestroyOnCleanup, nameof(DestroyOnCleanup), defaultValue: true);
        Scribe_Values.Look(ref OnlyDestroyOnCleanup, nameof(OnlyDestroyOnCleanup), defaultValue: false);
        Scribe_Collections.Look(ref worldObjects, nameof(worldObjects), LookMode.Reference);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            worldObjects?.RemoveAll(w => w is null);
        }
    }


    public override void Cleanup()
    {
        base.Cleanup();
        InSignal = null;
        if (DestroyOnCleanup && worldObjects is not null)
        {
            foreach (WorldObject worldObject in worldObjects)
            {
                TryRemove(worldObject);
            }
        }

        worldObjects = null;
    }

    public override IEnumerable<GlobalTargetInfo> QuestLookTargets
    {
        get
        {
            foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
            {
                yield return questLookTarget;
            }

            if (worldObjects is not null)
            {
                foreach (WorldObject worldObject in worldObjects)
                {
                    yield return worldObject;
                }
            }
        }
    }

    public void InitWorldObjects(IEnumerable<WorldObject> worldObjects)
    {
        if (worldObjects is null)
        {
            return;
        }

        this.worldObjects ??= [];
        foreach (WorldObject worldObject in worldObjects)
        {
            this.worldObjects.Add(worldObject);
        }
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (worldObjects is null)
        {
            return;
        }
        if (!OnlyDestroyOnCleanup && signal.tag == InSignal)
        {
            foreach (WorldObject worldObject in worldObjects)
            {
                TryRemove(worldObject);
            }
        }
    }

    public override void AssignDebugData()
    {
        base.AssignDebugData();
        InSignal = "DebugSignal" + Rand.Int;
        if (TileFinder.TryFindNewSiteTile(out PlanetTile tile))
        {
            worldObjects ??= [];
            worldObjects.Add(SiteMaker.MakeSite(
                sitePart: null,
                tile: tile,
                faction: null));
        }
    }

    public static void TryRemove(WorldObject worldObject)
    {
        if (worldObject is not null && worldObject.Spawned)
        {
            if (worldObject is MapParent { HasMap: not false } mapParent)
            {
                mapParent.forceRemoveWorldObjectWhenMapRemoved = true;
            }
            else
            {
                worldObject.Destroy();
            }
        }
    }

}