using UnityEngine;

// Tum gorseller kodla uretilen tek bir beyaz kareden turetilir (renk + olcek).
public static class SpriteFactory
{
    static Sprite square;
    static Sprite glow;
    static Material spriteMat;

    public static Sprite Square
    {
        get
        {
            if (square == null)
            {
                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                var px = new Color32[16];
                for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
                tex.SetPixels32(px);
                tex.Apply();
                tex.filterMode = FilterMode.Point;
                square = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }
            return square;
        }
    }

    // Kenarlara dogru yumusakca kaybolan dairesel isik (parilti efektleri icin).
    public static Sprite Glow
    {
        get
        {
            if (glow == null)
            {
                const int N = 64;
                var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
                var px = new Color32[N * N];
                float half = (N - 1) * 0.5f;
                for (int y = 0; y < N; y++)
                {
                    for (int x = 0; x < N; x++)
                    {
                        float d = Mathf.Sqrt((x - half) * (x - half) + (y - half) * (y - half)) / half;
                        float a = Mathf.Clamp01(1f - d);
                        a = a * a;   // yumusak dusus
                        px[y * N + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                    }
                }
                tex.SetPixels32(px);
                tex.Apply();
                tex.filterMode = FilterMode.Bilinear;
                glow = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), N);
            }
            return glow;
        }
    }

    public static Material SpriteMat
    {
        get
        {
            if (spriteMat == null)
            {
                var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (sh == null) sh = Shader.Find("Sprites/Default");
                spriteMat = new Material(sh);
            }
            return spriteMat;
        }
    }

    // pos = merkez, size = dunya birimi cinsinden boyut.
    public static SpriteRenderer NewQuad(string name, Transform parent, Vector2 pos, Vector2 size, Color color, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Square;
        sr.sharedMaterial = SpriteMat;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }
}
