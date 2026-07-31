using OberoniaAurea_Frame.UI;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 文本工具类。 
/// </summary>
[StaticConstructorOnStartup]
[Obsolete("请使用 OberoniaAurea_Frame.Utility 命名空间下同名工具类。")]
public static class OAFrame_TextUtility
{

    private static readonly GUIContent tempTextGUIContent = new();

    /// <summary>
    /// 根据数值为文本着色（基于值本身）。
    /// </summary>
    /// <param name="oriStr">原始文本</param>
    /// <param name="value">比较数值</param>
    /// <param name="reverse">是否反转颜色逻辑</param>
    /// <param name="originPoint">比较基准点</param>
    public static string ColorizeStrByValue(this TaggedString oriStr, float value, bool reverse = false, float originPoint = 0f)
    {
        Color color = (reverse ^ (value < originPoint)) ? ColorLibrary.RedReadable : Color.green;
        return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{oriStr}</color>";
    }

    /// <summary>
    /// 根据偏移量为文本着色（基于偏移）。
    /// </summary>
    /// <param name="oriStr">原始文本</param>
    /// <param name="value">偏移数值</param>
    /// <param name="reverse">是否反转颜色逻辑</param>
    /// <param name="originPoint">比较基准点</param>
    public static string ColorizeStrByOffset(this TaggedString oriStr, float value, bool reverse = false, float originPoint = 0f)
    {
        Color color = (reverse ^ (value < originPoint)) ? ColorLibrary.RedReadable : Color.green;
        return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{oriStr}</color>";
    }

    /// <summary>
    /// 根据系数为文本着色（基于倍率）。
    /// </summary>
    /// <param name="oriStr">原始文本</param>
    /// <param name="value">系数数值</param>
    /// <param name="reverse">是否反转颜色逻辑</param>
    /// <param name="originPoint">比较基准点（默认 1）</param>
    public static string ColorizeStrByFactor(this TaggedString oriStr, float value, bool reverse = false, float originPoint = 1f)
    {
        Color color = (reverse ^ (value < originPoint)) ? ColorLibrary.RedReadable : Color.green;
        return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{oriStr}</color>";
    }

    /// <summary>
    /// 创建整数命名参数。
    /// </summary>
    /// <param name="value">整数值</param>
    /// <param name="name">参数名称</param>
    /// <param name="includeSign">是否显示符号</param>
    public static NamedArgument IntNamedArgument(int value, string name, bool includeSign = false)
    {
        return (includeSign ? value.ToStringWithSign() : value.ToString()).Named(name);
    }
    /// <summary>
    /// 创建带颜色和符号的整数字符串。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="value">整数值</param>
    /// <param name="includeSign">是否显示符号。为 <see langword="true"/> 时显示，否则不显示。</param>
    /// <param name="originPoint">比较基准点</param>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ColoredIntString(int value, bool includeSign = false, int originPoint = 0, bool reverse = false)
    {
        return (includeSign ? value.ToStringWithSign() : value.ToString()).Colorize((reverse ^ value < originPoint) ? ColorLibrary.RedReadable : Color.green);
    }
    /// <summary>
    /// 创建带颜色和符号的整数命名参数。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="value">整数值</param>
    /// <param name="name">参数名称</param>
    /// <param name="includeSign">是否显示符号。为 <see langword="true"/> 时显示，否则不显示。</param>
    /// <param name="originPoint">比较基准点</param>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    public static NamedArgument ColoredIntNamedArgument(int value, string name, bool includeSign = false, int originPoint = 0, bool reverse = false) => ColoredIntString(value, includeSign, originPoint, reverse).Named(name);

    /// <summary>
    /// 创建带颜色和符号的浮点数字符串。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="value">浮点数值</param>
    /// <param name="format">格式化字符串</param>
    /// <param name="includeSign">是否显示符号。为 <see langword="true"/> 时显示，否则不显示。</param>
    /// <param name="originPoint">比较基准点</param>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ColoredFloatString(float value, string format = "0.##", bool includeSign = false, float originPoint = 0f, bool reverse = false)
    {
        return (includeSign ? value.ToStringWithSign(format) : value.ToString(format)).Colorize((reverse ^ value < originPoint) ? ColorLibrary.RedReadable : Color.green);
    }

    /// <summary>
    /// 创建浮点数命名参数。
    /// </summary>
    /// <param name="value">浮点数值</param>
    /// <param name="name">参数名称</param>
    /// <param name="format">格式化字符串</param>
    /// <param name="includeSign">是否显示符号</param>
    public static NamedArgument FloatNamedArgument(float value, string name, string format = "0.##", bool includeSign = false)
    {
        return (includeSign ? value.ToStringWithSign(format) : value.ToString(format)).Named(name);
    }
    /// <summary>
    /// 创建带颜色和符号的浮点数命名参数。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="value">浮点数值</param>
    /// <param name="name">参数名称</param>
    /// <param name="format">格式化字符串</param>
    /// <param name="includeSign">是否显示符号。为 <see langword="true"/> 时显示，否则不显示。</param>
    /// <param name="originPoint">比较基准点</param>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    public static NamedArgument ColoredFloatNamedArgument(float value, string name, string format = "0.##", bool includeSign = false, float originPoint = 0f, bool reverse = false) => ColoredFloatString(value, format, includeSign, originPoint, reverse).Named(name);

