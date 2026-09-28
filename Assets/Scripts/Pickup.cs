using UnityEngine;

// Itens coletáveis. Gemas somem rápido: o jogador arrisca se aproximar das sombras para pegá-las.
public class Pickup : MonoBehaviour
{
    public enum Kind { Gem, Heart }

    const float GemLifetime = 6f;
    const float HeartLifetime = 9f;

    Kind kind;
    float life, maxLife;
    SpriteRenderer sr;
    Vector3 basePos;

    public static void Spawn(Kind kind, Vector3 pos)
    {
        var go = Bootstrap.NewObject(kind.ToString());
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = kind == Kind.Gem ? SpriteFactory.Gem() : SpriteFactory.Heart();
        sr.sortingOrder = 0;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;

        var p = go.AddComponent<Pickup>();
        p.kind = kind;
        p.sr = sr;
        p.basePos = pos;
        p.maxLife = kind == Kind.Gem ? GemLifetime : HeartLifetime;
    }

    void Update()
    {
        life += Time.deltaTime;
        transform.position = basePos + Vector3.up * Mathf.Sin(life * 5f) * 0.08f;

        // Pisca nos últimos 2 segundos avisando que vai sumir.
        float left = maxLife - life;
        sr.enabled = left > 2f || Mathf.Repeat(life * 8f, 1f) > 0.5f;
        if (left <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null || !player.Alive) return;

        if (kind == Kind.Gem)
        {
            GameManager.Instance.OnGemCollected(transform.position);
            Fx.Burst(transform.position, new Color(0.3f, 0.9f, 1f, 0.7f), 1f);
        }
        else
        {
            player.Heal();
            GameManager.Instance.Banner("+1 VIDA");
        }
        Destroy(gameObject);
    }
}
