using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 基于游戏字体的文本样式。
/// </summary>
public struct TextStyle_GameFont
{
    /// <summary> 
    /// 是否自动换行。
    /// </summary>
    public bool wordWrap = true;

    /// <summary> 
    /// 游戏字体类型。 
    /// </summary>
    public GameFont font = GameFont.Small;

    /// <summary> 
    /// 文本对齐方式。 
    /// </summary>
    public TextAnchor anchor = TextAnchor.UpperLeft;

    /// <summary> 
    /// <see cref="GUI"/> 颜色。 
    /// </summary>
    public Color guiColor = Color.white;

    /// <summary> 
    /// 默认样式。 
    /// </summary>
    public static TextStyle_GameFont DefaultStyle => new(font: GameFont.Small, anchor: TextAnchor.UpperLeft, wordWrap: true);

    /// <summary> 
    /// 当前文本样式。 
    /// </summary>
    public static TextStyle_GameFont CurTextStyle => new(guiColor: GUI.color, font: Text.Font, anchor: Text.Anchor, wordWrap: Text.WordWrap);

    /// <summary>
    /// 创建文本样式。
    /// </summary>
    /// <param name="font">游戏字体</param>
    /// <param name="anchor">对齐方式</param>
    /// <param name="wordWrap">是否换行</param>
    public TextStyle_GameFont(GameFont font = GameFont.Small, TextAnchor anchor = TextAnchor.UpperLeft, bool wordWrap = true)
    {
        this.font = font;
        this.anchor = anchor;
        this.wordWrap = wordWrap;
    }

    /// <summary>
    /// 创建文本样式。
    /// </summary>
    /// <param name="guiColor"><see cref="GUI"/> 颜色</param>
    /// <param name="font">游戏字体</param>
    /// <param name="anchor">对齐方式</param>
    /// <param name="wordWrap">是否换行</param>
    public TextStyle_GameFont(Color guiColor, GameFont font = GameFont.Small, TextAnchor anchor = TextAnchor.UpperLeft, bool wordWrap = true) : this(font, anchor, wordWrap)
    {
        this.guiColor = guiColor;
    }

    /// <summary> 
    /// 应用文本样式到游戏。 
    /// </summary>
    public readonly void SetGameTextStyle()
    {
        Text.WordWrap = wordWrap;
        Text.Anchor = anchor;
        Text.Font = font;
        GUI.color = guiColor;
    }
}