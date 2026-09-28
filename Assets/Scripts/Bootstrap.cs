using UnityEngine;

// Monta o jogo inteiro por código ao carregar a cena. A cena pode estar vazia.
public static class Bootstrap
{
    public static Transform Root { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnGameStart()
    {
        Build(true);
    }

    // Reinicia destruindo o mundo atual e criando outro (sem recarregar cena).
    public static void Rebuild(bool startInMenu)
    {
        if (Root != null) Object.Destroy(Root.gameObject);
        Build(startInMenu);
    }

    public static GameObject NewObject(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(Root, false);
        return go;
    }

    static void Build(bool startInMenu)
    {
        Time.timeScale = 1f;
        Physics2D.gravity = Vector2.zero;
        Physics2D.queriesStartInColliders = false;
        Enemy.All.Clear();

        Root = new GameObject("World").transform;

        Camera cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        var arena = ArenaBuilder.Build();
        var player = PlayerController.Create(arena.PlayerSpawn);

        cam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);
        var follow = cam.GetComponent<CameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
        follow.target = player;

        Darkness.Create(player);

        var gmGo = NewObject("GameManager");
        var spawner = gmGo.AddComponent<WaveSpawner>();
        spawner.arena = arena;
        spawner.player = player;
        var gm = gmGo.AddComponent<GameManager>();
        gm.Setup(player, spawner, startInMenu);
    }
}
