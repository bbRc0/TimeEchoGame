using System.Collections.Generic;
using UnityEngine;

// Bolumdeki tum butonlar basiliyken acilir. Kapanirken icinde kalan
// oyuncuyu ezer — bu da dogal bir tur sonudur.
public class Door : MonoBehaviour
{
    public List<PressurePlate> Plates = new List<PressurePlate>();

    BoxCollider2D col;
    SpriteRenderer sr;
    bool open;
    Vector2 size;

    static readonly Color Closed = new Color(0.86f, 0.30f, 0.36f);

    public static Door Spawn(Vector2 center, int tilesTall, Transform parent)
    {
        var go = new GameObject("Door");
        go.transform.SetParent(parent, false);
        go.transform.position = center;

        var d = go.AddComponent<Door>();
        d.size = new Vector2(0.96f, tilesTall);
        d.col = go.AddComponent<BoxCollider2D>();
        d.col.size = d.size;
        d.sr = SpriteFactory.NewQuad("Visual", go.transform, center, d.size, Closed, 5);
        return d;
    }

    void FixedUpdate()
    {
        bool shouldOpen = Plates.Count > 0;
        foreach (var p in Plates)
            if (!p.IsPressed) { shouldOpen = false; break; }

        if (shouldOpen == open) return;
        open = shouldOpen;
        col.enabled = !open;

        if (!open && LoopManager.I != null && LoopManager.I.IsPlaying)
        {
            // Kapi kapanirken oyuncu araliktaysa ezilir.
            var hits = Physics2D.OverlapBoxAll(transform.position, size * 0.95f, 0f);
            foreach (var h in hits)
            {
                var rb = h.attachedRigidbody;
                if (rb != null && rb.GetComponent<PlayerController>() != null)
                {
                    LoopManager.I.PlayerDied("Kapıya sıkışıp öldün!");
                    break;
                }
            }
        }
    }

    void Update()
    {
        var c = Closed;
        c.a = open ? 0.12f : 1f;
        sr.color = Color.Lerp(sr.color, c, Time.deltaTime * 14f);
    }
}
