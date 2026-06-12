using UnityEngine;

// Zemin dikeni: dokunan oyuncuyu oldurur. Hayaletler etkilenmez
// (onlar zaten gecmisin kaydi - dikene basmadiklari icin hayatta kaldilar).
public class Spike : MonoBehaviour
{
    Vector2 checkCenter;
    static readonly Vector2 CheckSize = new Vector2(0.7f, 0.35f);

    public static Spike Spawn(Vector2 cellCenter, Transform parent)
    {
        var go = new GameObject("Spike");
        go.transform.SetParent(parent, false);
        go.transform.position = cellCenter;

        var sr = go.AddComponent<SpriteRenderer>();
        var art = GameSprites.Spikes;
        if (art != null)
        {
            sr.sprite = art;
        }
        else
        {
            sr.sprite = SpriteFactory.Square;
            sr.color = new Color(0.85f, 0.88f, 0.95f);
            go.transform.localScale = new Vector3(0.9f, 0.4f, 1f);
        }
        sr.sharedMaterial = SpriteFactory.SpriteMat;
        sr.sortingOrder = 3;

        var sp = go.AddComponent<Spike>();
        // Dikenler karonun alt yarisinda durur; olum bolgesi de orasi.
        sp.checkCenter = cellCenter + new Vector2(0f, -0.25f);
        return sp;
    }

    void FixedUpdate()
    {
        if (LoopManager.I == null || !LoopManager.I.IsPlaying) return;

        var hits = Physics2D.OverlapBoxAll(checkCenter, CheckSize, 0f);
        foreach (var h in hits)
        {
            var rb = h.attachedRigidbody;
            if (rb != null && rb.GetComponent<PlayerController>() != null)
            {
                LoopManager.I.PlayerDied("Dikenlere bastın!");
                return;
            }
        }
    }
}
