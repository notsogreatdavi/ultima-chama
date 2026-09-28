using System;
using System.Collections.Generic;
using UnityEngine;

// Gera todos os sprites do jogo por código (sem assets externos).
public static class SpriteFactory
{
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
    static readonly Dictionary<string, Sprite[]> frameCache = new Dictionary<string, Sprite[]>();

    // f recebe coordenadas normalizadas (-1..1) e devolve a cor do pixel.
    public static Sprite Make(string key, int size, float ppu, Func<float, float, Color> f)
    {
        if (cache.TryGetValue(key, out Sprite cached)) return cached;

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x + 0.5f) / size * 2f - 1f;
                float ny = (y + 0.5f) / size * 2f - 1f;
                pixels[y * size + x] = f(nx, ny);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();

        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
        cache[key] = sprite;
        return sprite;
    }

    public static Sprite Circle(Color color)
    {
        return Make("circle", 32, 32, (x, y) => x * x + y * y <= 1f ? color : Color.clear);
    }

    public static Sprite Soft()
    {
        return Make("soft", 32, 32, (x, y) =>
        {
            float a = Mathf.Clamp01(1f - Mathf.Sqrt(x * x + y * y));
            return new Color(1f, 1f, 1f, a * a);
        });
    }

    public static Sprite Ring()
    {
        return Make("ring", 64, 64, (x, y) =>
        {
            float d = Mathf.Sqrt(x * x + y * y);
            return d <= 1f && d >= 0.86f ? Color.white : Color.clear;
        });
    }

    public static Sprite Tile(string key, Color baseColor, Color edge)
    {
        return Make(key, 16, 16, (x, y) =>
        {
            bool border = Mathf.Abs(x) > 0.85f || Mathf.Abs(y) > 0.85f;
            return border ? edge : baseColor;
        });
    }

    public static Sprite Gem()
    {
        return Make("gem", 16, 32, (x, y) =>
        {
            float d = Mathf.Abs(x) + Mathf.Abs(y);
            if (d > 1f) return Color.clear;
            return d < 0.45f ? new Color(0.7f, 1f, 1f) : new Color(0.2f, 0.85f, 1f);
        });
    }

    public static Sprite Heart()
    {
        return Make("heart", 24, 36, (x, y) =>
        {
            float hx = x * 1.2f, hy = y * 1.2f + 0.2f;
            float v = Mathf.Pow(hx * hx + hy * hy - 1f, 3f) - hx * hx * hy * hy * hy;
            return v <= 0f ? new Color(1f, 0.25f, 0.35f) : Color.clear;
        });
    }

    // Escuridão com um buraco de luz no centro. Cobre 60 unidades.
    public static Sprite Darkness()
    {
        const int size = 256;
        return Make("darkness", size, size / 60f, (x, y) =>
        {
            float d = Mathf.Sqrt(x * x + y * y);
            float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.1f, 0.37f, d));
            return new Color(0f, 0f, 0.02f, a * 0.96f);
        });
    }

    // Chama do jogador: 4 frames parado (tremulando), 4 frames andando (esticada).
    public static Sprite[] Flame(bool moving)
    {
        string key = moving ? "flame_move" : "flame_idle";
        if (frameCache.TryGetValue(key, out Sprite[] frames)) return frames;

        frames = new Sprite[4];
        for (int i = 0; i < 4; i++)
        {
            float wobble = Mathf.Sin(i * Mathf.PI / 2f) * 0.08f;
            float stretch = moving ? 1.25f + wobble : 1f + wobble * 0.5f;
            float squash = moving ? 0.8f : 1f - wobble * 0.5f;
            frames[i] = Make(key + i, 32, 32, (x, y) =>
            {
                float fx = x / squash;
                float fy = (y + 0.15f) / stretch;
                // formato de gota: mais fina em cima
                float width = 0.75f * Mathf.Clamp01(1f - Mathf.Max(0f, fy) * 0.9f);
                float d = fx * fx / (width * width + 0.0001f) + fy * fy / 0.64f;
                if (d > 1f) return Color.clear;
                if (d < 0.25f) return new Color(1f, 1f, 0.8f);
                if (d < 0.6f) return new Color(1f, 0.8f, 0.25f);
                return new Color(1f, 0.45f, 0.1f);
            });
        }
        frameCache[key] = frames;
        return frames;
    }

    // Sombra inimiga: corpo pulsando com olhos. 4 frames.
    public static Sprite[] Shade(string key, Color body, Color eyes)
    {
        if (frameCache.TryGetValue(key, out Sprite[] frames)) return frames;

        frames = new Sprite[4];
        for (int i = 0; i < 4; i++)
        {
            float pulse = 0.85f + Mathf.Sin(i * Mathf.PI / 2f) * 0.1f;
            int phase = i;
            frames[i] = Make(key + i, 32, 32, (x, y) =>
            {
                float px = x / pulse, py = y / pulse;
                bool eye = (Mathf.Abs(px - 0.32f) < 0.14f || Mathf.Abs(px + 0.32f) < 0.14f) && Mathf.Abs(py - 0.2f) < 0.14f;
                float bottom = -0.7f + Mathf.Sin(px * 9f + phase * 1.6f) * 0.12f;
                bool inside = px * px + py * py <= 0.8f && py > bottom;
                if (!inside) return Color.clear;
                return eye ? eyes : body;
            });
        }
        frameCache[key] = frames;
        return frames;
    }
}
