using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

//给予玩家型交易
public class WorldObjectCompProperties_SaleRequestComp : WorldObjectCompProperties
{
    public WorldObjectCompProperties_SaleRequestComp()
    {
        compClass = typeof(SaleRequestComp);
    }
}

public class SaleRequestComp : WorldObjectComp
{
    protected const string loadPrefix = "saleRQ_";

    protected bool active;
    public ThingDef thingDef;
    public int count;
    public int expiration = -1;

    public bool ActiveRequest => active && expiration > Find.TickManager.TicksGame;

    public override string CompInspectStringExtra()
    {
        if (ActiveRequest)
        {
            return "CaravanRequestInfo".Translate(TradeRequestUtility.RequestedThingLabel(thingDef, count).CapitalizeFirst(), (expiration - Find.TickManager.TicksGame).ToStringTicksToDays(), (thingDef.GetStatValueAbstract(StatDefOf.MarketValue) * count).ToStringMoney());
        }
        return null;
    }

    public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
    {
        if (ActiveRequest)
        {
            yield return FulfillRequestCommand(caravan);
        }
    }
    public void InitSaleRequest(ThingDef thingDef, int thingCount, int expirationDelay)
    {
        this.thingDef = thingDef;
        count = thingCount;
        expiration = Find.TickManager.TicksGame + expirationDelay;
        active = true;
    }
    public void Disable()
    {
        active = false;
        expiration = -1;
        thingDef = null;
        count = 0;
    }

    private Command_Action FulfillRequestCommand(Caravan caravan)
    {
        Command_Action command_Action = new()
        {
            defaultLabel = "OAFrame_CommandReciveSaleOffer".Translate(),
            defaultDesc = "OAFrame_CommandReciveSaleOfferDesc".Translate(),
            icon = OAFrame_IconUtility.TradeCommandIcon,
            action = delegate
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OAFrame_CommandFulfillSaleRQConfirm".Translate(GenLabel.ThingLabel(thingDef, null, count)), delegate
                {
                    Fulfill(caravan);
                }));
            }
        };
        return command_Action;
    }

    private void Fulfill(Caravan caravan)
    {
        List<Thing> things = OAFrame_MiscUtility.TryGenerateThing(thingDef, count);
        foreach (Thing t in things)
        {
            CaravanInventoryUtility.GiveThing(caravan, t);
        }
        QuestUtility.SendQuestTargetSignals(parent.questTags, "SaleTradeRequestFulfilled", parent.Named("SUBJECT"), caravan.Named("CARAVAN"));
        Disable();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref active, loadPrefix + "active", defaultValue: false);
        Scribe_Values.Look(ref expiration, loadPrefix + "expiration", -1);
        Scribe_Defs.Look(ref thingDef, loadPrefix + "thingDef");
        Scribe_Values.Look(ref count, loadPrefix + "count", 0);
    }
}