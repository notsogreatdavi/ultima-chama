using UnityEngine;

// Efeitos visuais simples: explosões, anéis e marcadores. Crescem e somem.
public class Fx : MonoBehaviour
{
    SpriteRenderer sr;
    float life, maxLife, startScale, endScale;
    Color color;

    public static Fx Spawn(Sprite sprite, Vector3 pos, Color color, float startScale, float endScale, float duration, int order = 4)
    {
        var go = Bootstrap.NewObject("Fx");
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        var fx = go.AddComponent<Fx>();
        fx.sr = sr;
        fx.color = color;
        fx.startScale = startScale;
        fx.endScale = endScale;
        fx.maxLife = duration;
        go.transform.localScale = Vector3.one * startScale;
        return fx;
    }

    public static void Burst(Vector3 pos, Color color, float size = 1.6f)
    {
        Spawn(SpriteFactory.Soft(), pos, color, size * 0.4f, size, 0.35f);
    }

    public static void Ring(Vector3 pos, Color color, float radius)
    {
        Spawn(SpriteFactory.Ring(), pos, color, 0.5f, radius * 2f, 0.4f);
    }

    void Update()
    {
        life += Time.deltaTime;
        float t = Mathf.Clamp01(life / maxLife);
        transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);
        sr.color = new Color(color.r, color.g, color.b, color.a * (1f - t));
        if (t >= 1f) Destroy(gameObject);
    }
}
