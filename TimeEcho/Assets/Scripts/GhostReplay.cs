using System.Collections.Generic;
using UnityEngine;

// Gecmis bir turun birebir tekrari. Butonlara basabilir ve ustune cikilabilir
// (tek yonlu platform), ama oyuncuyu yandan itmez.
public class GhostReplay : MonoBehaviour
{
    // Hayalet, OYUNCUNUN kendi sprite'ini soluk mavi-beyaz tonda kullanir:
    // ayni siluet = "gecmisteki sen" aninda okunur, yanlis anlasilmaz.
    static readonly Color GhostTint = new Color(0.72f, 0.85f, 1f, 0.55f);

    List<Vector2> frames;
    Rigidbody2D rb;
    SpriteRenderer bodySr;
    bool artMode;
    bool vanished;
    int facing = 1;
    float animTimer;
    bool walkFrame;

    public static GhostReplay Spawn(List<Vector2> recording, Transform parent)
    {
        var go = new GameObject("Ghost");
        go.transform.SetParent(parent, false);
        Vector2 start = recording.Count > 0 ? recording[0] : Vector2.zero;
        go.transform.position = start;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;                    // hicbir kosulda donmesin
        go.transform.rotation = Quaternion.identity;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.95f);
        col.usedByEffector = true;

        var eff = go.AddComponent<PlatformEffector2D>();
        eff.useOneWay = true;
        eff.surfaceArc = 170f;

        var g = go.AddComponent<GhostReplay>();
        g.frames = recording;

        var body = new GameObject("Body");
        body.transform.SetParent(go.transform, false);
        g.bodySr = body.AddComponent<SpriteRenderer>();
        g.bodySr.sharedMaterial = SpriteFactory.SpriteMat;
        g.bodySr.sortingOrder = 8;

        var art = GameSprites.PlayerIdle;
        if (art != null)
        {
            g.artMode = true;
            g.bodySr.sprite = art;       // oyuncuyla ayni astronot
            g.bodySr.color = GhostTint;  // soluk, yari saydam "yanki" gorunumu
        }
        else
        {
            g.bodySr.sprite = SpriteFactory.Square;
            g.bodySr.color = new Color(0.42f, 0.88f, 1f, 0.42f);
            body.transform.localScale = new Vector3(0.8f, 0.95f, 1f);
        }
        return g;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (vanished || LoopManager.I == null || !LoopManager.I.IsPlaying) return;

        int f = LoopManager.I.Frame;
        if (f < frames.Count)
        {
            bool moving = false;
            if (f > 0)
            {
                float dx = frames[f].x - frames[f - 1].x;
                if (dx > 0.001f) facing = 1;
                else if (dx < -0.001f) facing = -1;
                moving = Mathf.Abs(dx) > 0.02f;
            }
            if (artMode)
            {
                // Art sola bakar; saga giderken aynala (oyuncuyla ayni kural).
                bodySr.flipX = facing > 0;
                if (moving)
                {
                    animTimer -= Time.fixedDeltaTime;
                    if (animTimer <= 0f) { walkFrame = !walkFrame; animTimer = 0.12f; }
                }
                else walkFrame = false;
                bodySr.sprite = walkFrame ? GameSprites.PlayerWalk : GameSprites.PlayerIdle;
            }
            rb.MovePosition(frames[f]);
        }
        else
        {
            // Kaydin sonu = bu kopyanin oldugu an. Hayalet de ayni anda yok olur.
            vanished = true;
            Fx.Burst(rb.position, new Color(0.42f, 0.88f, 1f, 0.8f), 8);
            Sfx.GhostVanish();
            gameObject.SetActive(false);
        }
    }
}
