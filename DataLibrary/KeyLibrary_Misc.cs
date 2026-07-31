using Verse;

namespace OberoniaAurea_Frame.DataLibrary;

/// <summary>
/// 杂项字符串值常量库。
/// </summary>
[StaticConstructorOnStartup]
public static class KeyLibrary_Misc
{
    /// <summary> 
    /// 未知 
    /// </summary>
    public const string UNKOWN = "UNKOWN";

    /// <summary> 
    /// 2空格缩进 
    /// </summary>
    public const string SpaceCap2 = "  ";
    /// <summary> 
    /// 4空格缩进 
    /// </summary>
    public const string SpaceCap4 = "    ";
    /// <summary>
    /// 8空格缩进 
    /// </summary>
    public const string SpaceCap8 = "        ";

    /// <summary> 
    /// 错误提示文本 
    /// </summary>
    public const string ErrorTip = "ERROR (；′⌒`)";
    /// <summary> 
    /// 红色的错误提示文本 
    /// </summary>
    public static string ErrorTipWithColor => ErrorTip.Colorize(ColorLibrary.RedReadable);
}