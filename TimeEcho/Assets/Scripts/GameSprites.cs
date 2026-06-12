using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Kenney "Pixel Platformer" (CC0) sayfalarini calisma zamaninda yukleyip dilimler.
// Dosyalar yoksa Ok=false olur ve cagiran kod renkli kare yedegine duser.
public static class GameSprites
{
    static Texture2D tiles;   // 18x18, 20x9 izgara
    static Texture2D chars;   // 24x24, 9x3 izgara
    static bool loaded;
    static readonly Dictionary<int, Sprite> cache = new Dictionary<int, Sprite>();

    public static bool Ok
    {
        get { Load(); return tiles != null && chars != null; }
    }

    public static Texture2D TilesTex { get { Load(); return tiles; } }

    static void Load()
    {
        if (loaded) return;
        loaded = true;
        tiles = LoadTex("tilemap_packed.png");
        chars = LoadTex("tilemap-characters_packed.png");

        // Sigorta: bazi platform/Unity surumleri PNG'yi ters (bas asagi) yukleyebilir.
        // Karo sayfasinin ust seridi dolu bloklardir; ters yuklendiyse ust serit
        // rakam karolarinin SEFFAF arka planina denk gelir — tespit edip ceviririz.
        if (tiles != null && LooksFlipped(tiles))
        {
            FlipRows(tiles);
            if (chars != null) FlipRows(chars);
        }
    }

    static bool LooksFlipped(Texture2D t)
    {
        int y = t.height - 3, opaque = 0, n = 0;
        for (int x = 2; x < t.width; x += 7) { n++; if (t.GetPixel(x, y).a > 0.5f) opaque++; }
        return opaque < n / 2;
    }

    static void FlipRows(Texture2D t)
    {
        var px = t.GetPixels32();
        var outp = new Color32[px.Length];
        int w = t.width, h = t.height;
        for (int r = 0; r < h; r++)
            System.Array.Copy(px, r * w, outp, (h - 1 - r) * w, w);
        t.SetPixels32(outp);
        t.Apply();
    }

    static Texture2D LoadTex(string file)
    {
        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, "TimeEcho", file);
            if (!File.Exists(path)) return null;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(File.ReadAllBytes(path))) return null;
            tex.filterMode = FilterMode.Point;
            return tex;
        }
        catch { return null; }
    }

    static Sprite Slice(Texture2D sheet, int sheetId, int c, int r, int cell)
    {
        if (sheet == null) return null;
        int key = (sheetId << 20) | (c << 10) | r;
        if (cache.TryGetValue(key, out var s)) return s;
        // PNG sol-ust kose (0,0); Unity dokusu sol-alt -> satiri ters cevir.
        int rows = sheet.height / cell;
        var rect = new Rect(c * cell, (rows - 1 - r) * cell, cell, cell);
        s = Sprite.Create(sheet, rect, new Vector2(0.5f, 0.5f), cell, 0,
            SpriteMeshType.FullRect);
        cache[key] = s;
        return s;
    }

    public static Sprite Tile(int c, int r) { Load(); return Slice(tiles, 0, c, r, 18); }
    public static Sprite Char(int c, int r) { Load(); return Slice(chars, 1, c, r, 24); }

    // --- Secilmis kareler ---
    public static Sprite WallTop => Tile(1, 0);          // cimenli toprak
    public static Sprite WallTop2 => Tile(2, 0);
    public static Sprite WallFill => Tile(1, 6);         // ic toprak
    public static Sprite WallFill2 => Tile(2, 6);
    public static Sprite DoorLock => Tile(8, 1);         // altin kilit blogu (bulmaca kapisi)
    public static Sprite PlateBtn => Tile(7, 5);         // kirmizi buton
    public static Sprite Spikes => Tile(8, 3);           // beyaz cift diken
    public static Sprite ExitTop => Tile(10, 6);         // ahsap kapi ust
    public static Sprite ExitBottom => Tile(10, 7);      // ahsap kapi alt
    public static Sprite Cloud(int i) => Tile(13 + (i % 4), 7);
    public static Sprite Decor(int i)
    {
        // cim, cim2, agac, kaktus, mantar
        switch (i % 5)
        {
            case 0: return Tile(4, 6);
            case 1: return Tile(5, 6);
            case 2: return Tile(6, 6);
            case 3: return Tile(7, 6);
            default: return Tile(8, 6);
        }
    }

    public static Sprite PlayerIdle => Char(6, 0);       // turuncu astronot
    public static Sprite PlayerWalk => Char(7, 0);
    // Hayalet ayri sprite kullanmaz: oyuncu karesi soluk tonla boyanir (GhostReplay).

    // HUD icin: karoyu dogrudan IMGUI'ye cizer (kalpler vb.)
    public static void DrawTileGUI(Rect r, int c, int row)
    {
        Load();
        if (tiles == null) return;
        float w = tiles.width, h = tiles.height;
        int rows = tiles.height / 18;
        var tc = new Rect(c * 18f / w, (rows - 1 - row) * 18f / h, 18f / w, 18f / h);
        GUI.DrawTextureWithTexCoords(r, tiles, tc);
    }
}
