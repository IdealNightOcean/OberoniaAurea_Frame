using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

//大地图简单商店
public class SiteTrader : ITrader, IThingHolder, IExposable, ILoadReferenceable, IPawnRetentionHolder
{
    protected static readonly List<string> TempExtantNames = [];

    protected Faction faction;
    public string traderName = "Nameless";
    protected int loadID = -1;
    public TraderKindDef traderKind;
    protected ThingOwner things;
    protected List<Pawn> tmpSavedPawns = [];
    protected WorldObject associateWorldObject;
    protected int randomPriceFactorSeed = -1;

    public Faction Faction => faction;
    public virtual string FullTitle => traderName + " (" + traderKind.label + ")";
    public TraderKindDef TraderKind => traderKind;
    public int Silver => CountHeldOf(ThingDefOf.Silver);
    public int RandomPriceFactorSeed => randomPriceFactorSeed;
    public string TraderName => traderName;
    public bool CanTradeNow => associateWorldObject is not null;
    public float TradePriceImprovementOffsetForPlayer => 0f;
    public TradeCurrency TradeCurrency => traderKind.tradeCurrency;
    public IThingHolder ParentHolder
    {
        get
        {
            if (associateWorldObject is null || !associateWorldObject.Spawned)
            {
                return null;
            }
            return Find.World;
        }
    }
    public string GetInfoText()
    {
        return FullTitle;
    }
    public IEnumerable<Thing> Goods
    {
        get
        {
            for (int i = 0; i < things.Count; i++)
            {
                yield return things[i];
            }
        }
    }

    public SiteTrader() { }

    public SiteTrader(TraderKindDef traderKind, WorldObject worldObject, Faction faction = null)
    {
        this.traderKind = traderKind;
        this.associateWorldObject = worldObject;
        this.faction = faction;

        things = new ThingOwner<Thing>(this);
        TempExtantNames.Clear();
        List<Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++)
        {
            TempExtantNames.AddRange(maps[i].passingShipManager.passingShips.Select(x => x.name));
        }
        traderName = NameGenerator.GenerateName(RulePackDefOf.NamerTraderGeneral, TempExtantNames);
        if (faction is not null)
        {
            traderName = string.Format("{0} {1} {2}", traderName, "OfLower".Translate(), faction.Name);
        }
        randomPriceFactorSeed = Rand.RangeInclusive(1, 10000000);
        loadID = Find.UniqueIDsManager.GetNextPassingShipID();
    }

    public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
    {
        Caravan caravan = playerNegotiator.GetCaravan();
        foreach (Thing item in CaravanInventoryUtility.AllInventoryItems(caravan))
        {
            yield return item;
        }
        List<Pawn> pawns = caravan.PawnsListForReading;
        for (int i = 0; i < pawns.Count; i++)
        {
            if (!caravan.IsOwner(pawns[i]))
            {
                yield return pawns[i];
            }
        }
    }

    public void GenerateThings(int tile)
    {
        ThingSetMakerParams parms = default;
        parms.traderDef = traderKind;
        parms.tile = tile;
        things.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms));
        for (int i = 0; i < things.Count; i++)
        {
            if (things[i] is Pawn pawn)
            {
                Find.WorldPawns.PassToWorld(pawn);
            }
        }
    }

    public void Traderick()
    {
        for (int i = 0; i < things.Count; i++)
        {
            if (things[i] is Pawn p)
            {
                p.Tick();
                if (p.Dead)
                {
                    things.Remove(p);
                }
            }
        }
    }

    public void Destory()
    {
        things.ClearAndDestroyContentsOrPassToWorld();
        tmpSavedPawns.Clear();
    }

    public string GetCallLabel()
    {
        return traderName + " (" + traderKind.label + ")";
    }

    protected AcceptanceReport CanCommunicateWith(Pawn negotiator)
    {
        return negotiator.CanTradeWith(faction, TraderKind).Accepted;
    }

    public int CountHeldOf(ThingDef thingDef, ThingDef stuffDef = null)
    {
        return HeldThingMatching(thingDef, stuffDef)?.stackCount ?? 0;
    }

    public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
    {
        Caravan caravan = playerNegotiator.GetCaravan();
        Thing thing = toGive.SplitOff(countToGive);
        thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
        if (toGive is Pawn pawn)
        {
            CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawn, caravan.PawnsListForReading);
            caravan.RemovePawn(pawn);
        }
        if (!things.TryAdd(thing, canMergeWithExistingStacks: false))
        {
            thing.Destroy();
        }
    }

    public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
    {
        Caravan caravan = playerNegotiator.GetCaravan();
        Thing thing = toGive.SplitOff(countToGive);
        thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
        if (thing is Pawn p)
        {
            caravan.AddPawn(p, addCarriedPawnToWorldPawnsIfAny: true);
            return;
        }
        Pawn pawn = CaravanInventoryUtility.FindPawnToMoveInventoryTo(thing, caravan.PawnsListForReading, null);
        if (pawn is null)
        {
            Log.Error("Could not find any pawn to give sold thing to.");
            thing.Destroy();
        }
        else if (!pawn.inventory.innerContainer.TryAdd(thing))
        {
            Log.Error("Could not add sold thing to inventory.");
            thing.Destroy();
        }
    }

    protected Thing HeldThingMatching(ThingDef thingDef, ThingDef stuffDef)
    {
        for (int i = 0; i < things.Count; i++)
        {
            if (things[i].def == thingDef && things[i].Stuff == stuffDef)
            {
                return things[i];
            }
        }
        return null;
    }

    public void ChangeCountHeldOf(ThingDef thingDef, ThingDef stuffDef, int count)
    {
        Thing thing = HeldThingMatching(thingDef, stuffDef);
        if (thing is null)
        {
            Log.Error("Changing count of thing trader doesn't have: " + thingDef);
        }
        thing.stackCount += count;
    }

    public override string ToString()
    {
        return FullTitle;
    }

    public ThingOwner GetDirectlyHeldThings()
    {
        return things;
    }
    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }
    public void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            tmpSavedPawns.Clear();
            if (things is not null)
            {
                for (int num = things.Count - 1; num >= 0; num--)
                {
                    if (things[num] is Pawn item)
                    {
                        things.Remove(item);
                        tmpSavedPawns.Add(item);
                    }
                }
            }
        }
        Scribe_Values.Look(ref traderName, "traderName");
        Scribe_Values.Look(ref loadID, "loadID", 0);
        Scribe_Defs.Look(ref traderKind, "traderKind");
        Scribe_References.Look(ref associateWorldObject, "associateWorldObject");
        Scribe_References.Look(ref faction, "faction");
        Scribe_Deep.Look(ref things, "things", this);
        Scribe_Collections.Look(ref tmpSavedPawns, "tmpSavedPawns", LookMode.Reference);
        Scribe_Values.Look(ref randomPriceFactorSeed, "randomPriceFactorSeed", 0);
        if (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.Saving)
        {
            tmpSavedPawns.RemoveAll(x => x is null);
            for (int i = 0; i < tmpSavedPawns.Count; i++)
            {
                things.TryAdd(tmpSavedPawns[i], canMergeWithExistingStacks: false);
            }
            tmpSavedPawns.Clear();
        }
    }
    public string GetUniqueLoadID()
    {
        return "OAFrame_SiteTrader_" + loadID;
    }

}
