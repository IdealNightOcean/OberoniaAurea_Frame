using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

//类型化物品交易
public class WorldObjectCompProperties_CategoryTradeRequestComp : WorldObjectCompProperties
{
    public WorldObjectCompProperties_CategoryTradeRequestComp()
    {
        compClass = typeof(CategoryTradeRequestComp);
    }
}

[StaticConstructorOnStartup]
public class CategoryTradeRequestComp : WorldObjectComp
{
    protected bool activeCategoryRQ;
    protected ThingCategoryDef categoryRQ_Def;
    protected int categoryRQ_Count;
    protected int categoryRQ_Left;
    protected bool categoryRQ_IsMeat;
    protected bool categoryRQ_AllowInsectMeat;
    protected bool categoryRQ_AllowHumanlikeMeat;
    protected int categoryRQ_Expiration = -1;
    public string outSignal_CategoryRQFulfilled;

    public bool ActiveRequest => activeCategoryRQ && categoryRQ_Expiration > Find.TickManager.TicksGame;

    public virtual void InitTradeRequest(ThingCategoryDef requestedCategoryDef, int requestCount, int requestDuration, bool isMeat = false, bool allowInsectMeat = false, bool allowHumanlikeMeat = false)
    {
        categoryRQ_Def = requestedCategoryDef;
        categoryRQ_Count = requestCount;
        categoryRQ_Left = requestCount;
        categoryRQ_IsMeat = isMeat;
        categoryRQ_AllowInsectMeat = allowInsectMeat;
        categoryRQ_AllowHumanlikeMeat = allowHumanlikeMeat;
        categoryRQ_Expiration = Find.TickManager.TicksGame + requestDuration;
        activeCategoryRQ = true;
    }

    public override string CompInspectStringExtra()
    {
        Log.Message(activeCategoryRQ + " | " + categoryRQ_Expiration);
        if (ActiveRequest)
        {
            return "OAFrame_CaravanCategoryRequestInfo".Translate(RequestedThingCategoryLabel(categoryRQ_Def, categoryRQ_Count, categoryRQ_IsMeat, categoryRQ_AllowInsectMeat, categoryRQ_AllowHumanlikeMeat).CapitalizeFirst(), (categoryRQ_Expiration - Find.TickManager.TicksGame).ToStringTicksToDays());
        }
        return null;
    }

    public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
    {
        if (ActiveRequest && CaravanVisitUtility.SettlementVisitedNow(caravan) == parent)
        {
            yield return FulfillRequestCommand(caravan);
        }
    }

    public void Disable()
    {
        activeCategoryRQ = false;
        categoryRQ_Expiration = -1;
        categoryRQ_Def = null;
        categoryRQ_Count = 0;
        categoryRQ_Left = 0;
        categoryRQ_AllowInsectMeat = false;
        categoryRQ_AllowHumanlikeMeat = false;
    }

