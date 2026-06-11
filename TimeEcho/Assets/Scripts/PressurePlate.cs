using UnityEngine;

// Ustunde bir beden (oyuncu veya hayalet) durdugu surece basili kalir.
public class PressurePlate : MonoBehaviour
{
    public bool IsPressed { get; private set; }

    static readonly Color Idle = new Color(1f, 0.83f, 0.25f);
    static readonly Color Down = new Color(0.45f, 0.95f, 0.35f);

    SpriteRenderer sr;
    Vector2 basePos;

    // cellCenter = plakanin oturdugu karenin merkezi.
    public static PressurePlate Spawn(Vector2 cellCenter, Transform parent)
    {
        var go = new GameObject("Plate");
        go.transform.SetParent(parent, false);
        Vector2 pos = cellCenter + new Vector2(0f, -0.39f);
        go.transform.position = pos;

        var p = go.AddComponent<PressurePlate>();
        p.basePos = pos;
        p.sr = SpriteFactory.NewQuad("Visual", go.transform, pos, new Vector2(0.9f, 0.22f), Idle, 4);
        SpriteFactory.NewQuad("Base", go.transform, pos + new Vector2(0f, -0.08f), new Vector2(1f, 0.10f), new Color(0.25f, 0.27f, 0.38f), 3);
        return p;
    }

    void FixedUpdate()
    {
        bool was = IsPressed;
        IsPressed = false;
        var hits = Physics2D.OverlapBoxAll(basePos + new Vector2(0f, 0.35f), new Vector2(0.95f, 0.7f), 0f);
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
        sr.color = Color.Lerp(sr.color, IsPressed ? Down : Idle, Time.deltaTime * 12f);
        var target = IsPressed ? basePos + new Vector2(0f, -0.06f) : basePos;
        sr.transform.position = Vector3.Lerp(sr.transform.position, (Vector3)target, Time.deltaTime * 12f);
    }
}
