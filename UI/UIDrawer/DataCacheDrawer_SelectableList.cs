using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame.UI;

/// <summary>
/// 可选择列表绘制器，将 <see cref="DrawDatas"/> 按网格布局绘制并支持单选（再次点击取消选择）
/// </summary>
public class DataCacheDrawer_SelectableList<T, U> : UIDrawerBase where T : IDataCache where U : DataCacheDrawerBase<T>
{
    /// <summary>
    /// 条目布局遍历信息
    /// </summary>
    protected struct EntryDrawInfo
    {
        /// <summary>所在行（从 1 开始）</summary>
        public int Row;
        /// <summary>所在列（从 1 开始）</summary>
        public int Column;
        /// <summary>条目左上角绘制位置</summary>
        public Vector2 EntryPos;
        /// <summary>条目是否与当前可视区域相交（用于视口裁剪）</summary>
        public bool Visible;
    }

    private const float ScrollBarThickness = 20f;

    /// <summary>
    /// 用于绘制单个条目的绘制器
    /// </summary>
    public U Drawer { get; protected set; }

    /// <summary>
    /// 列表数据源
    /// </summary>
    public IList<T> DrawDatas { get; protected set; }

    private int selectedIndex = -1;
    /// <summary>
    /// 当前选中条目索引，-1 表示未选中；赋值时会自动规整到合法范围
    /// </summary>
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (DrawDatas is null || DrawDatas.Count == 0)
            {
                selectedIndex = -1;
                return;
            }

            if (value < 0)
            {
                selectedIndex = -1;
                return;
            }

            if (selectedIndex >= DrawDatas.Count)
            {
                selectedIndex = DrawDatas.Count - 1;
                return;
            }

