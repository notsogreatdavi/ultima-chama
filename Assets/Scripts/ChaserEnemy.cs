using UnityEngine;

// Sombra comum: persegue o jogador diretamente.
public class ChaserEnemy : Enemy
{
    protected override Color BodyColor => new Color(0.45f, 0.2f, 0.6f);
    protected override string SpriteKey => "chaser";

    protected override Vector2 Desired()
    {
        return player.transform.position;
    }
}
