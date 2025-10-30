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

public class CategoryTradeRequestComp : WorldObjectComp
{
    protected const string loadPrefix = "categoryRQ_";

    protected bool active;
    protected ThingCategoryDef category;
    protected int count;
    protected int countLeft;
    protected bool allowInsectMeat;
    protected bool allowHumanlikeMeat;
    protected int expiration = -1;

    public bool ActiveRequest => active && expiration > Find.TickManager.TicksGame;

    public virtual void InitTradeRequest(ThingCategoryDef requestedCategoryDef, int requestedCount, int requestedDuration, bool allowInsectMeat = false, bool allowHumanlikeMeat = false)
    {
        category = requestedCategoryDef;
        count = requestedCount;
        countLeft = requestedCount;
        this.allowInsectMeat = allowInsectMeat;
        this.allowHumanlikeMeat = allowHumanlikeMeat;
        expiration = Find.TickManager.TicksGame + requestedDuration;
        active = true;
    }

    public override string CompInspectStringExtra()
    {
        if (ActiveRequest)
        {
            return "OAFrame_CaravanCategoryRequestInfo".Translate(RequestedThingCategoryLabel(category, countLeft, allowInsectMeat, allowHumanlikeMeat).CapitalizeFirst(), (expiration - Find.TickManager.TicksGame).ToStringTicksToDays());
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

    public void Disable()
    {
        active = false;
        expiration = -1;
        category = null;
        count = 0;
        countLeft = 0;
        allowInsectMeat = false;
        allowHumanlikeMeat = false;
    }

    private Command_Action FulfillRequestCommand(Caravan caravan)
    {
        Command_Action cmmand_Action = new()
        {
            defaultLabel = "CommandFulfillTradeOffer".Translate(),
            defaultDesc = "CommandFulfillTradeOfferDesc".Translate(),
            icon = OAFrame_IconUtility.TradeCommandIcon,
            action = delegate
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OAFrame_CommandFulfillCategoryTradeConfirm".Translate(category), delegate
                {
                    Fulfill(caravan);
                }));
            }
        };
        if (!caravan.HasAnyThingOfCategory(category, PlayerCanGive))
        {
            cmmand_Action.Disable("OAFrame_CommandFulfillCategoryTradeFailInsufficient".Translate(RequestedThingCategoryLabel(category, 1, allowInsectMeat, allowHumanlikeMeat)));
        }
        return cmmand_Action;
    }

    private void Fulfill(Caravan caravan)
    {
        List<Thing> list = CaravanInventoryUtility.TakeThings(caravan, delegate (Thing thing)
        {
            if (!thing.def.IsWithinCategory(category))
            {
                return 0;
            }
            if (!PlayerCanGive(thing))
            {
                return 0;
            }
            int num = Mathf.Min(countLeft, thing.stackCount);
            countLeft -= num;
            return num;
        });
        for (int i = 0; i < list.Count; i++)
        {
            list[i].Destroy();
        }
        if (countLeft <= 0)
        {
            QuestUtility.SendQuestTargetSignals(parent.questTags, "CategoryTradeRequestFulfilled", parent.Named("SUBJECT"), caravan.Named("CARAVAN"));
            Disable();
        }
        else
        {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OAFrame_FulfillCategoryTradePartComplete".Translate(category, count - countLeft, countLeft), null));
        }
    }

    private bool PlayerCanGive(Thing thing)
    {
        if (thing.def.rotatable && thing.GetRotStage() != RotStage.Fresh)
        {
            return false;
        }

        if (category == ThingCategoryDefOf.MeatRaw)
        {
            if (!thing.def.IsMeat)
            {
                return false;
            }
            MeatSourceCategory meatSource = FoodUtility.GetMeatSourceCategory(thing.def);
            return meatSource switch
            {
                MeatSourceCategory.NotMeat => false,
                MeatSourceCategory.Undefined => true,
                MeatSourceCategory.Insect => allowInsectMeat,
                MeatSourceCategory.Humanlike => allowHumanlikeMeat,
                _ => true,
            };
        }
        return true;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref active, loadPrefix + "active", defaultValue: false);
        Scribe_Values.Look(ref expiration, loadPrefix + "expiration", 0);
        Scribe_Defs.Look(ref category, loadPrefix + "category");
        Scribe_Values.Look(ref count, loadPrefix + "count", 0);
        Scribe_Values.Look(ref countLeft, loadPrefix + "countLeft", 0);
        Scribe_Values.Look(ref allowInsectMeat, loadPrefix + "allowInsectMeat", defaultValue: false);
        Scribe_Values.Look(ref allowHumanlikeMeat, loadPrefix + "allowHumanlikeMeat", defaultValue: false);
    }

    public static string RequestedThingCategoryLabel(ThingCategoryDef categoryDef, int count, bool allowInsectMeat, bool allowHumanlikeMeat)
    {
        string text = "OAFrame_RequestedThingCategoryLabel".Translate(categoryDef.label, count);
        if (categoryDef == ThingCategoryDefOf.MeatRaw)
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
