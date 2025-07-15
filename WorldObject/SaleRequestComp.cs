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
    protected bool activeSaleRQ;
    public ThingDef saleRQ_ThingDef;
    public int saleRQ_Count;
    public int saleRQ_Expiration = -1;
    public string outSignal_SaleRQGived;

    public bool ActiveRequest => activeSaleRQ && saleRQ_Expiration > Find.TickManager.TicksGame;

    public override string CompInspectStringExtra()
    {
        if (ActiveRequest)
        {
            return "CaravanRequestInfo".Translate(TradeRequestUtility.RequestedThingLabel(saleRQ_ThingDef, saleRQ_Count).CapitalizeFirst(), (saleRQ_Expiration - Find.TickManager.TicksGame).ToStringTicksToDays(), (saleRQ_ThingDef.GetStatValueAbstract(StatDefOf.MarketValue) * saleRQ_Count).ToStringMoney());
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
        saleRQ_ThingDef = thingDef;
        saleRQ_Count = thingCount;
        saleRQ_Expiration = Find.TickManager.TicksGame + expirationDelay;
        activeSaleRQ = true;
    }
    public void Disable()
    {
        activeSaleRQ = false;
        saleRQ_Expiration = -1;
        saleRQ_ThingDef = null;
        saleRQ_Count = 0;
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
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OAFrame_CommandFulfillSaleRQConfirm".Translate(GenLabel.ThingLabel(saleRQ_ThingDef, null, saleRQ_Count)), delegate
                {
                    Fulfill(caravan);
                }));
            }
        };
        return command_Action;
    }
    private void Fulfill(Caravan caravan)
    {
        List<Thing> things = OAFrame_MiscUtility.TryGenerateThing(saleRQ_ThingDef, saleRQ_Count);
        foreach (Thing t in things)
        {
            CaravanInventoryUtility.GiveThing(caravan, t);
        }
        /*
        if (parent.Faction is not null)
        {
            Faction.OfPlayer.TryAffectGoodwillWith(parent.Faction, 12, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.QuestGoodwillReward);
        }
        */
        QuestUtility.SendQuestTargetSignals(parent.questTags, "OAFrame_SaleRequestFulfilled", parent.Named("SUBJECT"), caravan.Named("CARAVAN"));
        Disable();
    }
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref activeSaleRQ, "activeSaleRQ", defaultValue: false);
        Scribe_Values.Look(ref saleRQ_Expiration, "saleRQ_Expiration", -1);
        Scribe_Defs.Look(ref saleRQ_ThingDef, "saleRQ_ThingDef");
        Scribe_Values.Look(ref saleRQ_Count, "saleRQ_Count", 0);
    }
}
