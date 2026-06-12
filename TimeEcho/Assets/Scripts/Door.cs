using System.Collections.Generic;
using UnityEngine;

// Esleştigi harfin TUM butonlari basiliyken acilir. Kapanirken icinde kalan
// oyuncuyu ezer — bu da dogal bir tur sonudur.
public class Door : MonoBehaviour
{
    public List<PressurePlate> Plates = new List<PressurePlate>();

    BoxCollider2D col;
    SpriteRenderer sr;
    bool open;
    Vector2 size;

    static readonly Color ClosedTintArt = Color.white;
    static readonly Color ClosedTintFallback = new Color(0.86f, 0.30f, 0.36f);
    Color closedTint;

    public static Door Spawn(Vector2 center, int tilesTall, Transform parent)
    {
        var go = new GameObject("Door");
        go.transform.SetParent(parent, false);
        go.transform.position = center;

        var d = go.AddComponent<Door>();
        d.size = new Vector2(0.96f, tilesTall);
        d.col = go.AddComponent<BoxCollider2D>();
        d.col.size = d.size;

        var vgo = new GameObject("Visual");
        vgo.transform.SetParent(go.transform, false);
        d.sr = vgo.AddComponent<SpriteRenderer>();
        d.sr.sharedMaterial = SpriteFactory.SpriteMat;
        d.sr.sortingOrder = 5;

        var art = GameSprites.DoorLock;
        if (art != null)
        {
            // Kilitli altin bloklardan dikey sutun (tiled)
            d.sr.sprite = art;
            d.sr.drawMode = SpriteDrawMode.Tiled;
            d.sr.size = new Vector2(1f, tilesTall);
            d.closedTint = ClosedTintArt;
        }
        else
        {
            d.sr.sprite = SpriteFactory.Square;
            vgo.transform.localScale = new Vector3(0.96f, tilesTall, 1f);
            d.closedTint = ClosedTintFallback;
        }
        d.sr.color = d.closedTint;
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
        var c = closedTint;
        c.a = open ? 0.15f : 1f;
        sr.color = Color.Lerp(sr.color, c, Time.deltaTime * 14f);
    }
}