            selectedIndex = value;
        }
    }

    /// <summary>
    /// 是否存在已选中的条目
    /// </summary>
    public bool HasSelectedItem => SelectedIndex >= 0 && SelectedIndex < DrawDatas.Count;

    /// <summary>
    /// 当前选中的数据条目，未选中或无数据时返回默认值
    /// </summary>
    public T SelectedItem
    {
        get
        {
            if (DrawDatas is null || DrawDatas.Count == 0)
                return default;
            if (SelectedIndex < 0 || SelectedIndex >= DrawDatas.Count)
                return default;
            return DrawDatas[SelectedIndex];
        }
    }

    private int rowLimit = -1;
    /// <summary>
    /// 行数上限：横向滚动时每列最多排列的行数，-1 表示不限
    /// </summary>
    public int RowLimit
    {
        get => rowLimit;
        set
        {
            rowLimit = value;
            LayoutSizeChanged = true;
        }
    }

    private int columnLimit = -1;
    /// <summary>
    /// 列数上限：纵向滚动时每行最多排列的列数，-1 表示不限
    /// </summary>
    public int ColumnLimit
    {
        get => columnLimit;
        set
        {
            columnLimit = value;
            LayoutSizeChanged = true;
        }
    }

    private bool horizontalScroll = false;
    /// <summary>
    /// 是否横向滚动排列，<see langword="false"/> 时为纵向滚动
    /// </summary>
    public bool HorizontalScroll
    {
        get => horizontalScroll;
        set
        {
            horizontalScroll = value;
            LayoutSizeChanged = true;
        }
    }

    private bool showScrollBar = true;
    /// <summary>
    /// 是否显示滚动条并为其预留尺寸
    /// </summary>
    public bool ShowScrollBar
    {
        get => showScrollBar;
        set
        {
            showScrollBar = value;
            LayoutSizeChanged = true;
        }
    }

    /// <summary>是否绘制鼠标悬停高亮</summary>
    public bool DrawMouseOverHighlight = true;
    /// <summary>是否为选中条目绘制选框</summary>
    public bool DrawSelectedBox = true;
    /// <summary>是否为选中条目绘制选中高亮</summary>
    public bool DrawSelectedHighlight = true;

    /// <summary>是否允许点击已选中条目再次取消选择</summary>
    public bool EnableDeselect = true;

    private Vector2 itemInterval = Vector2.zero;
    /// <summary>
    /// 条目之间的水平与垂直间隔
    /// </summary>
    public Vector2 ItemInterval
    {
        get => itemInterval;
        set
        {
            itemInterval = value;
            LayoutSizeChanged = true;
        }
    }

    private ScrollLayoutStrategy layoutStrategy = ScrollLayoutStrategy.ViewGiven;
    /// <summary>
    /// 滚动区域的布局计算策略
    /// </summary>
    public ScrollLayoutStrategy LayoutStrategy
    {
        get => layoutStrategy;
        set
        {
            layoutStrategy = value;
            LayoutSizeChanged = true;
        }
    }

    /// <summary>
    /// 布局参数是否已变更，需要在下次绘制时重新计算
    /// </summary>
    protected bool LayoutSizeChanged { get; set; } = true;

    /// <summary>当前滚动位置</summary>
    protected Vector2 scrollPosition = Vector2.zero;
    /// <summary>是否正处于选择流程中，用于防止重入</summary>
    protected bool onSelecting = false;

    private Vector2 outRectSize = new(-1f, -1f);
    /// <summary>
    /// 滚动视口（外框）尺寸，首次访问或布局失效时懒计算
    /// </summary>
    protected Vector2 OutRectSize
    {
        get
        {
            if (outRectSize.x < 0f || outRectSize.y < 0f)
            {
                RefreshOutRectSize();
            }

            return outRectSize;
        }
    }

    protected Vector2 entryDrawSize = new(-1f, -1f);
    /// <summary>
    /// 单个条目的绘制尺寸，首次访问或布局失效时懒计算
    /// </summary>
    protected Vector2 EntryDrawSize
    {
        get
        {
            if (entryDrawSize.x < 0f || entryDrawSize.y < 0f)
            {
                RefreshEntryDrawSize();
            }

            return entryDrawSize;
        }
    }


    /// <summary>
    /// 选中项变化事件；参数依次为：旧的选择索引、新的选择索引（-1 表示取消选择）
    /// </summary>
    public EventDispatcher<Action<int, int>> OnSelectedItem { get; } = new();

    /// <summary>
    /// 创建可选择列表绘制器
    /// </summary>
    /// <param name="drawer">用于绘制单个条目的绘制器</param>
    /// <param name="drawDatas">列表数据源</param>
    public DataCacheDrawer_SelectableList(U drawer, IList<T> drawDatas)
    {
        Drawer = drawer;
        DrawDatas = drawDatas;
        LayoutSizeChanged = true;
    }

    /// <summary>
    /// 替换条目绘制器，同时标记布局失效并重置选择
    /// </summary>
    /// <param name="drawer">新的条目绘制器</param>
    public virtual void SetDrawer(U drawer)
    {
        Drawer = drawer;
        LayoutSizeChanged = true;
        ResetSelection();
    }

    /// <summary>
    /// 替换列表数据源并重置选择
    /// </summary>
    /// <param name="drawDatas">新的数据源</param>
    public virtual void SetDrawDatas(IList<T> drawDatas)
    {
        DrawDatas = drawDatas;
        ResetSelection();
    }

    /// <summary>
    /// 清除当前选择
    /// </summary>
    public virtual void ResetSelection()
    {
        SelectItem(-1);
    }

    /// <summary>
    /// 选择指定索引的条目；在 <see cref="EnableDeselect"/> 开启时再次选择已选中条目会取消选择
    /// </summary>
    /// <param name="index">目标索引，-1 表示取消选择</param>
    /// <param name="applySelectionEvent">是否触发 <see cref="OnSelectedItem"/> 事件</param>
    /// <returns>选择索引是否发生变化</returns>
    public bool SelectItem(int index, bool applySelectionEvent = true)
    {
        if (onSelecting)
            return false;

        onSelecting = true;
        bool result = DoSelectItem(index, applySelectionEvent);
        onSelecting = false;
        return result;
    }


    /// <summary>
    /// 执行实际选择变更逻辑，可由子类重写
    /// </summary>
    /// <param name="index">目标索引</param>
    /// <param name="applySelectionEvent">是否触发 <see cref="OnSelectedItem"/> 事件</param>
    /// <returns>选择索引是否发生变化</returns>
    protected virtual bool DoSelectItem(int index, bool applySelectionEvent = true)
    {
        int oldSelectedIndex = SelectedIndex;
        if (index >= 0 && DrawDatas is not null && index < DrawDatas.Count)
        {
            SelectedIndex = (EnableDeselect && SelectedIndex == index) ? -1 : index;
            if (applySelectionEvent)
                ApplyOnSelectedItem(oldSelectedIndex, SelectedIndex);
            return true;
        }
        else if (SelectedIndex != -1)
        {
            SelectedIndex = -1;
            if (applySelectionEvent)
                ApplyOnSelectedItem(oldSelectedIndex, SelectedIndex);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 触发 <see cref="OnSelectedItem"/> 事件
    /// </summary>
    /// <param name="oldSelectedIndex">变更前的选择索引</param>
    /// <param name="newSelectedIndex">变更后的选择索引</param>
    public void ApplyOnSelectedItem(int oldSelectedIndex, int newSelectedIndex)
    {
        OnSelectedItem.Raise(handler => handler(oldSelectedIndex, newSelectedIndex));
    }

    private void RefreshOutRectSize()
    {
        Vector2 newOutRectSize = ValidDrawSize;
        if (Drawer is null)
        {
            outRectSize = newOutRectSize;
            return;
        }

        if (LayoutStrategy != ScrollLayoutStrategy.ViewDerivedByRowCol)
        {
            outRectSize = newOutRectSize;
            return;
        }
        else
        {
            Vector2 entrySize = Drawer.DrawSize;
            float outlineThickness = Drawer.OutlineThickness;
            float stepX = entrySize.x + ItemInterval.x - outlineThickness;
            float stepY = entrySize.y + ItemInterval.y - outlineThickness;

            if (stepX <= 0f || stepY <= 0f)
            {
                outRectSize = newOutRectSize;
                return;
            }

            if (ColumnLimit > 0)
                newOutRectSize.x = ColumnLimit * stepX + outlineThickness;
            if (RowLimit > 0)
                newOutRectSize.y = RowLimit * stepY + outlineThickness;

            Rect viewRect = GetViewRect();
            if (ShowScrollBar)
            {
                if (HorizontalScroll)
                {
                    if (viewRect.width > newOutRectSize.x)
                    {
                        newOutRectSize.y += ScrollBarThickness;
                    }
                }
                else
                {
                    if (viewRect.height > newOutRectSize.y)
                    {
                        newOutRectSize.x += ScrollBarThickness;
                    }
                }
            }

            if (ShowScrollBar)
            {
                if (HorizontalScroll)
                    newOutRectSize.y = Mathf.Max(newOutRectSize.y, ScrollBarThickness);
                else
                    newOutRectSize.x = Mathf.Max(newOutRectSize.x, ScrollBarThickness);
            }
            else
            {
                newOutRectSize.x = Mathf.Max(newOutRectSize.x, 0f);
                newOutRectSize.y = Mathf.Max(newOutRectSize.y, 0f);
            }

            outRectSize = newOutRectSize;
        }
    }

    /// <summary>
    /// 重新计算单个条目的绘制尺寸并写回 <see cref="EntryDrawSize"/>；在 <see cref="ScrollLayoutStrategy.ViewGivenItemAdapt"/> 策略下会按行列约束等比适配条目绘制器尺寸
    /// </summary>
    public void RefreshEntryDrawSize()
    {
        if (LayoutStrategy != ScrollLayoutStrategy.ViewGivenItemAdapt)
        {
            entryDrawSize = Drawer?.DrawSize ?? Vector2.zero;
            return;
        }

        if (Drawer is null)
        {
            entryDrawSize = Vector2.zero;
            return;
        }

        Vector2 validOutRectSize = ValidDrawSize;
        if (ShowScrollBar)
        {
            if (HorizontalScroll)
                validOutRectSize.y -= ScrollBarThickness;
            else
                validOutRectSize.x -= ScrollBarThickness;
        }

        if (validOutRectSize.x < 1e-6f || validOutRectSize.y < 1e-6f)
        {
            entryDrawSize = Vector2.zero;
            return;
        }


        if (HorizontalScroll)
        {
            if (RowLimit > 0)
            {
                float validHeight = validOutRectSize.y - itemInterval.y * (RowLimit - 1);
                if (validHeight <= 1e-6f)
                {
                    entryDrawSize = Vector2.zero;
                    return;
                }

                float entryHeight = validHeight / RowLimit;
                Drawer.SetDrawSizeAspectFit(new(validOutRectSize.x, entryHeight));
            }
        }
        else
        {
            if (ColumnLimit > 0)
            {
                float validWidth = validOutRectSize.x - itemInterval.x * (ColumnLimit - 1);
                if (validWidth <= 1e-6f)
                {
                    entryDrawSize = Vector2.zero;
                    return;
                }

                float entryWidth = validWidth / ColumnLimit;
                Drawer.SetDrawSizeAspectFit(new(entryWidth, validOutRectSize.y));
            }
        }

        entryDrawSize = Drawer.DrawSize;
    }

    /// <summary>
    /// 计算包含全部条目的滚动内容区域（view rect）
    /// </summary>
    /// <returns>全部条目铺开后的总内容区域；数据源为空、绘制器为空或步进尺寸非法时返回 <see cref="Rect.zero"/></returns>
    public Rect GetViewRect()
    {
        if (DrawDatas is null || DrawDatas.Count == 0 || Drawer is null)
            return Rect.zero;

        Vector2 entrySize = Drawer.DrawSize;
        float outlineThickness = Drawer.OutlineThickness;
        float stepX = entrySize.x + ItemInterval.x - outlineThickness;
        float stepY = entrySize.y + ItemInterval.y - outlineThickness;

        if (stepX <= 0f || stepY <= 0f)
            return Rect.zero;

        int totalCount = DrawDatas.Count;
        int totalCols, totalRows;
        if (HorizontalScroll)
        {
            totalRows = RowLimit > 0 ? Math.Min(RowLimit, totalCount) : totalCount;
            totalCols = Mathf.CeilToInt((float)totalCount / totalRows);
        }
        else
        {
            totalCols = ColumnLimit > 0 ? Math.Min(ColumnLimit, totalCount) : totalCount;
            totalRows = Mathf.CeilToInt((float)totalCount / totalCols);
        }

        float totalWidth = Mathf.Max(1e-6f, totalCols * stepX + outlineThickness);
        float totalHeight = Mathf.Max(1e-6f, totalRows * stepY + outlineThickness);

        return new Rect(0f, 0f, totalWidth, totalHeight);
    }

    /// <summary>
    /// 在指定位置绘制带滚动与选择功能的条目网格，视口外的条目会被跳过绘制
    /// </summary>
    /// <param name="position">列表左上角位置</param>
    public void Draw(Vector2 position)
    {
        if (Drawer is null || DrawDatas is null || DrawDatas.Count == 0)
            return;

        if (LayoutSizeChanged)
        {
            ResetLayoutSize();
            RefreshLayoutSize();
        }

        Rect outRect = new(position, outRectSize);
        Rect viewRect = GetViewRect();

        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

        Rect visibleRect = new(scrollPosition, outRect.size);
        EntryDrawInfo entryDrawInfo = new()
        {
            Row = 1,
            Column = 1,
            EntryPos = Vector2.zero,
            Visible = true
        };
        for (int i = 0; i < DrawDatas.Count; i++)
        {
            Rect entryRect = new(entryDrawInfo.EntryPos, Drawer.DrawSize);
            entryDrawInfo.Visible = entryRect.Overlaps(visibleRect);
            if (entryDrawInfo.Visible)
            {
                DrawEntry(entryRect, i);
            }

            entryDrawInfo = UpdateEntrytPosition(entryDrawInfo);
        }

        Widgets.EndScrollView();
    }

    /// <summary>
    /// 绘制单个条目并处理点击选择与高亮效果，可由子类重写
    /// </summary>
    /// <param name="inRect">条目绘制区域</param>
    /// <param name="dataIndex">条目在 <see cref="DrawDatas"/> 中的索引</param>
    protected virtual void DrawEntry(Rect inRect, int dataIndex)
    {
        Drawer.SetDrawData(DrawDatas[dataIndex]);
        Drawer.Draw(inRect.position);

        if (Widgets.ButtonInvisible(inRect))
            SelectItem(dataIndex);

        if (dataIndex == SelectedIndex)
        {
            if (DrawSelectedBox)
                Widgets.DrawBox(inRect);
            if (DrawSelectedHighlight)
                Widgets.DrawHighlightSelected(inRect);
        }
        else if (DrawMouseOverHighlight && Mouse.IsOver(inRect))
            Widgets.DrawHighlight(inRect);
    }

    /// <summary>
    /// 根据当前滚动方向与行列上限，计算下一个条目的行列与位置
    /// </summary>
    /// <param name="entryDrawInfo">当前条目的布局信息</param>
    /// <returns>下一个条目的布局信息</returns>
    protected virtual EntryDrawInfo UpdateEntrytPosition(EntryDrawInfo entryDrawInfo)
    {
        Vector2 entrySize = Drawer.DrawSize;
        float offsetX = entrySize.x + itemInterval.x - Drawer.OutlineThickness;
        float offsetY = entrySize.y + itemInterval.y - Drawer.OutlineThickness;
        if (HorizontalScroll)
        {
            if (RowLimit > 0 && entryDrawInfo.Row >= RowLimit)
            {
                entryDrawInfo.Row = 1;
                entryDrawInfo.Column++;
                entryDrawInfo.EntryPos.x += offsetX;
                entryDrawInfo.EntryPos.y = 0f;
            }
            else
            {
                entryDrawInfo.Row++;
                entryDrawInfo.EntryPos.y += offsetY;
            }
        }
        else
        {
            if (ColumnLimit > 0 && entryDrawInfo.Column >= ColumnLimit)
            {
                entryDrawInfo.Column = 1;
                entryDrawInfo.Row++;
                entryDrawInfo.EntryPos.x = 0f;
                entryDrawInfo.EntryPos.y += offsetY;
            }
            else
            {
                entryDrawInfo.Column++;
                entryDrawInfo.EntryPos.x += offsetX;
            }
        }

        entryDrawInfo.Visible = true;
        return entryDrawInfo;
    }

    /// <summary>
    /// 重新计算视口尺寸与条目尺寸，并清除布局失效标记
    /// </summary>
    protected void RefreshLayoutSize()
    {
        RefreshOutRectSize();
        RefreshEntryDrawSize();
        LayoutSizeChanged = false;
    }

    /// <summary>
    /// 重置布局缓存，使下次绘制时重新计算视口与条目尺寸
    /// </summary>
    public void ResetLayoutSize()
    {
        LayoutSizeChanged = true;
        outRectSize = new(-1f, -1f);
        entryDrawSize = new(-1f, -1f);
    }
}