    private Command FulfillRequestCommand(Caravan caravan)
    {
        Command_Action command_Action = new()
        {
            defaultLabel = "CommandFulfillTradeOffer".Translate(),
            defaultDesc = "CommandFulfillTradeOfferDesc".Translate(),
            icon = IconUtility.TradeCommandIcon,
            action = delegate
            {
                if (!ActiveRequest)
                {
                    Log.Error("Attempted to fulfill an unavailable request");
                }
                else if (!OberoniaAureaFrameUtility.HasAnyThings(caravan, categoryRQ_Def, PlayerCanGive))
                {
                    Messages.Message("CommandFulfillTradeOfferFailInsufficient".Translate(RequestedThingCategoryLabel(categoryRQ_Def, categoryRQ_Count, categoryRQ_IsMeat, categoryRQ_AllowInsectMeat, categoryRQ_AllowHumanlikeMeat)), MessageTypeDefOf.RejectInput, historical: false);
                }
                else
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OAGene_CommandFulfillCategoryTradeConfirm".Translate(categoryRQ_Def), delegate
                    {
                        Fulfill(caravan);
                    }));
                }
            }
        };
        if (!OberoniaAureaFrameUtility.HasAnyThings(caravan, categoryRQ_Def, PlayerCanGive))
        {
            command_Action.Disable("OAFrame_CommandFulfillCategoryTradeFailInsufficient".Translate(RequestedThingCategoryLabel(categoryRQ_Def, 1, categoryRQ_IsMeat, categoryRQ_AllowInsectMeat, categoryRQ_AllowHumanlikeMeat)));
        }
        return command_Action;
    }

    private void Fulfill(Caravan caravan)
    {
        List<Thing> list = CaravanInventoryUtility.TakeThings(caravan, delegate (Thing thing)
        {
            if (!thing.def.thingCategories.Contains(categoryRQ_Def))
            {
                return 0;
            }
            if (!PlayerCanGive(thing))
            {
                return 0;
            }
            int num = Mathf.Min(categoryRQ_Left, thing.stackCount);
            categoryRQ_Left -= num;
            return num;
        });
        for (int i = 0; i < list.Count; i++)
        {
            list[i].Destroy();
        }
        if (categoryRQ_Left <= 0)
        {
            /*
            if (parent.Faction != null)
            {
                Faction.OfPlayer.TryAffectGoodwillWith(parent.Faction, 12, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.QuestGoodwillReward);
            }
            */
            QuestUtility.SendQuestTargetSignals(parent.questTags, "OAFrame_CategoryRQFulfilled", parent.Named("SUBJECT"), caravan.Named("CARAVAN"));
            Disable();
        }
        else
        {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OAFrame_FulfillCategoryTradePartComplete".Translate(categoryRQ_Def, categoryRQ_Count - categoryRQ_Left, categoryRQ_Left), null));
        }
    }

    private bool PlayerCanGive(Thing thing)
    {
        if (thing.GetRotStage() != RotStage.Fresh)
        {
            return false;
        }
        if (categoryRQ_IsMeat)
        {
            if (FoodUtility.GetFoodKind(thing.def) == FoodKind.Meat)
            {
                MeatSourceCategory meatSource = FoodUtility.GetMeatSourceCategory(thing.def);
                if (!categoryRQ_AllowInsectMeat && meatSource == MeatSourceCategory.Insect)
                {
                    return false;
                }
                if (!categoryRQ_AllowHumanlikeMeat && meatSource == MeatSourceCategory.Humanlike)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref activeCategoryRQ, "activeCategoryRQ", defaultValue: false);
        Scribe_Values.Look(ref categoryRQ_Expiration, "categoryRQ_Expiration", 0);
        Scribe_Defs.Look(ref categoryRQ_Def, "categoryRQ_Def");
        Scribe_Values.Look(ref categoryRQ_Count, "categoryRQ_Count", 0);
        Scribe_Values.Look(ref categoryRQ_Left, "categoryRQ_Left", 0);
        Scribe_Values.Look(ref categoryRQ_IsMeat, "categoryRQ_IsMeat", defaultValue: false);
        Scribe_Values.Look(ref categoryRQ_AllowInsectMeat, "categoryRQ_AllowInsectMeat", defaultValue: false);
        Scribe_Values.Look(ref categoryRQ_AllowHumanlikeMeat, "categoryRQ_AllowHumanlikeMeat", defaultValue: false);
    }

    public static string RequestedThingCategoryLabel(ThingCategoryDef def, int count, bool isMeat, bool allowInsectMeat, bool allowHumanlikeMeat)
    {
        string text = "OAFrame_RequestedThingCategoryLabel".Translate(def.label, count);
        if (isMeat)
        {
            if (!allowInsectMeat)
            {
                text += " (" + "OAFrame_NotInsectMeat".Translate() + ")";
            }

            if (!allowHumanlikeMeat)
            {
                text += " (" + "OAFrame_NotHumanlikeMeat".Translate() + ")";
            }
        }
        return text;
    }
}
