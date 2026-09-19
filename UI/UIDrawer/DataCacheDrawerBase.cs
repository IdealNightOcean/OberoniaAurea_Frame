using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame.UI;

/// <summary>
/// 数据缓存绘制器基类，将 <typeparamref name="T"/> 类型的数据源与具体绘制逻辑绑定
/// </summary>
/// <typeparam name="T">绑定的数据缓存类型</typeparam>
public abstract class DataCacheDrawerBase<T> : UIDrawerBase where T : IDataCache
{
    /// <summary>
    /// 当前绑定的绘制数据源
    /// </summary>
    protected T DrawData { get; private set; }

    /// <summary>
    /// 设置绘制数据源
    /// </summary>
    /// <param name="drawData">要绑定的数据缓存实例</param>
    public virtual void SetDrawData(T drawData) => this.DrawData = drawData;

    /// <summary>
    /// 执行绘制：数据源为 <see langword="null"/> 时记录错误并跳过；数据为 <see cref="DataCacheState.Dirty"/> 时先刷新；仅在 <see cref="IDataCache.CanDraw"/> 时调用 <see cref="DrawInner"/>
    /// </summary>
    /// <param name="position">绘制起始位置</param>
    public void Draw(Vector2 position)
    {
        if (this.DrawData is null)
        {
            Log.ErrorOnce("[OAFrame] 绘制数据源 DrawData 不能为空，请先设置有效的 DrawData 实例", key: 78433286);
            return;
        }

        if (this.DrawData.DataState == DataCacheState.Dirty)
            this.DrawData.Refresh();

        if (this.DrawData.CanDraw)
            DrawInner(position);

        OberoniaAurea_Frame.UI.OAFrame_UIUtility.ResetTextStyleToDefault();
    }

    /// <summary>
    /// 子类实现的具体绘制逻辑，仅在数据源允许绘制时被调用
    /// </summary>
    /// <param name="position">绘制起始位置</param>
    protected abstract void DrawInner(Vector2 position);

}
