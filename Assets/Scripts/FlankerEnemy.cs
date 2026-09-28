using UnityEngine;

// Sombra caçadora: prevê para onde o jogador vai e corta o caminho.
// De perto, circula e dá investidas.
public class FlankerEnemy : Enemy
{
    const float Prediction = 0.9f;

    Rigidbody2D playerRb;
    float orbitSign;

    protected override Color BodyColor => new Color(0.75f, 0.15f, 0.25f);
    protected override string SpriteKey => "flanker";

    protected override void Start()
    {
        base.Start();
        playerRb = player.GetComponent<Rigidbody2D>();
        orbitSign = Random.value < 0.5f ? -1f : 1f;
        scoreValue = 10;
    }

    protected override Vector2 Desired()
    {
        Vector2 p = player.transform.position;
        Vector2 predicted = p + playerRb.GetVelocity() * Prediction;
        Vector2 toPlayer = p - rb.position;

        // Em distância média, circula de lado antes de avançar.
        if (toPlayer.magnitude < 4f && toPlayer.magnitude > 1.5f && Mathf.Repeat(Time.time, 3f) < 1.5f)
        {
            Vector2 side = new Vector2(-toPlayer.y, toPlayer.x).normalized * orbitSign;
            return rb.position + side * 2f + toPlayer.normalized * 0.5f;
        }
        return predicted;
    }
}
