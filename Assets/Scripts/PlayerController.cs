using UnityEngine;

// A chama: move, atira, dá dash e solta a Nova (ação especial que custa vida).
public class PlayerController : MonoBehaviour
{
    public const int MaxHp = 5;

    public float speed = 6f;
    public float fireRate = 0.16f;
    public float dashSpeed = 18f;
    public float dashTime = 0.15f;
    public float dashCooldown = 0.8f;
    public float novaRadius = 5f;
    public float novaCooldown = 3f;
    public float invulnTime = 1.1f;

    public int Hp { get; private set; } = MaxHp;
    public bool Alive => Hp > 0;
    public float NovaCooldownLeft => novaTimer;
    public bool CanNova => Alive && Hp > 1 && novaTimer <= 0f;

    Rigidbody2D rb;
    SpriteRenderer sr;
    SpriteAnimator anim;
    Camera cam;

    Vector2 moveInput;
    Vector2 dashDir;
    float fireTimer, dashTimer, dashCdTimer, novaTimer, invulnTimer;

    public static PlayerController Create(Vector3 pos)
    {
        var go = Bootstrap.NewObject("Player");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        go.AddComponent<SpriteAnimator>().fps = 10f;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        go.AddComponent<CircleCollider2D>().radius = 0.35f;
        return go.AddComponent<PlayerController>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<SpriteAnimator>();
        cam = Camera.main;
        anim.Play(SpriteFactory.Flame(false));
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (!Alive || gm == null || gm.State != GameState.Playing)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = Inp.Move();
        anim.Play(SpriteFactory.Flame(moveInput.sqrMagnitude > 0.01f));

        fireTimer -= Time.deltaTime;
        dashTimer -= Time.deltaTime;
        dashCdTimer -= Time.deltaTime;
        novaTimer -= Time.deltaTime;
        invulnTimer -= Time.deltaTime;

        Vector2 aim = (Vector2)(Inp.MouseWorld(cam) - transform.position);
        if (aim.sqrMagnitude < 0.01f) aim = Vector2.up;
        aim.Normalize();

        if (Inp.Fire() && fireTimer <= 0f)
        {
            fireTimer = fireRate;
            Projectile.Spawn(transform.position + (Vector3)(aim * 0.5f), aim);
        }

        if (Inp.Dash() && dashCdTimer <= 0f)
        {
            dashDir = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : aim;
            dashTimer = dashTime;
            dashCdTimer = dashCooldown;
            Fx.Burst(transform.position, new Color(1f, 0.6f, 0.2f, 0.6f), 1.2f);
        }

        if (Inp.Nova()) TryNova();

        // Pisca durante a invulnerabilidade.
        sr.enabled = invulnTimer <= 0f || Mathf.Repeat(Time.time * 12f, 1f) > 0.4f;
    }

    void FixedUpdate()
    {
        if (!Alive)
        {
            rb.SetVelocity(Vector2.zero);
            return;
        }
        rb.SetVelocity(dashTimer > 0f ? dashDir * dashSpeed : moveInput * speed);
    }

    // Ação especial: elimina todas as sombras no raio, mas consome 1 de vida (e a luz diminui).
    void TryNova()
    {
        if (!CanNova) return;

        Hp--;
        novaTimer = novaCooldown;
        invulnTimer = 0.5f;

        int killed = 0;
        for (int i = Enemy.All.Count - 1; i >= 0; i--)
        {
            var e = Enemy.All[i];
            if (Vector2.Distance(e.transform.position, transform.position) <= novaRadius)
            {
                e.Die();
                killed++;
            }
        }

        Fx.Ring(transform.position, new Color(1f, 0.7f, 0.2f), novaRadius);
        Fx.Burst(transform.position, new Color(1f, 0.9f, 0.5f, 0.8f), novaRadius);
        CameraFollow.Instance?.Shake(0.45f);
        GameManager.Instance.OnNova(killed);
    }

    public void Heal()
    {
        Hp = Mathf.Min(MaxHp, Hp + 1);
        Fx.Burst(transform.position, new Color(1f, 0.3f, 0.4f, 0.7f), 2f);
    }

    void OnCollisionStay2D(Collision2D c)
    {
        if (c.collider.GetComponent<Enemy>() != null) TakeHit(c.transform.position);
    }

    void TakeHit(Vector3 from)
    {
        if (!Alive || invulnTimer > 0f || dashTimer > 0f) return;
        if (GameManager.Instance.State != GameState.Playing) return;

        Hp--;
        invulnTimer = invulnTime;
        CameraFollow.Instance?.Shake(0.35f);
        Fx.Burst(transform.position, new Color(1f, 0.2f, 0.2f, 0.8f), 2f);

        // Empurra as sombras próximas para dar espaço de fuga.
        foreach (var e in Enemy.All)
        {
            Vector2 away = e.transform.position - transform.position;
            if (away.magnitude < 2.5f) e.Knockback(away.normalized * 8f);
        }

        GameManager.Instance.OnPlayerHit();

        if (Hp <= 0)
        {
            sr.enabled = false;
            GetComponent<Collider2D>().enabled = false;
            Fx.Burst(transform.position, new Color(1f, 0.6f, 0.1f), 4f);
            GameManager.Instance.OnPlayerDied();
        }
    }
}
