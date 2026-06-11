using System.Collections.Generic;
using UnityEngine;

// Gecmis bir turun birebir tekrari. Butonlara basabilir ve ustune cikilabilir
// (tek yonlu platform), ama oyuncuyu yandan itmez.
public class GhostReplay : MonoBehaviour
{
    List<Vector2> frames;
    Rigidbody2D rb;
    Transform eyes;
    bool vanished;
    int facing = 1;

    public static GhostReplay Spawn(List<Vector2> recording, Transform parent)
    {
        var go = new GameObject("Ghost");
        go.transform.SetParent(parent, false);
        Vector2 start = recording.Count > 0 ? recording[0] : Vector2.zero;
        go.transform.position = start;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.95f);
        col.usedByEffector = true;

        var eff = go.AddComponent<PlatformEffector2D>();
        eff.useOneWay = true;
        eff.surfaceArc = 170f;

        var body = new Color(0.42f, 0.88f, 1f, 0.42f);
        SpriteFactory.NewQuad("Body", go.transform, start, new Vector2(0.8f, 0.95f), body, 8);

        var g = go.AddComponent<GhostReplay>();
        g.frames = recording;
        g.eyes = new GameObject("Eyes").transform;
        g.eyes.SetParent(go.transform, false);
        var eyeC = new Color(1f, 1f, 1f, 0.55f);
        SpriteFactory.NewQuad("EyeL", g.eyes, start, new Vector2(0.12f, 0.16f), eyeC, 9).transform.localPosition = new Vector3(-0.14f, 0.36f, 0f);
        SpriteFactory.NewQuad("EyeR", g.eyes, start, new Vector2(0.12f, 0.16f), eyeC, 9).transform.localPosition = new Vector3(0.14f, 0.36f, 0f);
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
            if (f > 0)
            {
                float dx = frames[f].x - frames[f - 1].x;
                if (dx > 0.001f) facing = 1;
                else if (dx < -0.001f) facing = -1;
                eyes.localPosition = new Vector3(0.10f * facing, 0f, 0f);
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
