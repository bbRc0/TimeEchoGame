using UnityEngine;

public class ExitZone : MonoBehaviour
{
    Transform glow;

    public static ExitZone Spawn(Vector2 cellCenter, Transform parent)
    {
        var go = new GameObject("Exit");
        go.transform.SetParent(parent, false);
        Vector2 pos = cellCenter + new Vector2(0f, 0.2f);
        go.transform.position = pos;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.9f, 1.4f);

        var e = go.AddComponent<ExitZone>();
        SpriteFactory.NewQuad("Frame", go.transform, pos, new Vector2(0.95f, 1.45f), new Color(0.20f, 0.55f, 0.32f), 2);
        e.glow = SpriteFactory.NewQuad("Glow", go.transform, pos, new Vector2(0.7f, 1.2f), new Color(0.35f, 0.95f, 0.55f), 3).transform;
        return e;
    }

    void Update()
    {
        float s = 1f + Mathf.Sin(Time.time * 4f) * 0.06f;
        glow.localScale = new Vector3(0.7f * s, 1.2f * s, 1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var rb = other.attachedRigidbody;
        if (rb != null && rb.GetComponent<PlayerController>() != null && LoopManager.I != null)
            LoopManager.I.ExitReached();
    }
}
