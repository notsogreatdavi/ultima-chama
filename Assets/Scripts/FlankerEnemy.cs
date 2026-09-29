using UnityEngine;

// Caçadora: prevê para onde Pavio vai e corta o caminho.
// De perto, circula e dá o bote (investida curta).
public class FlankerEnemy : Enemy
{
    const float Prediction = 0.9f;
    const float LungeRange = 3f;
    const float LungeTime = 0.35f;
    const float LungeCooldown = 2f;

    Rigidbody2D playerRb;
    float orbitSign;
    float lungeTimer;
    float lungeCd = 1f;
    Vector2 lungeDir;

    protected override string Variant => "cacadora";
    protected override bool Busy => lungeTimer > 0f;

    protected override void Start()
    {
        base.Start();
        playerRb = player.GetComponent<Rigidbody2D>();
        orbitSign = Random.value < 0.5f ? -1f : 1f;
        scoreValue = 10;
    }

    protected override void Update()
    {
        base.Update();
        lungeTimer -= Time.deltaTime;
        lungeCd -= Time.deltaTime;
        boost = lungeTimer > 0f ? 2.4f : 1f;

        if (player == null || !player.Alive || lungeCd > 0f) return;
        Vector2 toPlayer = player.Body - Body;
        if (toPlayer.magnitude < LungeRange)
        {
            lungeDir = toPlayer.normalized;
            lungeTimer = LungeTime;
            lungeCd = LungeCooldown;
            anim.Play("breus/cacadora_lunge", false, true);
        }
    }

    protected override Vector2 Desired()
    {
        Vector2 me = Body;
        if (lungeTimer > 0f) return me + lungeDir * 5f;

        Vector2 p = player.Body;
        Vector2 predicted = p + playerRb.GetVelocity() * Prediction;
        Vector2 toPlayer = p - me;

        // Em distância média, circula de lado antes de avançar.
        if (toPlayer.magnitude < 4.5f && toPlayer.magnitude > 1.5f && Mathf.Repeat(Time.time, 3f) < 1.5f)
        {
            Vector2 side = new Vector2(-toPlayer.y, toPlayer.x).normalized * orbitSign;
            return me + side * 2f + toPlayer.normalized * 0.5f;
        }
        return predicted;
    }
}
