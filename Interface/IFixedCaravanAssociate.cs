using RimWorld.Planet;

namespace OberoniaAurea_Frame;

/// <summary>
/// 固定远行队关联接口。
/// </summary>
public interface IFixedCaravanAssociate
{
    FixedCaravan AssociatedFixedCaravan { get; }
    string FixedCaravanName { get; }
    
    /// <summary>
    /// 玩家主动将<see cref="FixedCaravan"/>转换为<see cref="Caravan"/>的预处理。
    /// </summary>
    void PreConvertToCaravanByPlayer();
    
    /// <summary>
    /// <see cref="FixedCaravan"/>转换为<see cref="Caravan"/>的后处理。
    /// </summary>
    void PostConvertToCaravan(Caravan caravan);
    
    /// <summary>
    /// 获取<see cref="FixedCaravan"/>的工作描述。
    /// </summary>
    string FixedCaravanWorkDesc();
}
