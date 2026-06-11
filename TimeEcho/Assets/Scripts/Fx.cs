using UnityEngine;

// Olum / yok olus patlamasi: kisa omurlu, carpismasiz kare parcaciklar.
public static class Fx
{
    public static void Burst(Vector2 pos, Color color, int count = 14)
    {
        for (int i = 0; i < count; i++)
        {
            float size = Random.Range(0.10f, 0.22f);
            var sr = SpriteFactory.NewQuad("FxParticle", null, pos, new Vector2(size, size), color, 30);
            var rb = sr.gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2f;
            rb.linearVelocity = Random.insideUnitCircle * 6f + Vector2.up * 4f;
            rb.angularVelocity = Random.Range(-360f, 360f);
            sr.gameObject.AddComponent<FxParticle>();
        }
    }
}

public class FxParticle : MonoBehaviour
{
    const float Life = 0.7f;
    float t;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t >= Life) { Destroy(gameObject); return; }
        var c = sr.color;
        c.a = 1f - t / Life;
        sr.color = c;
    }
}
