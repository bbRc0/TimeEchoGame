using System.Collections.Generic;
using UnityEngine;

public class LevelRefs
{
    public Vector2 Spawn;
    public List<PressurePlate> Plates = new List<PressurePlate>();
    public List<Door> Doors = new List<Door>();
    public TimeWave Wave;
    public int Width, Height;
}

public static class LevelBuilder
{
    static readonly Color Ground = new Color(0.28f, 0.31f, 0.45f);
    static readonly Color Backdrop = new Color(0.10f, 0.11f, 0.17f);

    public static LevelRefs Build(string[] map, Transform root)
    {
        var refs = new LevelRefs();
        int rows = map.Length;
        int cols = map[0].Length;
        refs.Width = cols;
        refs.Height = rows;

        Vector2 Center(int c, int r) => new Vector2(c + 0.5f, (rows - 1 - r) + 0.5f);

        SpriteFactory.NewQuad("Backdrop", root, new Vector2(cols * 0.5f, rows * 0.5f),
            new Vector2(cols, rows), Backdrop, -10);

        // Duvarlar: satir basina bitisik '#' kosulari tek nesnede birlestir.
        for (int r = 0; r < rows; r++)
        {
            int c = 0;
            while (c < cols)
            {
                if (map[r][c] != '#') { c++; continue; }
                int start = c;
                while (c < cols && map[r][c] == '#') c++;
                int len = c - start;
                var pos = new Vector2(start + len * 0.5f, (rows - 1 - r) + 0.5f);
                var sr = SpriteFactory.NewQuad("Wall", root, pos, new Vector2(len, 1f), Ground, 0);
                sr.gameObject.AddComponent<BoxCollider2D>(); // 1x1, olcekle buyur
            }
        }

        // Kapilar: sutun basina bitisik 'D' kosulari tek kapi olur.
        for (int c = 0; c < cols; c++)
        {
            int r = 0;
            while (r < rows)
            {
                if (map[r][c] != 'D') { r++; continue; }
                int start = r;
                while (r < rows && map[r][c] == 'D') r++;
                int len = r - start;
                float midRow = start + (len - 1) * 0.5f;
                var pos = new Vector2(c + 0.5f, (rows - 1 - midRow) + 0.5f);
                refs.Doors.Add(Door.Spawn(pos, len, root));
            }
        }

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                switch (map[r][c])
                {
                    case 'P': refs.Spawn = Center(c, r); break;
                    case 'o': refs.Plates.Add(PressurePlate.Spawn(Center(c, r), root)); break;
                    case 'E': ExitZone.Spawn(Center(c, r), root); break;
                }
            }
        }

        foreach (var d in refs.Doors)
            d.Plates = refs.Plates;

        refs.Wave = TimeWave.Spawn(cols, rows, root);
        return refs;
    }
}
