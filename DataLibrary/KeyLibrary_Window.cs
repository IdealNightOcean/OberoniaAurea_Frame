using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame.DataLibrary;

/// <summary>
/// 窗口相关字符串值常量库。
/// </summary>
[StaticConstructorOnStartup]
public static class KeyLibrary_Window
{
    /// <summary> 
    /// 确认 
    /// </summary>
    public const string Confirm = "Confirm";
    /// <summary> 
    /// 取消 
    /// </summary>
    public const string Cancel = "Cancel";
    /// <summary> 
    /// 返回 
    /// </summary>
    public const string GoBack = "GoBack";

    /// <summary> 
    /// 绿色确认文本
    /// </summary>
    public static string ConfirmGreen => Confirm.Colorize(Color.green);
    /// <summary> 
    /// 红色确认文本 
    /// </summary>
    public static string ConfirmRed => Confirm.Colorize(ColorLibrary.RedReadable);

    /// <summary> 
    /// 红色取消文本 
    /// </summary>
    public static string CancelRed => Cancel.Colorize(ColorLibrary.RedReadable);
}