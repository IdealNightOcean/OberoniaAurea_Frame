using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 大地图简单商店。
/// </summary>
public class SiteTrader : ITrader, IThingHolder, IExposable, IPawnRetentionHolder
{
    /// <summary>
    /// 现有的名称临时列表，用于生成唯一商人名称。
    /// </summary>
    protected static readonly List<string> TempExtantNames = [];

    /// <summary>
    /// 商人所属派系。
    /// </summary>
    protected Faction faction;
    /// <summary>
    /// 商人名称。
    /// </summary>
    public string traderName = "Nameless";
    /// <summary>
    /// 商人加载标识号。
    /// </summary>
    protected int loadID = -1;
    /// <summary>
    /// 商人类型定义。
    /// </summary>
    public TraderKindDef traderKind;
    /// <summary>
    /// 商人持有的物品所有者。
    /// </summary>
    protected ThingOwner things;
    /// <summary>
    /// 保存的临时小人列表。
    /// </summary>
    protected List<Pawn> tmpSavedPawns = [];
    /// <summary>
    /// 关联的世界对象。
    /// </summary>
    protected WorldObject associateWorldObject;
    /// <summary>
    /// 物品刷新间隔（游戏刻）。
    /// </summary>
    protected int refreshInterval = -1;
    /// <summary>
    /// 上次刷新物品的游戏时刻。
    /// </summary>
    protected int lastRefreshTick = -1;
    /// <summary>
    /// 随机价格因子种子。
    /// </summary>
    protected int randomPriceFactorSeed = -1;

    /// <inheritdoc/>
    public Faction Faction => faction;
    /// <inheritdoc/>
    public virtual string FullTitle => traderName + " (" + traderKind.label + ")";
    /// <inheritdoc/>
    public TraderKindDef TraderKind => traderKind;
    /// <inheritdoc/>
    public int Silver => CountHeldOf(ThingDefOf.Silver);
    /// <inheritdoc/>
    public int RandomPriceFactorSeed => randomPriceFactorSeed;
    /// <inheritdoc/>
    public string TraderName
    {
        get => traderName;
        set => traderName = value;
    }
    /// <inheritdoc/>
    public bool CanTradeNow => associateWorldObject is not null;
    /// <inheritdoc/>
    public float TradePriceImprovementOffsetForPlayer => 0f;
    /// <inheritdoc/>
    public TradeCurrency TradeCurrency => traderKind.tradeCurrency;
    /// <inheritdoc/>
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
    /// <summary>
    /// 获取信息文本。
    /// </summary>
    public string GetInfoText()
    {
        return FullTitle;
    }
    /// <inheritdoc/>
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

    /// <summary>
    /// 默认构造函数。
    /// </summary>
    public SiteTrader() { }

