using UnityEngine;

// Brutamontes: lento e resistente, fecha passagens. Vale mais Calor.
public class BruteEnemy : Enemy
{
    protected override string Variant => "brutamontes";
    protected override float BodyRadius => 0.85f;
    protected override Vector2 BodyOffset => new Vector2(0f, -0.2f);

    protected override void Start()
    {
        base.Start();
        scoreValue = 20;
    }

    protected override Vector2 Desired()
    {
        return player.Body;
    }
}
