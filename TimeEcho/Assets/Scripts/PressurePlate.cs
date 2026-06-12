using UnityEngine;

// Ustunde bir beden (oyuncu veya hayalet) durdugu surece basili kalir.
public class PressurePlate : MonoBehaviour
{
    public bool IsPressed { get; private set; }

    SpriteRenderer sr;
    Vector3 visualBase;
    Vector2 cellCenter;
    bool artMode;

    static readonly Color IdleFallback = new Color(1f, 0.83f, 0.25f);
    static readonly Color DownFallback = new Color(0.45f, 0.95f, 0.35f);

    public static PressurePlate Spawn(Vector2 cellCenter, Transform parent)
    {
        var go = new GameObject("Plate");
        go.transform.SetParent(parent, false);
        go.transform.position = cellCenter;

        var p = go.AddComponent<PressurePlate>();
        p.cellCenter = cellCenter;

        // Butonun kaidesi katidir: ustune cikilir, uzerinde durarak basili tutulur.
        var baseCol = go.AddComponent<BoxCollider2D>();
        baseCol.size = new Vector2(0.85f, 0.32f);
        baseCol.offset = new Vector2(0f, -0.34f);

        var vgo = new GameObject("Visual");
        vgo.transform.SetParent(go.transform, false);
        p.sr = vgo.AddComponent<SpriteRenderer>();
        p.sr.sharedMaterial = SpriteFactory.SpriteMat;
        p.sr.sortingOrder = 4;

        var art = GameSprites.PlateBtn;
        if (art != null)
        {
            p.artMode = true;
            p.sr.sprite = art;          // kirmizi buton (alt yarida govde)
            p.sr.color = Color.white;
        }
        else
        {
            p.sr.sprite = SpriteFactory.Square;
            vgo.transform.localScale = new Vector3(0.9f, 0.22f, 1f);
            vgo.transform.localPosition = new Vector3(0f, -0.39f, 0f);
            p.sr.color = IdleFallback;
        }
        p.visualBase = p.sr.transform.localPosition;
        return p;
    }

    void FixedUpdate()
    {
        bool was = IsPressed;
        IsPressed = false;
        // Algilama bolgesi kaidenin USTU ve kaideden DAR: yandan yaslanan basamaz,
        // sadece ustunde duran beden butonu basili tutar.
        var hits = Physics2D.OverlapBoxAll(cellCenter + new Vector2(0f, 0.18f), new Vector2(0.78f, 0.5f), 0f);
        foreach (var h in hits)
        {
            var rb = h.attachedRigidbody;
            if (rb == null) continue;
            if (rb.GetComponent<PlayerController>() != null || rb.GetComponent<GhostReplay>() != null)
            {
                IsPressed = true;
                break;
            }
        }

        if (IsPressed && !was && LoopManager.I != null && LoopManager.I.IsPlaying)
            Sfx.Button();
    }

    void Update()
    {
        // Basilinca buton hafifce coker; sprite modunda yesilimsi parlar.
        var targetPos = visualBase + (IsPressed ? new Vector3(0f, -0.07f, 0f) : Vector3.zero);
        sr.transform.localPosition = Vector3.Lerp(sr.transform.localPosition, targetPos, Time.deltaTime * 12f);

        Color target;
        if (artMode)
            target = IsPressed ? new Color(0.75f, 1f, 0.75f, 1f) : Color.white;
        else
            target = IsPressed ? DownFallback : IdleFallback;
        sr.color = Color.Lerp(sr.color, target, Time.deltaTime * 12f);
    }
}
