using UnityEngine;

// Plano B de iluminação (sem luz 2D): escuridão em faixas ao redor de Pavio.
// O raio de luz encolhe conforme a vida cai.
public class Darkness : MonoBehaviour
{
    PlayerController player;

    public static void Create(PlayerController player)
    {
        var go = Bootstrap.NewObject("Darkness");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Darkness();
        sr.sortingOrder = Fx.Order - 10;
        go.AddComponent<Darkness>().player = player;
    }

    void LateUpdate()
    {
        if (player == null) return;
        transform.position = player.Body;

        float lifeRatio = player.Alive ? (float)player.Hp / PlayerController.MaxHp : 0.35f;
        float flicker = 1f + Mathf.Sin(Time.time * 11f) * 0.012f + Mathf.Sin(Time.time * 23f) * 0.008f;
        float target = Mathf.Lerp(0.55f, 1f, lifeRatio) * flicker;
        float s = Mathf.Lerp(transform.localScale.x, target, 1f - Mathf.Exp(-6f * Time.deltaTime));
        transform.localScale = new Vector3(s, s, 1f);
    }
}
