using UnityEngine;

// Cikis: iki karo yuksekliginde ahsap kapi + arkasinda nabiz gibi atan isilti.
public class ExitZone : MonoBehaviour
{
    Transform glow;
    float glowBaseW = 0.9f, glowBaseH = 2.0f;

    public static ExitZone Spawn(Vector2 cellCenter, Transform parent)
    {
        var go = new GameObject("Exit");
        go.transform.SetParent(parent, false);
        go.transform.position = cellCenter;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.9f, 1.6f);
        col.offset = new Vector2(0f, 0.5f);

        var e = go.AddComponent<ExitZone>();

        var artBottom = GameSprites.ExitBottom;
        var artTop = GameSprites.ExitTop;
        if (artBottom != null && artTop != null)
        {
            var b = new GameObject("DoorBottom");
            b.transform.SetParent(go.transform, false);
            var bsr = b.AddComponent<SpriteRenderer>();
            bsr.sprite = artBottom; bsr.sharedMaterial = SpriteFactory.SpriteMat; bsr.sortingOrder = 3;

            var t = new GameObject("DoorTop");
            t.transform.SetParent(go.transform, false);
            t.transform.localPosition = new Vector3(0f, 1f, 0f);
            var tsr = t.AddComponent<SpriteRenderer>();
            tsr.sprite = artTop; tsr.sharedMaterial = SpriteFactory.SpriteMat; tsr.sortingOrder = 3;
        }
        else
        {
            SpriteFactory.NewQuad("Frame", go.transform, cellCenter + new Vector2(0f, 0.5f),
                new Vector2(0.95f, 1.9f), new Color(0.20f, 0.55f, 0.32f), 3);
        }

        // Isilti: kapinin arkasinda yumusak radyal parilti (kare blok degil)
        var ggo = new GameObject("Glow");
        ggo.transform.SetParent(go.transform, false);
        ggo.transform.position = cellCenter + new Vector2(0f, 0.5f);
        var gsr = ggo.AddComponent<SpriteRenderer>();
        gsr.sprite = SpriteFactory.Glow;
        gsr.sharedMaterial = SpriteFactory.SpriteMat;
        gsr.color = new Color(0.45f, 0.95f, 0.55f, 0.45f);
        gsr.sortingOrder = 2;
        e.glow = ggo.transform;
        e.glowBaseW = 2.4f;   // radyal isik kapidan genis ama yumusak kayboluyor
        e.glowBaseH = 3.0f;
        return e;
    }

    void Update()
    {
        float s = 1f + Mathf.Sin(Time.time * 3.5f) * 0.10f;
        glow.localScale = new Vector3(glowBaseW * s, glowBaseH * s, 1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var rb = other.attachedRigidbody;
        if (rb != null && rb.GetComponent<PlayerController>() != null && LoopManager.I != null)
            LoopManager.I.ExitReached();
    }
}
