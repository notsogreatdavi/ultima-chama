using UnityEngine;

// Escuridão ao redor do jogador. O raio de luz encolhe conforme a vida cai:
// a chama é literalmente a vida do jogador.
public class Darkness : MonoBehaviour
{
    PlayerController player;

    public static void Create(PlayerController player)
    {
        var go = Bootstrap.NewObject("Darkness");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Darkness();
        sr.sortingOrder = 50;
        go.AddComponent<Darkness>().player = player;
    }

    void LateUpdate()
    {
        if (player == null) return;
        transform.position = player.transform.position;

        float lifeRatio = player.Alive ? (float)player.Hp / PlayerController.MaxHp : 0.35f;
        float flicker = 1f + Mathf.Sin(Time.time * 11f) * 0.012f + Mathf.Sin(Time.time * 23f) * 0.008f;
        float target = Mathf.Lerp(0.55f, 1f, lifeRatio) * flicker;
        float s = Mathf.Lerp(transform.localScale.x, target, 1f - Mathf.Exp(-6f * Time.deltaTime));
        transform.localScale = new Vector3(s, s, 1f);
    }
}
