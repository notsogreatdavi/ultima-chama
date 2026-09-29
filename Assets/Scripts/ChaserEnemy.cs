using UnityEngine;

// Breu comum: persegue Pavio diretamente.
public class ChaserEnemy : Enemy
{
    protected override string Variant => "comum";

    protected override Vector2 Desired()
    {
        return player.Body;
    }
}
