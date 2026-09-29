using UnityEngine;

// Itens coletáveis. Lumens somem rápido: o jogador arrisca se aproximar dos Breus para pegá-los.
public class Pickup : MonoBehaviour
{
    public enum Kind { Lumen, Vela }

    const float LumenLifetime = 6f;
    const float VelaLifetime = 9f;

    Kind kind;
    float life, maxLife;
    SpriteRenderer sr;
    Vector3 basePos;

    public static void Spawn(Kind kind, Vector3 pos)
    {
        var go = Bootstrap.NewObject(kind.ToString());
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        SpriteFactory.UseUnlit(sr);
        go.AddComponent<YSort>();
        go.AddComponent<SpriteAnimator>().Play(kind == Kind.Lumen ? "itens/lumen" : "itens/vela");

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;

        if (kind == Kind.Lumen)
            LightRig.AddGlow(go.transform, new Color(0.25f, 0.9f, 1f), 1.4f, 0.6f);
        else
            LightRig.AddGlow(go.transform, new Color(1f, 0.7f, 0.35f), 2f, 0.8f);

        var p = go.AddComponent<Pickup>();
        p.kind = kind;
        p.sr = sr;
        p.basePos = pos;
        p.maxLife = kind == Kind.Lumen ? LumenLifetime : VelaLifetime;
    }

    void Update()
    {
        life += Time.deltaTime;
        transform.position = basePos + Vector3.up * (Mathf.Round(Mathf.Sin(life * 5f) * 1.5f) / 16f);

        // Pisca nos últimos 2 segundos avisando que vai sumir.
        float left = maxLife - life;
        sr.enabled = left > 2f || Mathf.Repeat(life * 8f, 1f) > 0.5f;
        if (left <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null || !player.Alive) return;

        if (kind == Kind.Lumen)
        {
            GameManager.Instance.OnGemCollected(transform.position);
            player.Happy();
            Fx.Play("itens/impacto", transform.position);
        }
        else
        {
            player.Heal();
            GameManager.Instance.Banner("+1 CHAMA");
        }
        Destroy(gameObject);
    }
}
