using UnityEngine;
using UnityEngine.Rendering.Universal;

// Iluminação 2D: a luz de Pavio encolhe com a vida (a chama é a vida)
// e fica azul com Calor máximo. Também cria brilhos pequenos em itens e velas.
public class LightRig : MonoBehaviour
{
    const float MinRadius = 3.5f;
    const float MaxRadius = 7f;

    static readonly Color Warm = new Color(1f, 0.72f, 0.42f);
    static readonly Color Blue = new Color(0.55f, 0.72f, 1f);

    PlayerController player;
    Light2D light2D;
    float radius = MaxRadius;

    public static void Create(PlayerController player)
    {
        var go = Bootstrap.NewObject("Luz de Pavio");
        var l = go.AddComponent<Light2D>();
        l.lightType = Light2D.LightType.Point;
        l.color = Warm;
        l.intensity = 1.15f;
        l.pointLightInnerRadius = 1f;
        l.pointLightOuterRadius = MaxRadius;
        l.falloffIntensity = 0.55f;
        var rig = go.AddComponent<LightRig>();
        rig.player = player;
        rig.light2D = l;
    }

    public static void AddGlow(Transform parent, Color color, float radius, float intensity)
    {
        if (!Bootstrap.UseLights) return;
        var go = new GameObject("Brilho");
        go.transform.SetParent(parent, false);
        var l = go.AddComponent<Light2D>();
        l.lightType = Light2D.LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.pointLightInnerRadius = 0f;
        l.pointLightOuterRadius = radius;
        l.falloffIntensity = 0.7f;
    }

    // Deixa a luz global da cena bem fraca e arroxeada: só vê quem está perto de uma luz.
    public static void SetupGlobal(Transform root)
    {
        Light2D global = null;
        foreach (var l in Object.FindObjectsByType<Light2D>())
        {
            if (l.lightType == Light2D.LightType.Global) global = l;
        }
        if (global == null)
        {
            var go = new GameObject("Luz global");
            go.transform.SetParent(root, false);
            global = go.AddComponent<Light2D>();
            global.lightType = Light2D.LightType.Global;
        }
        global.color = new Color(0.55f, 0.5f, 0.85f);
        global.intensity = 0.2f;
    }

    void LateUpdate()
    {
        if (player == null) return;
        transform.position = player.Body;

        var gm = GameManager.Instance;
        float target = player.Alive
            ? Mathf.Lerp(MinRadius, MaxRadius, (player.Hp - 1f) / (PlayerController.MaxHp - 1f))
            : 0.5f;
        radius = Mathf.Lerp(radius, target, 1f - Mathf.Exp(-5f * Time.deltaTime));
        float flicker = 1f + Mathf.Sin(Time.time * 11f) * 0.02f + Mathf.Sin(Time.time * 23f) * 0.015f;
        light2D.pointLightOuterRadius = radius * flicker;
        light2D.color = gm != null && gm.Multiplier >= 4 ? Blue : Warm;
    }
}
