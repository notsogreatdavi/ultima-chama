using System.Collections.Generic;
using UnityEngine;

// Kit de UI em pixel art para IMGUI: escala inteira (U), painéis 9-slice e fontes pixel.
// Não usa GUI.matrix: as texturas são ampliadas por vizinho mais próximo para não borrar.
public static class UiKit
{
    public static readonly Color Cream = Hex(0xFFF8E0);
    public static readonly Color Muted = Hex(0xC9C5D8);
    public static readonly Color Gray = Hex(0x9A96AC);
    public static readonly Color Gold = Hex(0xFFE08A);
    public static readonly Color Orange = Hex(0xFFA630);
    public static readonly Color Ember = Hex(0xA8321E);
    public static readonly Color Ink = Hex(0x0E0B16);
    public static readonly Color Night = Hex(0x1B1528);
    public static readonly Color Cyan = Hex(0x7FF5E8);
    public static readonly Color Smoke = Hex(0x4A4458);

    // Tamanho de 1 pixel da UI em pixels de tela.
    public static int U { get; private set; } = 1;

    public static GUIStyle Panel, PanelHot, Button, Key;
    public static Texture2D FlameFull, FlameEmpty, HeatOn, HeatOff, HeatMax;

    static int builtHeight = -1;
    static Font silk, silkBold, pixelify;
    static readonly Dictionary<string, GUIStyle> textStyles = new Dictionary<string, GUIStyle>();
    static readonly Dictionary<GUIStyle, GUIStyle> shadowStyles = new Dictionary<GUIStyle, GUIStyle>();
    static readonly List<Texture2D> scaled = new List<Texture2D>();

    public static void Ensure()
    {
        if (Screen.height == builtHeight && Panel != null) return;
        builtHeight = Screen.height;
        U = Mathf.Max(1, Screen.height / 240);

        foreach (var t in scaled) Object.Destroy(t);
        scaled.Clear();
        textStyles.Clear();
        shadowStyles.Clear();

        if (silk == null)
        {
            silk = Resources.Load<Font>("Fonts/Silkscreen-Regular");
            silkBold = Resources.Load<Font>("Fonts/Silkscreen-Bold");
            pixelify = Resources.Load<Font>("Fonts/PixelifySans");
        }

        Panel = NineSlice("painel", 8, 7);
        PanelHot = NineSlice("painel_chama", 8, 7);

        Button = NineSlice("botao", 8, 4);
        Button.hover.background = Scale("botao_hover");
        Button.active.background = Button.hover.background;
        Button.font = silk;
        Button.fontSize = 10 * U;
        Button.alignment = TextAnchor.MiddleCenter;
        Button.normal.textColor = Cream;
        Button.hover.textColor = Gold;
        Button.active.textColor = Gold;

        Key = NineSlice("tecla", 5, 3);
        Key.font = silk;
        Key.fontSize = 7 * U;
        Key.alignment = TextAnchor.MiddleCenter;
        Key.normal.textColor = Night;

        FlameFull = SpriteFactory.UiTexture("chama_cheia");
        FlameEmpty = SpriteFactory.UiTexture("chama_vazia");
        HeatOn = SpriteFactory.UiTexture("calor_on");
        HeatOff = SpriteFactory.UiTexture("calor_off");
        HeatMax = SpriteFactory.UiTexture("calor_max");
    }

    // title: Silkscreen (títulos, números, HUD). Senão Pixelify Sans (textos).
    public static GUIStyle Text(int size, Color color, TextAnchor align = TextAnchor.UpperLeft, bool title = false, bool bold = false)
    {
        string key = size + "|" + color + "|" + align + "|" + title + "|" + bold;
        if (textStyles.TryGetValue(key, out GUIStyle s)) return s;
        s = new GUIStyle
        {
            font = title ? (bold ? silkBold : silk) : pixelify,
            fontSize = size * U,
            alignment = align,
            wordWrap = !title,
            clipping = TextClipping.Overflow,
        };
        s.normal.textColor = color;
        textStyles[key] = s;
        return s;
    }

    public static void Label(Rect r, string text, GUIStyle style)
    {
        GUI.Label(r, text, style);
    }

    // Texto com sombra dura deslocada (sem desfoque).
    public static void Shadowed(Rect r, string text, GUIStyle style, Color shadow, int offset = 1)
    {
        if (!shadowStyles.TryGetValue(style, out GUIStyle sh))
        {
            sh = new GUIStyle(style);
            shadowStyles[style] = sh;
        }
        sh.normal.textColor = shadow;
        float d = offset * U;
        GUI.Label(new Rect(r.x + d, r.y + d, r.width, r.height), text, sh);
        GUI.Label(r, text, style);
    }

    public static void Box(Rect r, GUIStyle style)
    {
        GUI.Box(r, GUIContent.none, style);
    }

    // Desenha um ícone no tamanho nativo vezes U.
    public static void Icon(float x, float y, Texture2D tex)
    {
        if (tex == null) return;
        GUI.DrawTexture(new Rect(x, y, tex.width * U, tex.height * U), tex);
    }

    public static void Fill(Rect r, Color c)
    {
        var old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
    }

    static GUIStyle NineSlice(string name, int slice, int pad)
    {
        var s = new GUIStyle();
        s.normal.background = Scale(name);
        s.border = new RectOffset(slice * U, slice * U, slice * U, slice * U);
        s.padding = new RectOffset(pad * U, pad * U, pad * U, pad * U);
        return s;
    }

    static Texture2D Scale(string name)
    {
        var src = SpriteFactory.UiTexture(name);
        if (src == null) return Texture2D.whiteTexture;

        int w = src.width, h = src.height;
        var pixels = src.GetPixels32();
        var dst = new Texture2D(w * U, h * U, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        var outPx = new Color32[w * U * h * U];
        for (int y = 0; y < h * U; y++)
        {
            for (int x = 0; x < w * U; x++)
            {
                outPx[y * w * U + x] = pixels[(y / U) * w + x / U];
            }
        }
        dst.SetPixels32(outPx);
        dst.Apply();
        scaled.Add(dst);
        return dst;
    }

    static Color Hex(int rgb)
    {
        return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f);
    }
}
