using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Base dos Breus. Subclasses definem para onde querem ir (Desired) e sua aparência;
// a base cuida de desviar de paredes, dano, dissolver ao morrer e drops.
public abstract class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> All = new List<Enemy>();

    public float speed = 2.5f;
    public int hp = 1;
    public int scoreValue = 5;

    protected PlayerController player;
    protected Rigidbody2D rb;
    protected SpriteAnimator anim;
    protected float boost = 1f;
    SpriteRenderer sr;
    Vector2 knockback;
    float hurtTimer;
    bool dead;

    static readonly RaycastHit2D[] hits = new RaycastHit2D[8];
    static readonly float[] steerAngles = { 0f, 35f, -35f, 70f, -70f, 110f, -110f };

    // Prefixo dos clipes em Resources/Sprites/breus (comum, cacadora, brutamontes).
    protected abstract string Variant { get; }
    protected virtual float BodyRadius => 0.5f;
    protected virtual Vector2 BodyOffset => new Vector2(0f, -0.15f);
    // Enquanto true, a base não volta para a animação de flutuar.
    protected virtual bool Busy => false;

    // Para onde o Breu quer ir (posição no mundo).
    protected abstract Vector2 Desired();

    public Vector3 Body => transform.position + (Vector3)BodyOffset;

    public static T Create<T>(Vector3 pos, PlayerController target, float speed, int hp) where T : Enemy
    {
        var go = Bootstrap.NewObject(typeof(T).Name);
        go.transform.position = pos;

        go.AddComponent<SpriteRenderer>();
        go.AddComponent<SpriteAnimator>();

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.mass = 0.5f;

        var col = go.AddComponent<CircleCollider2D>();
        var e = go.AddComponent<T>();
        col.radius = e.BodyRadius;
        col.offset = e.BodyOffset;
        go.AddComponent<YSort>().offset = e.BodyOffset.y;
        e.player = target;
        e.speed = speed;
        e.hp = hp;
        return e;
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<SpriteAnimator>();
    }

    protected virtual void Start()
    {
        anim.Play("breus/" + Variant + "_float");
    }

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    protected virtual void Update()
    {
        if (dead) return;
        hurtTimer -= Time.deltaTime;
        if (hurtTimer <= 0f && !Busy) anim.Play("breus/" + Variant + "_float");
        if (player != null) sr.flipX = player.transform.position.x < transform.position.x;
    }

    void FixedUpdate()
    {
        var gm = GameManager.Instance;
        if (dead || player == null || gm == null || gm.State != GameState.Playing || !player.Alive)
        {
            rb.SetVelocity(Vector2.zero);
            return;
        }

        Vector2 pos = rb.position + BodyOffset;
        Vector2 dir = Desired() - pos;
        if (dir.sqrMagnitude > 0.0001f) dir = Steer(pos, dir.normalized);

        rb.SetVelocity(dir * speed * boost + knockback);
        knockback = Vector2.MoveTowards(knockback, Vector2.zero, 30f * Time.fixedDeltaTime);
    }

    // Desvio de obstáculos: testa ângulos ao redor da direção desejada até achar caminho livre.
    Vector2 Steer(Vector2 pos, Vector2 dir)
    {
        var filter = ContactFilter2D.noFilter;
        foreach (float angle in steerAngles)
        {
            Vector2 d = Quaternion.Euler(0f, 0f, angle) * dir;
            if (!WallAhead(pos, d, filter)) return d;
        }
        return dir;
    }

    bool WallAhead(Vector2 pos, Vector2 dir, ContactFilter2D filter)
    {
        int count = Physics2D.CircleCast(pos, BodyRadius * 0.8f, dir, filter, hits, 1.2f);
        for (int i = 0; i < count; i++)
        {
            if (hits[i].collider is TilemapCollider2D) return true;
        }
        return false;
    }

    public void Knockback(Vector2 force)
    {
        knockback = force;
    }

    public void Hit(int damage)
    {
        if (dead) return;
        hp -= damage;
        hurtTimer = 0.17f;
        anim.Play("breus/" + Variant + "_hurt", false, true);
        if (player != null) Knockback(((Vector2)(Body - player.Body)).normalized * 5f);
        if (hp <= 0) Die();
    }

    public void Die()
    {
        if (dead) return;
        dead = true;
        All.Remove(this);

        GetComponent<Collider2D>().enabled = false;
        rb.SetVelocity(Vector2.zero);
        rb.simulated = false;

        string key = "breus/" + Variant + "_dissolve";
        anim.Play(key, false, true);
        CameraFollow.Instance?.Shake(0.08f);
        GameManager.Instance.OnEnemyKilled(scoreValue);

        // Recompensa: Lumen sempre, Vela às vezes.
        Pickup.Spawn(Pickup.Kind.Lumen, Body);
        if (Random.value < 0.07f) Pickup.Spawn(Pickup.Kind.Vela, Body + new Vector3(0.6f, 0.3f, 0f));

        Destroy(gameObject, SpriteFactory.Duration(key) + 0.05f);
    }
}
