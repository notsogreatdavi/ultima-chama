using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Cria a fase com Tilemap: piso (sem colisão) e paredes (TilemapCollider2D) com tiles em pixel art.
public class ArenaBuilder : MonoBehaviour
{
    public const int Width = 34;
    public const int Height = 22;

    public Vector3 PlayerSpawn { get; private set; }
    public List<Vector3> FreeCells { get; } = new List<Vector3>();

    // Pilares internos (canto inferior esquerdo de cada bloco 2x2).
    static readonly Vector2Int[] Pillars =
    {
        new Vector2Int(7, 5), new Vector2Int(25, 5), new Vector2Int(7, 15), new Vector2Int(25, 15),
        new Vector2Int(16, 4), new Vector2Int(16, 16), new Vector2Int(11, 10), new Vector2Int(21, 10),
    };

    public static ArenaBuilder Build()
    {
        var gridGo = Bootstrap.NewObject("Grid");
        gridGo.AddComponent<Grid>();
        var arena = gridGo.AddComponent<ArenaBuilder>();

        var floor = CreateLayer(gridGo.transform, "Piso", -100, false);
        var walls = CreateLayer(gridGo.transform, "Paredes", -90, true);

        var pisos = new Tile[4];
        for (int i = 0; i < 4; i++) pisos[i] = MakeTile(SpriteFactory.Tile("piso" + i), false);
        var face = MakeTile(SpriteFactory.Tile("parede_face"), true);
        var topo = MakeTile(SpriteFactory.Tile("parede_topo"), true);

        var solid = new HashSet<Vector2Int>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1) solid.Add(new Vector2Int(x, y));
            }
        }
        foreach (var p in Pillars)
        {
            for (int dx = 0; dx < 2; dx++)
                for (int dy = 0; dy < 2; dy++)
                    solid.Add(new Vector2Int(p.x + dx, p.y + dy));
        }

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                var key = new Vector2Int(x, y);
                if (solid.Contains(key))
                {
                    // Face de tijolos quando o vizinho de baixo é chão (falso 3D).
                    bool exposed = y > 0 && !solid.Contains(new Vector2Int(x, y - 1));
                    walls.SetTile(cell, exposed ? face : topo);
                }
                else
                {
                    int v = Mathf.Abs((x * 73856093) ^ (y * 19349663)) % 7;
                    floor.SetTile(cell, pisos[v < 4 ? 0 : v - 3]);
                    arena.FreeCells.Add(floor.GetCellCenterWorld(cell));
                }
            }
        }

        arena.PlayerSpawn = floor.GetCellCenterWorld(new Vector3Int(Width / 2, Height / 2 - 1, 0));
        arena.Decorate();
        return arena;
    }

    // Crânios e velas de chão espalhados, longe do centro.
    void Decorate()
    {
        var rng = new System.Random(7);
        int skulls = 0, candles = 0;
        foreach (var c in FreeCells)
        {
            if (Vector3.Distance(c, PlayerSpawn) < 5f) continue;
            double r = rng.NextDouble();
            if (r < 0.025 && skulls < 10)
            {
                Decor("cranio", c, false);
                skulls++;
            }
            else if (r > 0.985 && candles < 6)
            {
                var go = Decor("vela_chao", c, true);
                go.GetComponent<SpriteRenderer>().sortingOrder = Bootstrap.GlowOrder;
                LightRig.AddGlow(go.transform, new Color(1f, 0.65f, 0.3f), 2.2f, 0.7f);
                candles++;
            }
        }
    }

    GameObject Decor(string tile, Vector3 pos, bool unlit)
    {
        var go = Bootstrap.NewObject("Decor " + tile);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Tile(tile);
        sr.sortingOrder = -80;
        if (unlit) SpriteFactory.UseUnlit(sr);
        return go;
    }

    static Tilemap CreateLayer(Transform parent, string name, int order, bool solid)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var map = go.AddComponent<Tilemap>();
        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = order;
        if (solid)
        {
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            go.AddComponent<TilemapCollider2D>();
        }
        return map;
    }

    static Tile MakeTile(Sprite sprite, bool solid)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.colliderType = solid ? Tile.ColliderType.Grid : Tile.ColliderType.None;
        return tile;
    }
}
