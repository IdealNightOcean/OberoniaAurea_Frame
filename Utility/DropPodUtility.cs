using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public static class OAFrame_DropPodUtility
{
    public static IntVec3 DefaultDropThingOfDef(ThingDef def, int count, Map map, Faction faction = null, bool sendLetter = true)
    {
        List<Thing> things = OAFrame_MiscUtility.TryGenerateThing(def, count);
        return DefaultDropThing(things, map, faction, sendLetter);
    }

    public static IntVec3 DefaultDropThing(IEnumerable<Thing> things, Map map, Faction faction = null, bool sendLetter = true)
    {
        IntVec3 dropCell = DropCellFinder.TradeDropSpot(map);
        DropPodUtility.DropThingsNear(dropCell, map, things, canRoofPunch: false, forbid: false, allowFogged: false, faction: faction);
        if (sendLetter)
        {
            SendDropLetter(things, map, dropCell, faction);
        }
        return dropCell;
    }

    public static IntVec3 DefaultDropThingGroups(List<List<Thing>> thingGroups, Map map, Faction faction = null)
    {
        IntVec3 dropCell = DropCellFinder.TradeDropSpot(map);
        DropPodUtility.DropThingGroupsNear(dropCell, map, thingGroups, canRoofPunch: false, forbid: false, allowFogged: false, faction: faction);
        return dropCell;
    }

    public static IntVec3 DefaultDropSingleThingOfDef(ThingDef def, Map map, Faction faction = null, bool sendLetter = true)
    {
        return DefaultDropSingleThing(ThingMaker.MakeThing(def), map, faction, sendLetter);
    }

    public static IntVec3 DefaultDropSingleThing(Thing thing, Map map, Faction faction = null, bool sendLetter = true)
    {
        thing.stackCount = 1;
        IntVec3 dropCell = DropCellFinder.TradeDropSpot(map);
        DropPodUtility.DropThingsNear(dropCell, map, [thing], canRoofPunch: false, forbid: false, allowFogged: false, faction: faction);
        if (sendLetter)
        {
            SendDropLetter([thing], map, dropCell, faction);
        }
        return dropCell;
    }

    private static void SendDropLetter(IEnumerable<Thing> things, Map map, IntVec3 lookCell, Faction faction)
    {
        Find.LetterStack.ReceiveLetter(text: "LetterQuestDropPodsArrived".Translate(GenLabel.ThingsLabel(things)),
                                       label: "LetterLabelQuestDropPodsArrived".Translate(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: new(lookCell, map),
                                       relatedFaction: faction);
    }
}