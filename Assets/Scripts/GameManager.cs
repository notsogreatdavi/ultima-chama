using UnityEngine;

public enum GameState { Menu, Playing, Paused, GameOver }

// Estados do jogo, pontuação, combo, recorde e toda a interface (IMGUI).
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    const string BestKey = "UltimaChama.Best";

    public GameState State { get; private set; }
    public int Score { get; private set; }
    public int Combo { get; private set; }
    public int Multiplier => 1 + Combo / 5;

    PlayerController player;
    WaveSpawner spawner;
    int best;
    bool newRecord;

    string banner;
    float bannerTimer;
    float flash;
    Color flashColor;

    GUIStyle title, text, hud, bannerStyle, button;

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
        AddScore(value * Multiplier);
        if (Combo > 0 && Combo % 5 == 0) Banner("COMBO x" + Multiplier + "!");
    }

    public void OnGemCollected(Vector3 pos)
    {
        AddScore(10 * Multiplier);
    }

    public void OnPlayerHit()
    {
        if (Combo >= 5) Banner("COMBO PERDIDO");
        Combo = 0;
        Flash(new Color(1f, 0.1f, 0.1f));
    }

    public void OnNova(int killed)
    {
        Flash(new Color(1f, 0.7f, 0.2f));
        Banner(killed > 0 ? "NOVA!  " + killed + " sombras" : "NOVA...");
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

    void EnsureStyles()
    {
        if (title != null) return;
        title = new GUIStyle(GUI.skin.label) { fontSize = 72, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        title.normal.textColor = new Color(1f, 0.65f, 0.2f);
        text = new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        text.normal.textColor = Color.white;
        hud = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
        hud.normal.textColor = Color.white;
        bannerStyle = new GUIStyle(title) { fontSize = 44 };
        button = new GUIStyle(GUI.skin.button) { fontSize = 28, fontStyle = FontStyle.Bold };
    }

    void OnGUI()
    {
        EnsureStyles();
        float s = Screen.height / 720f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(s, s, 1f));
        float w = Screen.width / s;
        const float h = 720f;

        if (flash > 0f) Overlay(w, h, new Color(flashColor.r, flashColor.g, flashColor.b, flash * 0.35f));

        switch (State)
        {
            case GameState.Menu: DrawMenu(w, h); break;
            case GameState.Playing: DrawHud(w); break;
            case GameState.Paused: DrawHud(w); DrawPause(w, h); break;
            case GameState.GameOver: DrawGameOver(w, h); break;
        }
    }

    void DrawMenu(float w, float h)
    {
        Overlay(w, h, new Color(0f, 0f, 0f, 0.75f));
        GUI.Label(new Rect(0, 90, w, 100), "ÚLTIMA CHAMA", title);
        GUI.Label(new Rect(w / 2 - 400, 200, 800, 80),
            "Você é a última chama numa masmorra tomada por sombras.\nSua luz é sua vida: quanto mais ferido, mais escuro fica.", text);
        GUI.Label(new Rect(w / 2 - 420, 300, 840, 120),
            "WASD mover  |  Mouse ou Espaço atirar  |  Shift dash\n" +
            "Q  NOVA: destrói sombras ao redor, mas custa 1 de vida\n" +
            "Colete gemas antes que apaguem e mantenha o combo.  ESC pausa", text);
        GUI.Label(new Rect(0, 430, w, 40), "Recorde: " + best, text);
        if (GUI.Button(new Rect(w / 2 - 130, 490, 260, 70), "JOGAR", button)) StartGame();
        GUI.Label(new Rect(0, 570, w, 40), "(ou pressione Enter)", text);
    }

    void DrawHud(float w)
    {
        // Barra de vida (a chama).
        GUI.Label(new Rect(20, 12, 200, 34), "CHAMA", hud);
        for (int i = 0; i < PlayerController.MaxHp; i++)
        {
            bool full = i < player.Hp;
            var r = new Rect(120 + i * 34, 18, 28, 24);
            GUI.color = full ? new Color(1f, 0.6f, 0.15f) : new Color(0.3f, 0.3f, 0.3f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
        }
        GUI.color = Color.white;

        GUI.Label(new Rect(20, 50, 400, 34), "PONTOS  " + Score, hud);
        GUI.Label(new Rect(20, 84, 400, 34), "COMBO  x" + Multiplier + "  (" + Combo + ")", hud);
        GUI.Label(new Rect(w - 220, 12, 200, 34), "ONDA  " + spawner.Wave, hud);

        string nova;
        if (player.Hp <= 1) nova = "Q NOVA: chama fraca demais";
        else if (player.NovaCooldownLeft > 0f) nova = "Q NOVA: " + player.NovaCooldownLeft.ToString("0.0") + "s";
        else nova = "Q NOVA: PRONTA (-1 vida)";
        GUI.Label(new Rect(20, 680, 500, 34), nova, hud);

        if (bannerTimer > 0f && !string.IsNullOrEmpty(banner))
        {
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(bannerTimer));
            GUI.Label(new Rect(0, 130, w, 70), banner, bannerStyle);
            GUI.color = Color.white;
        }
    }

    void DrawPause(float w, float h)
    {
        Overlay(w, h, new Color(0f, 0f, 0f, 0.6f));
        GUI.Label(new Rect(0, 160, w, 100), "PAUSADO", title);
        if (GUI.Button(new Rect(w / 2 - 130, 300, 260, 64), "CONTINUAR", button)) SetPaused(false);
        if (GUI.Button(new Rect(w / 2 - 130, 380, 260, 64), "REINICIAR", button)) Bootstrap.Rebuild(false);
        if (GUI.Button(new Rect(w / 2 - 130, 460, 260, 64), "MENU", button)) Bootstrap.Rebuild(true);
    }

    void DrawGameOver(float w, float h)
    {
        Overlay(w, h, new Color(0f, 0f, 0f, 0.7f));
        GUI.Label(new Rect(0, 110, w, 100), "A CHAMA SE APAGOU", title);
        GUI.Label(new Rect(0, 230, w, 40), "Pontos: " + Score + "     Onda alcançada: " + spawner.Wave, text);
        GUI.Label(new Rect(0, 275, w, 40), newRecord ? "NOVO RECORDE!" : "Recorde: " + best, text);
        if (GUI.Button(new Rect(w / 2 - 130, 360, 260, 64), "JOGAR DE NOVO", button)) Bootstrap.Rebuild(false);
        if (GUI.Button(new Rect(w / 2 - 130, 440, 260, 64), "MENU", button)) Bootstrap.Rebuild(true);
        GUI.Label(new Rect(0, 520, w, 40), "(Enter para jogar de novo)", text);
    }

    static void Overlay(float w, float h, Color c)
    {
        GUI.color = c;
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
