using System.Collections;
using UnityEngine;

public enum GameState { Menu, Playing, Paused, GameOver }

// Estados do jogo, pontuação, Calor (combo), recorde e toda a interface (IMGUI em pixel art).
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    const string BestKey = "UltimaChama.Best";
    // Combo necessário para encher a barra de Calor (x4).
    const int HeatFull = 15;

    public GameState State { get; private set; }
    public int Score { get; private set; }
    public int Combo { get; private set; }
    public int Multiplier => 1 + Combo / 5;
    public int MaxMultiplier { get; private set; } = 1;

    PlayerController player;
    WaveSpawner spawner;
    int best;
    bool newRecord;

    string banner;
    float bannerTimer;
    float flash;
    Color flashColor;
    bool hitStopping;

    static readonly (string key, string label)[] Keys =
    {
        ("WASD", "andar"), ("MOUSE", "Faísca"), ("SHIFT", "Sopro"), ("Q", "Labareda"), ("ESC", "pausa"),
    };

    public void Setup(PlayerController p, WaveSpawner s, bool startInMenu)
    {
        Instance = this;
        player = p;
        spawner = s;
        best = PlayerPrefs.GetInt(BestKey, 0);

        if (startInMenu)
        {
            State = GameState.Menu;
            Time.timeScale = 0f;
        }
        else
        {
            StartGame();
        }
    }

    void StartGame()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;
        spawner.Begin();
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        bannerTimer -= dt;
        flash = Mathf.Max(0f, flash - dt * 2f);

        switch (State)
        {
            case GameState.Menu:
                if (Inp.Confirm()) StartGame();
                break;
            case GameState.Playing:
                if (Inp.Pause()) SetPaused(true);
                break;
            case GameState.Paused:
                if (Inp.Pause()) SetPaused(false);
                break;
            case GameState.GameOver:
                if (Inp.Confirm()) Bootstrap.Rebuild(false);
                break;
        }
    }

    void SetPaused(bool paused)
    {
        State = paused ? GameState.Paused : GameState.Playing;
        Time.timeScale = paused ? 0f : 1f;
    }

    // ---------- Sistema de recompensas ----------

    public void AddScore(int amount)
    {
        Score += amount;
    }

    public void OnEnemyKilled(int value)
    {
        Combo++;
        MaxMultiplier = Mathf.Max(MaxMultiplier, Multiplier);
        AddScore(value * Multiplier);
        if (Combo % 5 == 0) Banner("CALOR x" + Multiplier + "!");
        if (!hitStopping && State == GameState.Playing) StartCoroutine(HitStop());
    }

    // Congela o tempo por um instante no acerto final: dá peso ao golpe.
    IEnumerator HitStop()
    {
        hitStopping = true;
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(0.04f);
        // pausa e menu controlam o próprio timeScale
        if (State == GameState.Playing || State == GameState.GameOver) Time.timeScale = 1f;
        hitStopping = false;
    }

    public void OnGemCollected(Vector3 pos)
    {
        AddScore(10 * Multiplier);
    }

    public void OnPlayerHit()
    {
        if (Combo >= 5) Banner("CALOR PERDIDO");
        Combo = 0;
        Flash(new Color(1f, 0.1f, 0.1f));
    }

    public void OnNova(int killed)
    {
        Flash(new Color(1f, 0.7f, 0.2f));
        Banner(killed > 0 ? "LABAREDA! " + killed + " BREUS" : "LABAREDA");
    }

    public void OnPlayerDied()
    {
        State = GameState.GameOver;
        newRecord = Score > best;
        if (newRecord)
        {
            best = Score;
            PlayerPrefs.SetInt(BestKey, best);
            PlayerPrefs.Save();
        }
    }

    public void Banner(string msg)
    {
        banner = msg;
        bannerTimer = 1.8f;
    }

    void Flash(Color c)
    {
        flashColor = c;
        flash = 1f;
    }

    // ---------- Interface ----------

    void OnGUI()
    {
        UiKit.Ensure();
        float w = Screen.width;
        float h = Screen.height;

        if (flash > 0f) UiKit.Fill(new Rect(0, 0, w, h), new Color(flashColor.r, flashColor.g, flashColor.b, flash * 0.3f));

        switch (State)
        {
            case GameState.Menu: DrawMenu(w, h); break;
            case GameState.Playing: DrawHud(w, h); break;
            case GameState.Paused: DrawHud(w, h); DrawPause(w, h); break;
            case GameState.GameOver: DrawGameOver(w, h); break;
        }
    }

    void DrawHud(float w, float h)
    {
        int u = UiKit.U;

        // Chamas (vida).
        var life = new Rect(4 * u, 4 * u, 82 * u, 26 * u);
        UiKit.Box(life, UiKit.Panel);
        for (int i = 0; i < PlayerController.MaxHp; i++)
            UiKit.Icon(life.x + (7 + i * 14) * u, life.y + 7 * u, i < player.Hp ? UiKit.FlameFull : UiKit.FlameEmpty);

        // Calor: barra de 10 segmentos que enche até x4.
        var heat = new Rect(4 * u, 32 * u, 150 * u, 24 * u);
        UiKit.Box(heat, UiKit.Panel);
        UiKit.Label(new Rect(heat.x + 7 * u, heat.y + 6 * u, 36 * u, 12 * u), "CALOR", UiKit.Text(8, UiKit.Orange, TextAnchor.MiddleLeft, true));
        int filled = Mathf.Min(10, Combo * 10 / HeatFull);
        bool max = Multiplier >= 4;
        for (int i = 0; i < 10; i++)
            UiKit.Icon(heat.x + (44 + i * 7) * u, heat.y + 7 * u, i < filled ? (max ? UiKit.HeatMax : UiKit.HeatOn) : UiKit.HeatOff);
        UiKit.Shadowed(new Rect(heat.x + 116 * u, heat.y + 4 * u, 30 * u, 16 * u), "x" + Multiplier,
            UiKit.Text(12, max ? UiKit.Cyan : UiKit.Gold, TextAnchor.MiddleLeft, true, true), UiKit.Ember);

        // Pontos e onda.
        var score = new Rect(w - 100 * u, 4 * u, 96 * u, 34 * u);
        UiKit.Box(score, UiKit.Panel);
        UiKit.Label(new Rect(score.x + 7 * u, score.y + 6 * u, score.width - 14 * u, 8 * u), "PONTOS", UiKit.Text(6, UiKit.Gray, TextAnchor.UpperRight, true));
        UiKit.Label(new Rect(score.x + 7 * u, score.y + 13 * u, score.width - 14 * u, 16 * u), Score.ToString(), UiKit.Text(14, UiKit.Cream, TextAnchor.UpperRight, true, true));
        var wave = new Rect(w - 64 * u, 40 * u, 60 * u, 22 * u);
        UiKit.Box(wave, UiKit.Panel);
        UiKit.Label(wave, "ONDA " + spawner.Wave, UiKit.Text(8, UiKit.Cyan, TextAnchor.MiddleCenter, true));

        // Faixa central (Calor, onda, Labareda...).
        if (bannerTimer > 0f && !string.IsNullOrEmpty(banner))
        {
            var style = UiKit.Text(12, UiKit.Gold, TextAnchor.MiddleCenter, true, true);
            Vector2 size = style.CalcSize(new GUIContent(banner));
            var r = new Rect((w - size.x) / 2f - 12 * u, 48 * u, size.x + 24 * u, size.y + 16 * u);
            var old = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(bannerTimer * 2f));
            UiKit.Box(r, UiKit.PanelHot);
            UiKit.Shadowed(r, banner, style, UiKit.Ember);
            GUI.color = old;
        }

        // Labareda (Q).
        var nova = new Rect(4 * u, h - 36 * u, 128 * u, 32 * u);
        UiKit.Box(nova, UiKit.Panel);
        GUI.Box(new Rect(nova.x + 7 * u, nova.y + 6 * u, 20 * u, 20 * u), "Q", UiKit.Key);
        UiKit.Label(new Rect(nova.x + 31 * u, nova.y + 5 * u, 90 * u, 10 * u), "LABAREDA", UiKit.Text(8, UiKit.Orange, TextAnchor.UpperLeft, true));
        string status;
        if (player.Hp <= 1) status = "chama fraca demais";
        else if (player.NovaCooldownLeft > 0f) status = "recarga " + player.NovaCooldownLeft.ToString("0.0") + "s";
        else status = "pronta · custa 1 chama";
        UiKit.Label(new Rect(nova.x + 31 * u, nova.y + 15 * u, 95 * u, 10 * u), status, UiKit.Text(7, UiKit.Muted));

        // Sopro (dash).
        var dash = new Rect(w - 90 * u, h - 28 * u, 86 * u, 24 * u);
        UiKit.Box(dash, UiKit.Panel);
        UiKit.Label(new Rect(dash.x + 7 * u, dash.y + 6 * u, 32 * u, 12 * u), "SOPRO", UiKit.Text(7, UiKit.Muted, TextAnchor.MiddleLeft, true));
        var bar = new Rect(dash.x + 42 * u, dash.y + 9 * u, 36 * u, 6 * u);
        UiKit.Fill(bar, UiKit.Ink);
        UiKit.Fill(new Rect(bar.x + u, bar.y + u, (bar.width - 2 * u) * player.SoproPronto01, bar.height - 2 * u), UiKit.Orange);
    }

    void DrawMenu(float w, float h)
    {
        int u = UiKit.U;
        UiKit.Fill(new Rect(0, 0, 150 * u, h), new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.6f));

        UiKit.Shadowed(new Rect(16 * u, 20 * u, 200 * u, 14 * u), "ÚLTIMA", UiKit.Text(12, UiKit.Cream, TextAnchor.UpperLeft, true), UiKit.Ink);
        UiKit.Shadowed(new Rect(16 * u, 32 * u, 200 * u, 30 * u), "CHAMA", UiKit.Text(28, UiKit.Orange, TextAnchor.UpperLeft, true, true), UiKit.Ember, 2);
        UiKit.Label(new Rect(16 * u, 68 * u, 128 * u, 30 * u), "Mantenha Pavio aceso. Quanto mais você arrisca, mais forte a chama brilha.", UiKit.Text(8, UiKit.Muted));

        if (GUI.Button(new Rect(16 * u, 102 * u, 96 * u, 24 * u), "JOGAR", UiKit.Button)) StartGame();

        var rec = new Rect(16 * u, 130 * u, 96 * u, 22 * u);
        UiKit.Box(rec, UiKit.Panel);
        UiKit.Label(new Rect(rec.x + 7 * u, rec.y, 40 * u, rec.height), "RECORDE", UiKit.Text(6, UiKit.Gray, TextAnchor.MiddleLeft, true));
        UiKit.Label(new Rect(rec.x + 7 * u, rec.y, rec.width - 14 * u, rec.height), best.ToString(), UiKit.Text(10, UiKit.Gold, TextAnchor.MiddleRight, true));

        // Teclas.
        float x = 16 * u;
        float y = h - 22 * u;
        var labelStyle = UiKit.Text(7, UiKit.Muted, TextAnchor.MiddleLeft);
        foreach (var k in Keys)
        {
            float kw = Mathf.Max(18 * u, UiKit.Key.CalcSize(new GUIContent(k.key)).x + 6 * u);
            GUI.Box(new Rect(x, y, kw, 16 * u), k.key, UiKit.Key);
            float lw = labelStyle.CalcSize(new GUIContent(k.label)).x;
            UiKit.Label(new Rect(x + kw + 3 * u, y, lw, 16 * u), k.label, labelStyle);
            x += kw + lw + 12 * u;
        }
    }

    void DrawPause(float w, float h)
    {
        int u = UiKit.U;
        UiKit.Fill(new Rect(0, 0, w, h), new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.75f));

        var p = new Rect((w - 150 * u) / 2f, (h - 124 * u) / 2f, 150 * u, 124 * u);
        UiKit.Box(p, UiKit.Panel);
        UiKit.Shadowed(new Rect(p.x, p.y + 10 * u, p.width, 18 * u), "PAUSADO", UiKit.Text(16, UiKit.Gold, TextAnchor.MiddleCenter, true, true), UiKit.Ink);
        UiKit.Label(new Rect(p.x + 12 * u, p.y + 30 * u, p.width - 24 * u, 20 * u), "Pavio tirou um cochilo. Os Breus também esperam.", UiKit.Text(7, UiKit.Muted, TextAnchor.UpperCenter));

        float bx = p.x + 15 * u;
        if (GUI.Button(new Rect(bx, p.y + 52 * u, 120 * u, 20 * u), "CONTINUAR", UiKit.Button)) SetPaused(false);
        if (GUI.Button(new Rect(bx, p.y + 75 * u, 120 * u, 20 * u), "REINICIAR", UiKit.Button)) Bootstrap.Rebuild(false);
        if (GUI.Button(new Rect(bx, p.y + 98 * u, 120 * u, 20 * u), "MENU", UiKit.Button)) Bootstrap.Rebuild(true);
    }

    void DrawGameOver(float w, float h)
    {
        int u = UiKit.U;
        UiKit.Fill(new Rect(0, 0, w, h), new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.55f));

        float top = h * 0.16f;
        UiKit.Shadowed(new Rect(0, top, w, 20 * u), "A CHAMA SE APAGOU", UiKit.Text(16, UiKit.Cream, TextAnchor.MiddleCenter, true, true), UiKit.Smoke, 2);

        string[] labels = { "PONTOS", "ONDA", "MAIOR CALOR" };
        string[] values = { Score.ToString(), spawner.Wave.ToString(), "x" + MaxMultiplier };
        float sw = 74 * u;
        float sx = (w - (sw * 3 + 8 * u)) / 2f;
        for (int i = 0; i < 3; i++)
        {
            var r = new Rect(sx + i * (sw + 4 * u), top + 26 * u, sw, 34 * u);
            UiKit.Box(r, UiKit.Panel);
            UiKit.Label(new Rect(r.x, r.y + 6 * u, r.width, 8 * u), labels[i], UiKit.Text(6, UiKit.Gray, TextAnchor.UpperCenter, true));
            UiKit.Label(new Rect(r.x, r.y + 13 * u, r.width, 16 * u), values[i], UiKit.Text(12, UiKit.Cream, TextAnchor.UpperCenter, true, true));
        }

        float ry = top + 64 * u;
        if (newRecord)
        {
            var style = UiKit.Text(10, UiKit.Gold, TextAnchor.MiddleCenter, true, true);
            float rw = style.CalcSize(new GUIContent("NOVO RECORDE!")).x + 24 * u;
            var r = new Rect((w - rw) / 2f, ry, rw, 24 * u);
            UiKit.Box(r, UiKit.PanelHot);
            UiKit.Label(r, "NOVO RECORDE!", style);
        }
        else
        {
            UiKit.Label(new Rect(0, ry, w, 24 * u), "RECORDE " + best, UiKit.Text(8, UiKit.Gray, TextAnchor.MiddleCenter, true));
        }

        float by = ry + 30 * u;
        float total = 120 * u + 4 * u + 60 * u;
        float bx = (w - total) / 2f;
        if (GUI.Button(new Rect(bx, by, 120 * u, 24 * u), "ACENDER DE NOVO", UiKit.Button)) Bootstrap.Rebuild(false);
        if (GUI.Button(new Rect(bx + 124 * u, by, 60 * u, 24 * u), "MENU", UiKit.Button)) Bootstrap.Rebuild(true);
        UiKit.Label(new Rect(0, by + 28 * u, w, 10 * u), "(Enter para acender de novo)", UiKit.Text(7, UiKit.Gray, TextAnchor.UpperCenter));
    }
}
