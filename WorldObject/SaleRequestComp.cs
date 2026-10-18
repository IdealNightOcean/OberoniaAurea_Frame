using OberoniaAurea_Frame.DataLibrary;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 给予玩家型交易的组件属性。 
/// </summary>
public class WorldObjectCompProperties_SaleRequestComp : WorldObjectCompProperties
{
    /// <summary>
    /// 创建给予玩家型交易请求组件属性。
    /// </summary>
    public WorldObjectCompProperties_SaleRequestComp()
    {
        compClass = typeof(SaleRequestComp);
    }
}

/// <summary> 
/// 给予玩家型交易请求组件。 
/// </summary>
public class SaleRequestComp : WorldObjectComp
{
    /// <summary>
    /// 存档加载字段前缀。
    /// </summary>
    protected const string loadPrefix = "saleRQ_";

    /// <summary>
    /// 当前销售请求是否激活。
    /// </summary>
    protected bool active;
    /// <summary>
    /// 请求出售的物品定义。
    /// </summary>
    public ThingDef thingDef;
    /// <summary>
    /// 请求出售的物品数量。
    /// </summary>
    public int count;
    /// <summary>
    /// 请求过期时刻，-1 表示未设置。
    /// </summary>
    public int expiration = -1;

    /// <summary>
    /// 获取当前是否有活跃的销售请求。
    /// </summary>
    public bool ActiveRequest => active && expiration > Find.TickManager.TicksGame;

    /// <inheritdoc/>
    public override string CompInspectStringExtra()
    {
        if (ActiveRequest)
        {
            return "CaravanRequestInfo".Translate(TradeRequestUtility.RequestedThingLabel(thingDef, count).CapitalizeFirst(), (expiration - Find.TickManager.TicksGame).ToStringTicksToDays(), (thingDef.GetStatValueAbstract(StatDefOf.MarketValue) * count).ToStringMoney());
        }
        return null;
    }

    /// <inheritdoc/>
    public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
    {
        if (ActiveRequest)
        {
            yield return FulfillRequestCommand(caravan);
        }
    }

    /// <summary>
    /// 初始化销售请求。
    /// </summary>
    /// <param name="thingDef">要出售的物品定义。</param>
    /// <param name="thingCount">要出售的物品数量。</param>
    /// <param name="expirationDelay">请求持续的游戏刻数。</param>
    public void InitSaleRequest(ThingDef thingDef, int thingCount, int expirationDelay)
    {
        this.thingDef = thingDef;
        count = thingCount;
        expiration = Find.TickManager.TicksGame + expirationDelay;
        active = true;
    }

    /// <summary>
    /// 禁用销售请求。
    /// </summary>
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
            icon = IconLibrary_OAFrame.TradeCommandIcon,
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
        IEnumerable<Thing> things = Utility.OAFrame_ThingUtility.GenerateThingsSplitByStack(thingDef, count);
        Utility.OAFrame_CaravanUtility.GiveThings(caravan, things);

        QuestUtility.SendQuestTargetSignals(parent.questTags, "SaleTradeRequestFulfilled", parent.Named("SUBJECT"), caravan.Named("CARAVAN"));
        Disable();
    }

    /// <summary>
    /// 序列化/反序列化此对象需持久保存的字段。
    /// </summary>
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref active, loadPrefix + nameof(active), defaultValue: false);
        Scribe_Values.Look(ref expiration, loadPrefix + nameof(expiration), -1);
        Scribe_Defs.Look(ref thingDef, loadPrefix + nameof(thingDef));
        Scribe_Values.Look(ref count, loadPrefix + nameof(count), 0);
    }
}