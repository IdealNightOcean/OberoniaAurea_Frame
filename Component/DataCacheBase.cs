using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// UI 数据缓存基类，提供 <see cref="IDataCache"/> 的通用状态管理
/// </summary>
public abstract class DataCacheBase : IDataCache
{
    /// <summary>
    /// 当前数据状态
    /// </summary>
    public DataCacheState DataState { get; protected set; } = DataCacheState.Dirty;

    /// <summary>
    /// 数据有效可用
    /// </summary>
    public bool IsDataValid => DataState == DataCacheState.Ready;

    /// <summary>
    /// 允许绘制：空占位 / 正常有效数据
    /// </summary>
    public virtual bool CanDraw => DataState is DataCacheState.Empty or DataCacheState.Ready;

    /// <summary>
    /// 标记数据过期，下次绘制前应当刷新
    /// </summary>
    public void MarkDirty() => DataState = DataCacheState.Dirty;

    /// <summary>
    /// 刷新数据并更新 <see cref="DataState"/>
    /// </summary>
    /// <remarks>
    /// 若 <see cref="RefreshInner"/> 错误地返回 <see cref="DataCacheState.Dirty"/>，会记录错误日志并将状态回退为 <see cref="DataCacheState.Invalid"/>。
    /// </remarks>
    public void Refresh()
    {
        DataState = RefreshInner();

        if (DataState == DataCacheState.Dirty)
        {
            Log.Error($"[OARO] {GetType().Name}.RefreshInner 返回 {nameof(DataCacheState.Dirty)} 。{nameof(DataCacheState.Dirty)} 仅用作待刷新标记，不能作为刷新完成结果。");
            DataState = DataCacheState.Invalid;
        }
    }

    /// <summary>
    /// 执行实际数据校验逻辑
    /// <para>不可返回 <see cref="DataCacheState.Dirty"/>；该状态仅用于外部标记数据过期，不能作为刷新完成结果。</para>
    /// </summary>
    /// <returns>刷新完成后的业务状态</returns>
    protected abstract DataCacheState RefreshInner();
}