    /// <summary>
    /// 创建百分比命名参数。
    /// </summary>
    /// <param name="value">百分比数值</param>
    /// <param name="name">参数名称</param>
    /// <param name="format">格式化字符串</param>
    /// <param name="includeSign">是否显示符号</param>
    public static NamedArgument PercentNamedArgument(float value, string name, string format = "0.##", bool includeSign = false)
    {
        return (includeSign ? value.ToStringPercentSigned(format) : value.ToStringPercent(format)).Named(name);
    }
    /// <summary>
    /// 创建带颜色和符号的百分比数字符串。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="value">百分比数值（浮点）</param>
    /// <param name="format">格式化字符串</param>
    /// <param name="includeSign">是否显示符号。为 <see langword="true"/> 时显示，否则不显示。</param>
    /// <param name="originPoint">比较基准点</param>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ColoredPercentString(float value, string format = "0.##", bool includeSign = false, float originPoint = 0f, bool reverse = false)
    {
        return (includeSign ? value.ToStringPercentSigned(format) : value.ToStringPercent(format)).Colorize((reverse ^ value < originPoint) ? ColorLibrary.RedReadable : Color.green);
    }
    /// <summary>
    /// 创建带颜色和符号的百分比数命名参数。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="value">百分比数值（浮点）</param>
    /// <param name="name">参数名称</param>
    /// <param name="format">格式化字符串</param>
    /// <param name="includeSign">是否显示符号。为 <see langword="true"/> 时显示，否则不显示。</param>
    /// <param name="originPoint">比较基准点</param>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    public static NamedArgument ColoredPercentNamedArgument(float value, string name, string format = "0.##", bool includeSign = false, float originPoint = 0f, bool reverse = false) => ColoredPercentString(value, format, includeSign, originPoint, reverse).Named(name);


    /// <summary>计算文本在指定宽度下的高度。</summary>
    /// <param name="text">文本内容</param>
    /// <param name="textStyle">文本样式</param>
    /// <param name="width">可用宽度</param>
    /// <returns>文本高度</returns>
    public static float CalcHeight(string text, TextStyle textStyle, float width)
    {
        tempTextGUIContent.text = text;
        float height = textStyle.FontStyle.CalcHeight(tempTextGUIContent, width);
        tempTextGUIContent.text = string.Empty;
        textStyle.RestoreTextStyle();
        return height;
    }

    /// <summary> 计算文本尺寸。</summary>
    /// <param name="text">文本内容</param>
    /// <param name="textStyle">文本样式</param>
    /// <returns>文本尺寸</returns>
    public static Vector2 CalcSize(string text, TextStyle textStyle)
    {
        tempTextGUIContent.text = text;
        Vector2 size = textStyle.FontStyle.CalcSize(tempTextGUIContent);
        tempTextGUIContent.text = string.Empty;
        textStyle.RestoreTextStyle();
        return size;
    }

    /// <summary>使用省略号截断文本以适应矩形区域。</summary>
    /// <param name="rect">矩形区域</param>
    /// <param name="sourceText">源文本</param>
    /// <param name="textStyle">文本样式</param>
    /// <param name="isTextClamped">是否发生截断</param>
    /// <returns>截断后的文本</returns>
    public static string ClampTextWithEllipsis(Rect rect, string sourceText, TextStyle textStyle, out bool isTextClamped)
    {
        isTextClamped = false;
        if (string.IsNullOrEmpty(sourceText))
            return string.Empty;

        // 过短文本直接返回
        if (sourceText.Length <= 4)
            return sourceText;

        float fullRawWidth = CalcSize(sourceText, textStyle).x;
        if (fullRawWidth <= rect.width)
            return sourceText;

        string ellipsis = "...";
        float ellipsisWidth = CalcSize(ellipsis, textStyle).x;
        if (ellipsisWidth >= rect.width)
        {
            isTextClamped = true;
            return ellipsis;
        }

        float maxRawTextWidth = rect.width - ellipsisWidth;

        int left = 0;
        int right = sourceText.Length;
        int bestValidLength = 0;

        // 二分查找最长合法前缀长度
        while (left <= right)
        {
            int mid = (left + right) / 2;
            string subStr = sourceText.Substring(0, mid);
            float curWidth = CalcSize(subStr, textStyle).x;

            if (curWidth <= maxRawTextWidth)
            {
                bestValidLength = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        isTextClamped = true;
        // 如果一个字符都塞不下，只显示省略号
        if (bestValidLength == 0)
            return ellipsis;

        return sourceText.Substring(0, bestValidLength) + ellipsis;
    }
}