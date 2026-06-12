using UnityEngine;

// Periyodik acilip kapanan dikey lazer. Zamani tur saatine (LoopTime) bagli
// oldugu icin her turda ayni anda yanar/soner — hayalet kayitlariyla tutarli.
public class LaserBeam : MonoBehaviour
{
    public const float Cycle = 3.0f;   // tam dongu suresi
    public const float OnTime = 1.5f;  // dongunun yanik kismi
    const float Warn = 0.35f;          // yanmadan once gorsel uyari suresi

    float phaseOffset;
    float beamLen;
    Vector2 center;
    SpriteRenderer core, glow;

    public static LaserBeam Spawn(Vector2 center, int tilesTall, float phaseOffset, Transform parent)
    {
        var go = new GameObject("Laser");
        go.transform.SetParent(parent, false);
        go.transform.position = center;

        var lb = go.AddComponent<LaserBeam>();
        lb.center = center;
        lb.beamLen = tilesTall;
        lb.phaseOffset = phaseOffset;

        lb.glow = SpriteFactory.NewQuad("Glow", go.transform, center, new Vector2(0.55f, tilesTall), new Color(1f, 0.25f, 0.2f, 0.18f), 6);
        lb.core = SpriteFactory.NewQuad("Core", go.transform, center, new Vector2(0.18f, tilesTall), new Color(1f, 0.45f, 0.35f, 0.95f), 7);

        // Uclardaki yayici basliklar
        var capC = new Color(0.30f, 0.30f, 0.40f);
        SpriteFactory.NewQuad("CapTop", go.transform, center + new Vector2(0f, tilesTall * 0.5f - 0.12f), new Vector2(0.7f, 0.24f), capC, 8);
        SpriteFactory.NewQuad("CapBot", go.transform, center + new Vector2(0f, -tilesTall * 0.5f + 0.12f), new Vector2(0.7f, 0.24f), capC, 8);
        return lb;
    }

    float Phase => Mathf.Repeat((LoopManager.I != null ? LoopManager.I.LoopTime : 0f) + phaseOffset, Cycle);
    public bool IsOn => Phase < OnTime;

    void FixedUpdate()
    {
        if (LoopManager.I == null || !LoopManager.I.IsPlaying || !IsOn) return;

        var hits = Physics2D.OverlapBoxAll(center, new Vector2(0.45f, beamLen - 0.1f), 0f);
        foreach (var h in hits)
        {
            var rb = h.attachedRigidbody;
            if (rb != null && rb.GetComponent<PlayerController>() != null)
            {
                LoopManager.I.PlayerDied("Lazere yakalandın!");
                return;
            }
        }
    }

    void Update()
    {
        float p = Phase;
        float coreA, glowA;
        if (p < OnTime)
        {
            // yanik: hafif titresim
            float flicker = 0.85f + 0.15f * Mathf.Sin(Time.time * 40f);
            coreA = 0.95f * flicker;
            glowA = 0.20f * flicker;
        }
        else if (p > Cycle - Warn)
        {
            // yanmak uzere: uyari parlamasi
            float t = (p - (Cycle - Warn)) / Warn;
            coreA = 0.25f * t;
            glowA = 0.06f * t;
        }
        else
        {
            coreA = 0.05f;
            glowA = 0.0f;
        }

        var c = core.color; c.a = coreA; core.color = c;
        var g = glow.color; g.a = glowA; glow.color = g;
    }
}
