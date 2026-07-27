using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 文本样式结构。
/// </summary>
public struct TextStyle
{
    /// <summary> 极小字体样式。 </summary>
    public static readonly GUIStyle TinyFontStyle;
    /// <summary> 小字体样式。 </summary>
    public static readonly GUIStyle SmallFontStyle;
    /// <summary> 中等字体样式。 </summary>
    public static readonly GUIStyle MediumFontStyle;

    static TextStyle()
    {
        TinyFontStyle = new GUIStyle(GUI.skin.label)
        {
            font = Text.fontStyles[0].font
        };
        SmallFontStyle = new GUIStyle(GUI.skin.label)
        {
            font = Text.fontStyles[1].font,
            contentOffset = new Vector2(0f, -1f)
        };
        MediumFontStyle = new GUIStyle(GUI.skin.label)
        {
            font = Text.fontStyles[2].font
        };
    }

    /// <summary> 
    /// 是否自动换行。
    /// </summary>
    public bool wordWrap = true;

    /// <summary> 
    /// 文本对齐方式。 
    /// </summary>
    public TextAnchor anchor = TextAnchor.UpperLeft;

    /// <summary> 
    /// <see cref="GUI"/> 颜色。 
    /// </summary>
    public Color guiColor = Color.white;

    private GameFont font = GameFont.Small;
    /// <summary> 
    /// 游戏字体类型。 
    /// </summary>
    public readonly GameFont Font => font;

    private int fontSize;
    /// <summary> 
    /// 游戏字体字号。 
    /// </summary>
    public readonly int FontSize => fontSize;

    /// <summary> 获取应用了当前样式设置的 <see cref="GUIStyle"/>。 </summary>
    public readonly GUIStyle FontStyle
    {
        get
        {
            GUIStyle fontStyle = RawFontStyle;


            fontStyle.fontSize = fontSize;
            fontStyle.alignment = anchor;
            fontStyle.wordWrap = wordWrap;

            return fontStyle;
        }
    }

    private readonly GUIStyle RawFontStyle => font switch
    {
        GameFont.Tiny => TinyFontStyle,
        GameFont.Small => SmallFontStyle,
        _ => MediumFontStyle,
    };

    /// <summary> 
    /// 默认样式。 
    /// </summary>
    public static TextStyle DefaultStyle => new(font: GameFont.Small, anchor: TextAnchor.UpperLeft, wordWrap: true);

    /// <summary> 
    /// 当前文本样式。 
    /// </summary>
    public static TextStyle CurTextStyle => new(guiColor: GUI.color, font: Text.Font, anchor: Text.Anchor, wordWrap: Text.WordWrap);

    /// <summary>
    /// 创建文本样式。
    /// </summary>
    /// <param name="font">游戏字体</param>
    /// <param name="anchor">对齐方式</param>
    /// <param name="wordWrap">是否换行</param>
    public TextStyle(GameFont font = GameFont.Small, TextAnchor anchor = TextAnchor.UpperLeft, bool wordWrap = true)
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
    public TextStyle(Color guiColor, GameFont font = GameFont.Small, TextAnchor anchor = TextAnchor.UpperLeft, bool wordWrap = true) : this(font, anchor, wordWrap)
    {
        this.guiColor = guiColor;
    }

    /// <summary>
    /// 创建文本样式。
    /// </summary>
    /// <param name="fontSize">游戏字体字号</param>
    /// <param name="anchor">对齐方式</param>
    /// <param name="wordWrap">是否换行</param>
    public TextStyle(int fontSize, TextAnchor anchor = TextAnchor.UpperLeft, bool wordWrap = true)
    {
        this.anchor = anchor;
        this.wordWrap = wordWrap;
        SetFontSize(fontSize);
    }

    /// <summary>
    /// 创建文本样式。
    /// </summary>
    /// <param name="guiColor"><see cref="GUI"/> 颜色</param>
    /// <param name="fontSize">游戏字体字号</param>
    /// <param name="anchor">对齐方式</param>
    /// <param name="wordWrap">是否换行</param>
    public TextStyle(Color guiColor, int fontSize, TextAnchor anchor = TextAnchor.UpperLeft, bool wordWrap = true) : this(fontSize, anchor, wordWrap)
    {
        this.guiColor = guiColor;
    }


    /// <summary>
    /// 设置字体大小。
    /// </summary>
    /// <param name="fontSize">字体大小</param>
    /// <param name="overrideFont">是否自动覆盖字体类型</param>
    public void SetFontSize(int fontSize, bool overrideFont = true)
    {
        this.fontSize = Mathf.Max(0, fontSize);
        if (overrideFont)
        {
            font = fontSize switch
            {
                <= 14 => GameFont.Tiny,
                <= 20 => GameFont.Small,
                _ => GameFont.Medium,
            };
        }
    }

    /// <summary>
    /// 设置游戏字体。
    /// </summary>
    /// <param name="font">游戏字体</param>
    /// <param name="overrideFontSize">是否自动覆盖字体大小</param>
    public void SetGameFont(GameFont font, bool overrideFontSize = true)
    {
        this.font = font;
        if (overrideFontSize)
            fontSize = 0;
    }

    /// <summary> 恢复字体样式。 </summary>
    public readonly void RestoreTextStyle()
    {
        if (fontSize > 0)
        {
            RawFontStyle.fontSize = 0;
        }
    }
}