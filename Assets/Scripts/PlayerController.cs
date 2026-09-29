using UnityEngine;

// Pavio, a chaminha: move, solta Faísca, dá Sopro (dash) e Labareda (custa vida).
// Escolhe a animação e a expressão do rosto conforme o que acontece.
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

    // Centro do corpo da chama em relação ao pivô do sprite.
    public static readonly Vector3 BodyOffset = new Vector3(0f, -0.3f, 0f);

    public int Hp { get; private set; } = MaxHp;
    public bool Alive => Hp > 0;
    public float NovaCooldownLeft => novaTimer;
    public bool CanNova => Alive && Hp > 1 && novaTimer <= 0f;
    public float SoproPronto01 => 1f - Mathf.Clamp01(dashCdTimer / dashCooldown);
    public Vector3 Body => transform.position + BodyOffset;

    Rigidbody2D rb;
    SpriteRenderer sr;
    SpriteAnimator anim;
    Camera cam;

    Vector2 moveInput;
    Vector2 dashDir;
    Vector2 facing = Vector2.right;
    float fireTimer, dashTimer, dashCdTimer, novaTimer, invulnTimer;
    float hurtFace, angryFace, happyFace, blinkTimer, shootAnim, squash;
    float nextBlink = 3f;

    public static PlayerController Create(Vector3 pos)
    {
        var go = Bootstrap.NewObject("Pavio");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        SpriteFactory.UseUnlit(sr);
        go.AddComponent<YSort>().offset = BodyOffset.y;
        go.AddComponent<SpriteAnimator>();

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        col.offset = BodyOffset;
        return go.AddComponent<PlayerController>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<SpriteAnimator>();
        cam = Camera.main;
        anim.Play("pavio/idle_normal");
    }

    void Update()
    {
        if (!Alive) return;

        var gm = GameManager.Instance;
        bool playing = gm != null && gm.State == GameState.Playing;
        float dt = Time.deltaTime;

        fireTimer -= dt;
        dashTimer -= dt;
        dashCdTimer -= dt;
        novaTimer -= dt;
        invulnTimer -= dt;
        hurtFace -= dt;
        angryFace -= dt;
        happyFace -= dt;
        shootAnim -= dt;
        blinkTimer -= dt;
        nextBlink -= dt;
        if (nextBlink <= 0f)
        {
            blinkTimer = 0.15f;
            nextBlink = Random.Range(3f, 5f);
        }

        moveInput = playing ? Inp.Move() : Vector2.zero;

        Vector2 aim = (Vector2)(Inp.MouseWorld(cam) - Body);
        if (aim.sqrMagnitude < 0.01f) aim = facing;
        aim.Normalize();

        if (playing)
        {
            if (Inp.Fire() && fireTimer <= 0f)
            {
                fireTimer = fireRate;
                shootAnim = 0.12f;
                angryFace = 0.25f;
                squash = 0.18f;
                Projectile.Spawn(Body + (Vector3)(aim * 0.5f), aim);
            }

            if (Inp.Dash() && dashCdTimer <= 0f)
            {
                dashDir = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : aim;
                dashTimer = dashTime;
                dashCdTimer = dashCooldown;
                angryFace = 0.25f;
                Fx.Play("itens/fumaca", transform.position + new Vector3(0f, -0.6f, 0f), 1f, false, 500);
            }

            if (Inp.Nova()) TryNova();
        }

        if (moveInput.sqrMagnitude > 0.01f) facing = moveInput;
        else if (dashTimer <= 0f) facing = aim;
        if (dashTimer > 0f) facing = dashDir;

        UpdateVisual();
    }

    string Expression()
    {
        if (hurtFace > 0f) return "dano";
        if (angryFace > 0f) return "bravo";
        if (happyFace > 0f) return "feliz";
        if (Hp == 1) return "medo";
        if (blinkTimer > 0f) return "piscando";
        return "normal";
    }

    void UpdateVisual()
    {
        var gm = GameManager.Instance;
        string prefix = gm != null && gm.Multiplier >= 4 ? "azul_" : "";
        string key;
        if (hurtFace > 0.1f) key = "hurt";
        else if (dashTimer > 0f) key = "dash";
        else if (shootAnim > 0f) key = "shoot";
        else if (moveInput.sqrMagnitude > 0.01f) key = "run_" + Expression();
        else key = "idle_" + Expression();
        anim.Play("pavio/" + prefix + key, true, false, true);

        // Sprites desenhados virados para a direita.
        if (Mathf.Abs(facing.x) > 0.05f) sr.flipX = facing.x < 0f;

        // Achatar e voltar esticando (squash and stretch).
        squash = Mathf.MoveTowards(squash, 0f, Time.deltaTime * 1.2f);
        transform.localScale = new Vector3(1f + squash, 1f - squash, 1f);

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

    // Labareda: elimina os Breus no raio, mas consome 1 chama (e a luz diminui).
    void TryNova()
    {
        if (!CanNova) return;

        Hp--;
        novaTimer = novaCooldown;
        invulnTimer = 0.5f;
        angryFace = 0.5f;
        squash = -0.25f;

        int killed = 0;
        for (int i = Enemy.All.Count - 1; i >= 0; i--)
        {
            var e = Enemy.All[i];
            if (Vector2.Distance(e.transform.position, Body) <= novaRadius)
            {
                e.Die();
                killed++;
            }
        }

        // O anel do sprite chega a ~2,9 unidades de raio; escala até o raio real.
        Fx.Play("itens/labareda", Body, novaRadius / 2.9f);
        CameraFollow.Instance?.Shake(0.45f);
        GameManager.Instance.OnNova(killed);
    }

    public void Heal()
    {
        Hp = Mathf.Min(MaxHp, Hp + 1);
        happyFace = 0.8f;
        Fx.Play("itens/impacto", Body, 1.5f);
    }

    public void Happy()
    {
        happyFace = 0.6f;
    }

    void OnCollisionStay2D(Collision2D c)
    {
        if (c.collider.GetComponent<Enemy>() != null) TakeHit();
    }

    void TakeHit()
    {
        if (!Alive || invulnTimer > 0f || dashTimer > 0f) return;
        if (GameManager.Instance.State != GameState.Playing) return;

        Hp--;
        invulnTimer = invulnTime;
        hurtFace = 0.3f;
        CameraFollow.Instance?.Shake(0.35f);
        Fx.Play("itens/impacto", Body, 2f);

        // Empurra os Breus próximos para dar espaço de fuga.
        foreach (var e in Enemy.All)
        {
            Vector2 away = e.transform.position - Body;
            if (away.magnitude < 2.5f) e.Knockback(away.normalized * 8f);
        }

        GameManager.Instance.OnPlayerHit();

        if (Hp <= 0)
        {
            sr.enabled = true;
            transform.localScale = Vector3.one;
            GetComponent<Collider2D>().enabled = false;
            anim.Play("pavio/death", false, true);
            Fx.Play("itens/fumaca", Body + Vector3.up * 0.5f, 1.5f, false);
            GameManager.Instance.OnPlayerDied();
        }
    }
}
