using UnityEngine;

// Efeitos de quadros que tocam uma vez e somem (impacto, fumaça, Labareda, surgimento).
public static class Fx
{
    public const int Order = 3000;

    public static void Play(string key, Vector3 pos, float scale = 1f, bool unlit = true, int order = Order)
    {
        var go = Bootstrap.NewObject("Fx " + key);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * scale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = order;
        if (unlit) SpriteFactory.UseUnlit(sr);

        go.AddComponent<SpriteAnimator>().Play(key, false, true);
        Object.Destroy(go, SpriteFactory.Duration(key) + 0.05f);
    }
}