    /// <summary>
    /// 使用商人类型（<see cref="TraderKindDef"/>）、关联世界对象（<see cref="WorldObject"/>）和派系（<see cref="RimWorld.Faction"/>）初始化商店。
    /// </summary>
    /// <param name="traderKind">商人类型定义。</param>
    /// <param name="worldObject">关联的世界对象。</param>
    /// <param name="faction">商人所属派系，可为空。</param>
    /// <param name="rulePack">商人名称生成规则，可为空。</param>
    /// <param name="refreshInterval">物品刷新间隔（游戏刻），为 -1 时不自动刷新。</param>
    public SiteTrader(TraderKindDef traderKind, WorldObject worldObject, Faction faction = null, RulePackDef rulePack = null, int refreshInterval = -1)
    {
        this.traderKind = traderKind;
        associateWorldObject = worldObject;
        this.faction = faction;
        this.refreshInterval = refreshInterval;

        things = new ThingOwner<Thing>(this);
        TempExtantNames.Clear();
        List<Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++)
        {
            TempExtantNames.AddRange(maps[i].passingShipManager.passingShips.Select(x => x.name));
        }
        traderName = NameGenerator.GenerateName(rulePack ?? RulePackDefOf.NamerTraderGeneral, TempExtantNames);
        if (faction is not null)
        {
            traderName = string.Format("{0} {1} {2}", traderName, "OfLower".Translate(), faction.Name);
        }
        randomPriceFactorSeed = Rand.RangeInclusive(1, 10000000);
        loadID = Find.UniqueIDsManager.GetNextPassingShipID();
        this.refreshInterval = refreshInterval;
    }

    /// <summary>
    /// 获取商人愿意购买的殖民地物品。
    /// </summary>
    /// <param name="playerNegotiator">用于交易谈判的玩家小人。</param>
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

    /// <summary>
    /// 每隔指定时间间隔执行刷新检查。
    /// </summary>
    /// <param name="delta">距上次调用的时间间隔。</param>
    public void TickInterval(int delta)
    {
        if (refreshInterval > 0 && Find.TickManager.TicksGame > lastRefreshTick)
        {
            RefreshThings(associateWorldObject?.Tile ?? PlanetTile.Invalid);
        }
    }

    /// <summary>
    /// 生成商店物品。
    /// </summary>
    public void GenerateThings() => GenerateThings(associateWorldObject?.Tile ?? PlanetTile.Invalid);

    /// <summary>
    /// 在指定地块生成商店物品。
    /// </summary>
    /// <param name="tile">生成物品的地块。</param>
    public void GenerateThings(PlanetTile tile)
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
        lastRefreshTick = Find.TickManager.TicksGame;
    }

    /// <summary>
    /// 刷新商店物品。
    /// </summary>
    public void RefreshThings() => RefreshThings(associateWorldObject?.Tile ?? PlanetTile.Invalid);

    /// <summary>
    /// 在指定地块刷新商店物品。
    /// </summary>
    /// <param name="tile">刷新物品的地块。</param>
    public void RefreshThings(PlanetTile tile)
    {
        lastRefreshTick = Find.TickManager.TicksGame;
        things.ClearAndDestroyContentsOrPassToWorld();
        tmpSavedPawns.Clear();
        GenerateThings(tile);
    }

    /// <summary>
    /// 销毁商店并清理物品。
    /// </summary>
    public void Destroy()
    {
        things.ClearAndDestroyContentsOrPassToWorld();
        tmpSavedPawns.Clear();
    }

    /// <summary>
    /// 获取呼叫标签。
    /// </summary>
    public string GetCallLabel() => traderName + " (" + traderKind.label + ")";

    /// <inheritdoc/>
    protected AcceptanceReport CanCommunicateWith(Pawn negotiator) => negotiator.CanTradeWith(faction, TraderKind).Accepted;

    /// <summary>
    /// 计算持有指定物品的数量。
    /// </summary>
    /// <param name="thingDef">要计数的物品定义。</param>
    /// <param name="stuffDef">要计数的材料定义，可为空。</param>
    public int CountHeldOf(ThingDef thingDef, ThingDef stuffDef = null) => HeldThingMatching(thingDef, stuffDef)?.stackCount ?? 0;

    /// <summary>
    /// 将物品出售给商人。
    /// </summary>
    /// <param name="toGive">要出售的物品。</param>
    /// <param name="countToGive">要出售的数量。</param>
    /// <param name="playerNegotiator">用于交易谈判的玩家小人。</param>
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

    /// <summary>
    /// 将物品出售给玩家。
    /// </summary>
    /// <param name="toGive">要出售的物品。</param>
    /// <param name="countToGive">要出售的数量。</param>
    /// <param name="playerNegotiator">用于交易谈判的玩家小人。</param>
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

    /// <summary>
    /// 查找持有中与指定物品定义和材料匹配的物品。
    /// </summary>
    /// <param name="thingDef">要匹配的物品定义。</param>
    /// <param name="stuffDef">要匹配的材料定义。</param>
    /// <returns>匹配的物品，未找到时返回空。</returns>
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

    /// <summary>
    /// 修改持有物品的数量。
    /// </summary>
    /// <param name="thingDef">要修改数量的物品定义。</param>
    /// <param name="stuffDef">要修改数量的材料定义。</param>
    /// <param name="count">要增加到数量的增量值。</param>
    public void ChangeCountHeldOf(ThingDef thingDef, ThingDef stuffDef, int count)
    {
        Thing thing = HeldThingMatching(thingDef, stuffDef);
        if (thing is null)
        {
            Log.Error("Changing count of thing trader doesn't have: " + thingDef);
        }
        thing.stackCount += count;
    }

    /// <inheritdoc/>
    public override string ToString() => FullTitle;

    /// <inheritdoc/>
    public ThingOwner GetDirectlyHeldThings() => things;

    /// <inheritdoc/>
    public void GetChildHolders(List<IThingHolder> outChildren) => ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());

    /// <summary>
    /// 序列化/反序列化此对象需持久保存的字段。
    /// </summary>
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
        Scribe_Values.Look(ref traderName, nameof(traderName));
        Scribe_Values.Look(ref loadID, nameof(loadID), 0);
        Scribe_Defs.Look(ref traderKind, nameof(traderKind));
        Scribe_References.Look(ref associateWorldObject, nameof(associateWorldObject));
        Scribe_References.Look(ref faction, nameof(faction));
        Scribe_Deep.Look(ref things, nameof(things), this);
        Scribe_Collections.Look(ref tmpSavedPawns, nameof(tmpSavedPawns), LookMode.Reference);
        Scribe_Values.Look(ref refreshInterval, nameof(refreshInterval), -1);
        Scribe_Values.Look(ref lastRefreshTick, nameof(lastRefreshTick), -1);
        Scribe_Values.Look(ref randomPriceFactorSeed, nameof(randomPriceFactorSeed), 0);
        if (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.Saving)
        {
            tmpSavedPawns.RemoveAll(p => p is null);
            for (int i = 0; i < tmpSavedPawns.Count; i++)
            {
                things.TryAdd(tmpSavedPawns[i], canMergeWithExistingStacks: false);
            }
            tmpSavedPawns.Clear();
        }
    }
}
