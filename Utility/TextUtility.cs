using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_TextUtility
{
    /// <summary>
    /// 创建带颜色和符号的整数字符串。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ColoredIntString(int value, bool reverse = false)
    {
        return value.ToStringWithSign().Colorize((reverse ^ value < 0) ? ColorLibrary.RedReadable : Color.green);
    }
    /// <summary>
    /// 创建带颜色和符号的整数命名参数。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    public static NamedArgument ColoredIntNamedArgument(int value, string name, bool reverse = false) => ColoredIntString(value, reverse).Named(name);

    /// <summary>
    /// 创建带颜色和符号的浮点数字符串。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ColoredFloatString(float value, string format = "0.##", bool reverse = false)
    {
        return value.ToStringWithSign(format).Colorize((reverse ^ value < 0f) ? ColorLibrary.RedReadable : Color.green);
    }
    /// <summary>
    /// 创建带颜色和符号的浮点数命名参数。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    public static NamedArgument ColoredFloatNamedArgument(float value, string name, string format = "0.##", bool reverse = false) => ColoredFloatString(value, format, reverse).Named(name);

    /// <summary>
    /// 创建带颜色和符号的浮点数字符串。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ColoredPercentString(float value, string format = "0.##", bool reverse = false)
    {
        return value.ToStringPercentSigned(format).Colorize((reverse ^ value < 0f) ? ColorLibrary.RedReadable : Color.green);
    }
    /// <summary>
    /// 创建带颜色和符号的百分比数命名参数。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    public static NamedArgument ColoredPercentNamedArgument(float value, string name, string format = "0.##", bool reverse = false) => ColoredPercentString(value, format, reverse).Named(name);
}