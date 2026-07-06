using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 文本工具类。 
/// </summary>
[StaticConstructorOnStartup]
public static class OAFrame_TextUtility
{
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
}