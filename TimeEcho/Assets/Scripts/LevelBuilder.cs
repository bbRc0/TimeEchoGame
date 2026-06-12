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
    static readonly Color BackdropCol = new Color(0.10f, 0.11f, 0.17f);

    public static LevelRefs Build(string[] map, int levelIndex, Transform root)
    {
        var refs = new LevelRefs();
        int rows = map.Length;
        int cols = map[0].Length;
        refs.Width = cols;
        refs.Height = rows;
        bool art = GameSprites.Ok;
        var rng = new System.Random(levelIndex * 977 + 41);

        Vector2 Center(int c, int r) => new Vector2(c + 0.5f, (rows - 1 - r) + 0.5f);

        SpriteFactory.NewQuad("BackdropFill", root, new Vector2(cols * 0.5f, rows * 0.5f),
            new Vector2(cols, rows), BackdropCol, -20);
        Backdrop.Spawn(levelIndex, cols, rows, root);

        // --- Duvar gorselleri: karo karo (ustu acik olanlar cimenli) ---
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (map[r][c] != '#') continue;
                var pos = Center(c, r);
                var go = new GameObject("Wall");
                go.transform.SetParent(root, false);
                go.transform.position = pos;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sharedMaterial = SpriteFactory.SpriteMat;
                sr.sortingOrder = 0;
                if (art)
                {
                    bool exposed = r > 0 && map[r - 1][c] != '#';
                    if (exposed)
                        sr.sprite = (c % 2 == 0) ? GameSprites.WallTop : GameSprites.WallTop2;
                    else
                        sr.sprite = ((c + r) % 2 == 0) ? GameSprites.WallFill : GameSprites.WallFill2;
                }
                else
                {
                    sr.sprite = SpriteFactory.Square;
                    sr.color = Ground;
                }
            }
        }

        // --- Carpismalar: satir basina bitisik '#' kosulari tek collider ---
        for (int r = 0; r < rows; r++)
        {
            int c = 0;
            while (c < cols)
            {
                if (map[r][c] != '#') { c++; continue; }
                int start = c;
                while (c < cols && map[r][c] == '#') c++;
                int len = c - start;
                var go = new GameObject("WallCol");
                go.transform.SetParent(root, false);
                go.transform.position = new Vector2(start + len * 0.5f, (rows - 1 - r) + 0.5f);
                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(len, 1f);
            }
        }

        // --- Kapilar: sutun basina bitisik ayni harf kosulari tek kapi ---
        var doorLetters = new Dictionary<Door, char>();
        for (int c = 0; c < cols; c++)
        {
            int r = 0;
            while (r < rows)
            {
                char ch = map[r][c];
                if (ch < 'A' || ch > 'C') { r++; continue; }
                int start = r;
                while (r < rows && map[r][c] == ch) r++;
                int len = r - start;
                float midRow = start + (len - 1) * 0.5f;
                var pos = new Vector2(c + 0.5f, (rows - 1 - midRow) + 0.5f);
                var door = Door.Spawn(pos, len, root);
                doorLetters[door] = char.ToLower(ch);
                refs.Doors.Add(door);
            }
        }

        // --- Lazerler: sutun basina bitisik 'L' kosulari tek isin ---
        int laserIdx = 0;
        for (int c = 0; c < cols; c++)
        {
            int r = 0;
            while (r < rows)
            {
                if (map[r][c] != 'L') { r++; continue; }
                int start = r;
                while (r < rows && map[r][c] == 'L') r++;
                int len = r - start;
                float midRow = start + (len - 1) * 0.5f;
                var pos = new Vector2(c + 0.5f, (rows - 1 - midRow) + 0.5f);
                // Lazerler sirayla zit fazda calisir (8. bolum ritim bulmacasi)
                float phase = (laserIdx % 2 == 0) ? 0f : LaserBeam.OnTime;
                LaserBeam.Spawn(pos, len, phase, root);
                laserIdx++;
            }
        }

        // --- Tekil hucreler ---
        var platesByLetter = new Dictionary<char, List<PressurePlate>>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                char ch = map[r][c];
                switch (ch)
                {
                    case 'P':
                        refs.Spawn = Center(c, r);
                        break;
                    case 'E':
                        ExitZone.Spawn(Center(c, r), root);
                        break;
                    case '^':
                        Spike.Spawn(Center(c, r), root);
                        break;
                    default:
                        if (ch >= 'a' && ch <= 'c')
                        {
                            var plate = PressurePlate.Spawn(Center(c, r), root);
                            refs.Plates.Add(plate);
                            if (!platesByLetter.TryGetValue(ch, out var list))
                                platesByLetter[ch] = list = new List<PressurePlate>();
                            list.Add(plate);
                        }
                        break;
                }
            }
        }

        // Kapi -> ayni harfli butonlar
        foreach (var kv in doorLetters)
        {
            if (platesByLetter.TryGetValue(kv.Value, out var list))
                kv.Key.Plates = list;
        }

        // --- Susleme: zemin ustundeki bos hucrelere cim/agac/kaktus serpistir ---
        if (art)
        {
            for (int r = 1; r < rows - 1; r++)
            {
                for (int c = 1; c < cols - 1; c++)
                {
                    if (map[r][c] != ' ' || map[r + 1][c] != '#') continue;
                    if (rng.NextDouble() > 0.20) continue;
                    var go = new GameObject("Decor");
                    go.transform.SetParent(root, false);
                    go.transform.position = Center(c, r);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = GameSprites.Decor(rng.Next(5));
                    sr.sharedMaterial = SpriteFactory.SpriteMat;
                    sr.sortingOrder = 1;
                }
            }
        }

        refs.Wave = TimeWave.Spawn(cols, rows, root);
        return refs;
    }
}
