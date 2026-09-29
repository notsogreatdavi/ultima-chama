using System;
using System.Collections.Generic;
using UnityEngine;

// Biblioteca de sprites: carrega os quadros gerados por tools/sprites (Resources/Sprites).
public static class SpriteFactory
{
    [Serializable]
    class AnimMeta { public string key; public int frames; public int fps; }

    [Serializable]
    class MetaFile { public AnimMeta[] anims; }

    static readonly Dictionary<string, Sprite[]> clips = new Dictionary<string, Sprite[]>();
    static readonly Dictionary<string, Sprite> singles = new Dictionary<string, Sprite>();
    static readonly Dictionary<string, Sprite> generated = new Dictionary<string, Sprite>();
    static Dictionary<string, int> fps;
    static Material unlit;
    static bool unlitLoaded;

    // Ex.: Clip("pavio/idle_normal") devolve os quadros em ordem.
    public static Sprite[] Clip(string key)
    {
        if (clips.TryGetValue(key, out Sprite[] cached)) return cached;
        var frames = Resources.LoadAll<Sprite>("Sprites/" + key);
        Array.Sort(frames, (a, b) => string.CompareOrdinal(a.name, b.name));
        if (frames.Length == 0) Debug.LogWarning("Clipe sem quadros: " + key);
        clips[key] = frames;
        return frames;
    }

    public static int Fps(string key)
    {
        if (fps == null)
        {
            fps = new Dictionary<string, int>();
            var text = Resources.Load<TextAsset>("Sprites/meta");
            if (text != null)
            {
                foreach (var a in JsonUtility.FromJson<MetaFile>(text.text).anims) fps[a.key] = a.fps;
            }
        }
        return fps.TryGetValue(key, out int v) ? v : 10;
    }

    public static float Duration(string key)
    {
        return Clip(key).Length / (float)Fps(key);
    }

    public static Sprite Tile(string name)
    {
        return Single("Sprites/tiles/" + name);
    }

    public static Texture2D UiTexture(string name)
    {
        var s = Single("Sprites/ui/" + name);
        return s != null ? s.texture : null;
    }

    static Sprite Single(string path)
    {
        if (singles.TryGetValue(path, out Sprite s)) return s;
        s = Resources.Load<Sprite>(path);
        if (s == null) Debug.LogWarning("Sprite não encontrado: " + path);
        singles[path] = s;
        return s;
    }

    // Material sem iluminação para o que brilha (chama, faíscas, efeitos).
    public static Material Unlit
    {
        get
        {
            if (!unlitLoaded)
            {
                unlit = Resources.Load<Material>("Materials/SpriteUnlit");
                unlitLoaded = true;
            }
            return unlit;
        }
    }

    public static void UseUnlit(SpriteRenderer sr)
    {
        if (Unlit != null) sr.sharedMaterial = Unlit;
    }

    // ---- Gerado por código (usado só no plano B de escuridão) ----

    public static Sprite Make(string key, int size, float ppu, Func<float, float, Color> f)
    {
        if (generated.TryGetValue(key, out Sprite cached)) return cached;

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
        generated[key] = sprite;
        return sprite;
    }

    // Escuridão em faixas com um buraco de luz no centro. Cobre 60 unidades.
    public static Sprite Darkness()
    {
        const int size = 256;
        return Make("darkness", size, size / 60f, (x, y) =>
        {
            float d = Mathf.Sqrt(x * x + y * y);
            float a = d < 0.1f ? 0f : d < 0.14f ? 0.38f : d < 0.18f ? 0.62f : 0.9f;
            return new Color(0.055f, 0.043f, 0.086f, a);
        });
    }
}
