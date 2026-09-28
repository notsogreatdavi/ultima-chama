using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Cria a fase com Tilemap: chão (sem colisão) e paredes (TilemapCollider2D).
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

        var floor = CreateLayer(gridGo.transform, "Floor", -10, false);
        var walls = CreateLayer(gridGo.transform, "Walls", -5, true);

        var floorA = MakeTile(SpriteFactory.Tile("floorA", new Color(0.16f, 0.14f, 0.2f), new Color(0.13f, 0.11f, 0.16f)), false);
        var floorB = MakeTile(SpriteFactory.Tile("floorB", new Color(0.19f, 0.16f, 0.23f), new Color(0.13f, 0.11f, 0.16f)), false);
        var wall = MakeTile(SpriteFactory.Tile("wall", new Color(0.35f, 0.3f, 0.4f), new Color(0.2f, 0.17f, 0.24f)), true);

        var blocked = new HashSet<Vector2Int>();
        foreach (var p in Pillars)
        {
            for (int dx = 0; dx < 2; dx++)
                for (int dy = 0; dy < 2; dy++)
                    blocked.Add(new Vector2Int(p.x + dx, p.y + dy));
        }

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                floor.SetTile(cell, (x + y) % 2 == 0 ? floorA : floorB);

                bool border = x == 0 || y == 0 || x == Width - 1 || y == Height - 1;
                if (border || blocked.Contains(new Vector2Int(x, y)))
                    walls.SetTile(cell, wall);
                else
                    arena.FreeCells.Add(floor.GetCellCenterWorld(cell));
            }
        }

        arena.PlayerSpawn = floor.GetCellCenterWorld(new Vector3Int(Width / 2, Height / 2 - 1, 0));
        return arena;
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
