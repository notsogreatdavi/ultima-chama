using System.Collections;
using UnityEngine;

// Ondas de sombras com dificuldade crescente: mais inimigos, mais rápidos,
// intervalo menor e mais caçadoras a cada onda. Inimigos mortos voltam na próxima onda.
public class WaveSpawner : MonoBehaviour
{
    public ArenaBuilder arena;
    public PlayerController player;

    public int Wave { get; private set; }

    int pending;

    public void Begin()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        while (true)
        {
            Wave++;
            GameManager.Instance.Banner("ONDA " + Wave);
            yield return new WaitForSeconds(1.5f);

            int count = 4 + Wave * 2;
            float interval = Mathf.Max(0.2f, 1.1f - Wave * 0.09f);
            for (int i = 0; i < count; i++)
            {
                StartCoroutine(SpawnOne());
                yield return new WaitForSeconds(interval);
            }

            while (Enemy.All.Count > 0 || pending > 0) yield return null;
            if (!player.Alive) yield break;

            int bonus = 50 * Wave;
            GameManager.Instance.AddScore(bonus);
            GameManager.Instance.Banner("ONDA LIMPA  +" + bonus);
            yield return new WaitForSeconds(2.5f);
        }
    }

    // Marca o local antes de nascer, para o jogador ter chance de reagir.
    IEnumerator SpawnOne()
    {
        pending++;
        Vector3 pos = PickSpawnPoint();
        Fx.Spawn(SpriteFactory.Ring(), pos, new Color(0.8f, 0.2f, 0.9f, 0.9f), 1.2f, 0.3f, 0.7f, 0);
        yield return new WaitForSeconds(0.7f);
        pending--;
        if (!player.Alive) yield break;

        float speed = Mathf.Min(2.2f + Wave * 0.22f, 6f);
        float flankerChance = Mathf.Min(0.55f, (Wave - 1) * 0.1f);
        bool tank = Wave >= 4 && Random.value < 0.15f;

        if (Random.value < flankerChance)
            Enemy.Create<FlankerEnemy>(pos, player, speed * 1.1f, 1);
        else
            Enemy.Create<ChaserEnemy>(pos, player, tank ? speed * 0.7f : speed, tank ? 4 : 1);
    }

    Vector3 PickSpawnPoint()
    {
        Vector3 best = arena.FreeCells[0];
        for (int tries = 0; tries < 30; tries++)
        {
            best = arena.FreeCells[Random.Range(0, arena.FreeCells.Count)];
            if (Vector3.Distance(best, player.transform.position) > 7f) break;
        }
        return best;
    }
}
