namespace OberoniaAurea_Frame;

/// <summary>
/// UI 数据缓存接口，维护供绘制器使用的数据及其刷新状态
/// </summary>
public interface IDataCache
{
    /// <summary>
    /// 当前数据状态
    /// </summary>
    DataCacheState DataState { get; }

    /// <summary>
    /// 当前数据是否允许绘制（空占位或正常有效数据）
    /// </summary>
    bool CanDraw { get; }

    /// <summary>
    /// 标记数据过期，下次绘制前需要重新刷新
    /// </summary>
    void MarkDirty();

    /// <summary>
    /// 立即刷新数据并更新 <see cref="DataState"/>
    /// </summary>
    void Refresh();
}
