using System.Collections.Generic;
using UnityEngine;

// Hareketli arka plan: yanip sonen yildizlar + farkli hizlarda suzulen
// gece bulutlari (paralaks hissi). Bolum indeksine gore tohumlanir,
// yani her bolumun gokyuzu farkli ama deterministiktir.
public class Backdrop : MonoBehaviour
{
    class Star { public SpriteRenderer sr; public float baseA, speed, phase; }
    class Cloud { public Transform t; public float speed, size; }

    readonly List<Star> stars = new List<Star>();
    readonly List<Cloud> clouds = new List<Cloud>();
    float w, h;

    public static Backdrop Spawn(int seed, float w, float h, Transform parent)
    {
        var go = new GameObject("Backdrop");
        go.transform.SetParent(parent, false);
        var bd = go.AddComponent<Backdrop>();
        bd.w = w; bd.h = h;
        var rng = new System.Random(seed * 131 + 17);
        float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);

        // Yildizlar (uzak katman)
        for (int i = 0; i < 30; i++)
        {
            float size = R(0.04f, 0.10f);
            var sr = SpriteFactory.NewQuad("Star", go.transform,
                new Vector2(R(0.5f, w - 0.5f), R(2.5f, h - 0.5f)),
                new Vector2(size, size),
                new Color(0.9f, 0.92f, 1f, 0.6f), -18);
            bd.stars.Add(new Star { sr = sr, baseA = R(0.25f, 0.7f), speed = R(0.8f, 2.6f), phase = R(0f, 6.28f) });
        }

        // Bulutlar (yakin katman, sola suzulur)
        for (int i = 0; i < 6; i++)
        {
            var cgo = new GameObject("Cloud");
            cgo.transform.SetParent(go.transform, false);
            cgo.transform.position = new Vector2(R(0f, w), R(h * 0.45f, h - 1.2f));
            float scl = R(1.4f, 2.6f);
            cgo.transform.localScale = new Vector3(scl, scl, 1f);

            var sr = cgo.AddComponent<SpriteRenderer>();
            var art = GameSprites.Cloud(i);
            if (art != null) sr.sprite = art;
            else { sr.sprite = SpriteFactory.Square; cgo.transform.localScale = new Vector3(scl * 1.6f, scl * 0.5f, 1f); }
            sr.sharedMaterial = SpriteFactory.SpriteMat;
            // Gece tonu: bulutlari koyu maviye boya
            sr.color = new Color(0.45f, 0.52f, 0.78f, R(0.25f, 0.45f));
            sr.sortingOrder = -16 + (i % 2); // hafif derinlik cesidi

            bd.clouds.Add(new Cloud { t = cgo.transform, speed = R(0.15f, 0.55f), size = scl });
        }

        return bd;
    }

    void Update()
    {
        float t = Time.time;
        foreach (var s in stars)
        {
            var c = s.sr.color;
            c.a = s.baseA * (0.55f + 0.45f * Mathf.Sin(t * s.speed + s.phase));
            s.sr.color = c;
        }
        foreach (var c in clouds)
        {
            var p = c.t.position;
            p.x -= c.speed * Time.deltaTime;
            if (p.x < -c.size - 1f) p.x = w + c.size + 1f; // sagdan geri gir
            c.t.position = p;
        }
    }
}
