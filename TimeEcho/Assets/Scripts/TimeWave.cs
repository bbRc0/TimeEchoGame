using UnityEngine;

// Turu bitiren olum: her turda ayni anda soldan dogan ve sahayi supuren dalga.
// Oyuncuya yetistigi anda tur biter ve hayalet dogar.
public class TimeWave : MonoBehaviour
{
    public const float StartDelay = 8f;
    public const float Speed = 9f;

    const float OriginX = -1.5f;

    float levelW, levelH;
    Transform fill, edge;

    public float FrontX { get; private set; }
    public float TimeUntilStart => StartDelay - (LoopManager.I != null ? LoopManager.I.LoopTime : 0f);

    public static TimeWave Spawn(float levelW, float levelH, Transform parent)
    {
        var go = new GameObject("TimeWave");
        go.transform.SetParent(parent, false);

        var w = go.AddComponent<TimeWave>();
        w.levelW = levelW;
        w.levelH = levelH;
        w.FrontX = OriginX;

        var fillC = new Color(0.55f, 0.25f, 0.85f, 0.32f);
        var edgeC = new Color(0.85f, 0.45f, 1f, 0.95f);
        w.fill = SpriteFactory.NewQuad("Fill", go.transform, Vector2.zero, Vector2.one, fillC, 20).transform;
        w.edge = SpriteFactory.NewQuad("Edge", go.transform, Vector2.zero, Vector2.one, edgeC, 21).transform;
        w.fill.gameObject.SetActive(false);
        w.edge.gameObject.SetActive(false);
        return w;
    }

    void FixedUpdate()
    {
        // Oyun durdugunda (olum/bolum sonu) dalga oldugu yerde donar;
        // yeni tur basladiginda LoopTime sifirlandigi icin kendiliginden basa doner.
        if (LoopManager.I == null || !LoopManager.I.IsPlaying) return;

        float t = LoopManager.I.LoopTime;
        FrontX = OriginX + Mathf.Max(0f, t - StartDelay) * Speed;

        var p = LoopManager.I.Player;
        if (p != null && p.gameObject.activeSelf && p.transform.position.x - FrontX <= 0.3f)
            LoopManager.I.PlayerDied("Zaman dalgası sana yetişti!");
    }

    void Update()
    {
        bool active = FrontX > OriginX + 0.01f;
        fill.gameObject.SetActive(active);
        edge.gameObject.SetActive(active);
        if (!active) return;

        float left = OriginX;
        float w = Mathf.Max(0.05f, FrontX - left);
        fill.position = new Vector3(left + w * 0.5f, levelH * 0.5f, 0f);
        fill.localScale = new Vector3(w, levelH, 1f);
        edge.position = new Vector3(FrontX, levelH * 0.5f, 0f);
        edge.localScale = new Vector3(0.22f, levelH, 1f);
    }
}
