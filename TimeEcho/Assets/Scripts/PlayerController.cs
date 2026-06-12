using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public const float MoveSpeed = 7f;
    public const float JumpVelocity = 11.9f;
    const float CoyoteTime = 0.10f;
    const float JumpBuffer = 0.12f;

    // Bu turda kat edilen yol; olum aninda LoopManager bunu hayalet kaydina cevirir.
    public readonly List<Vector2> Recording = new List<Vector2>();

    Rigidbody2D rb;
    SpriteRenderer bodySr;
    bool artMode;
    float inputX;
    float jumpBufferLeft;
    float coyoteLeft;
    int facing = 1;
    bool wasGrounded;
    float stepTimer;
    float prevPosX;
    float animTimer;
    bool walkFrame;
    bool walking;

    // Duvara tutunmayi onler: sifir surtunme => duvara yaslanip yavas dusme yok.
    static PhysicsMaterial2D slick;
    static PhysicsMaterial2D Slick
    {
        get
        {
            if (slick == null)
                slick = new PhysicsMaterial2D("Slick") { friction = 0f, bounciness = 0f };
            return slick;
        }
    }

    public static PlayerController Spawn(Vector2 pos, Transform parent)
    {
        var go = new GameObject("Player");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.95f);
        col.sharedMaterial = Slick;

        var pc = go.AddComponent<PlayerController>();

        var body = new GameObject("Body");
        body.transform.SetParent(go.transform, false);
        pc.bodySr = body.AddComponent<SpriteRenderer>();
        pc.bodySr.sharedMaterial = SpriteFactory.SpriteMat;
        pc.bodySr.sortingOrder = 10;

        var art = GameSprites.PlayerIdle;
        if (art != null)
        {
            pc.artMode = true;
            pc.bodySr.sprite = art;     // turuncu astronot, 24px = 1 birim
        }
        else
        {
            pc.bodySr.sprite = SpriteFactory.Square;
            pc.bodySr.color = new Color(1f, 0.71f, 0.28f);
            body.transform.localScale = new Vector3(0.8f, 0.95f, 1f);
        }
        return pc;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ResetTo(Vector2 pos)
    {
        gameObject.SetActive(true);
        rb.position = pos;
        transform.position = pos;
        rb.linearVelocity = Vector2.zero;
        Recording.Clear();
        jumpBufferLeft = 0f;
        coyoteLeft = 0f;
        prevPosX = pos.x;
        wasGrounded = false;
        stepTimer = 0f;
        facing = 1;
        if (bodySr != null) bodySr.flipX = true;   // sag yon = aynali (art sola bakar)
    }

    void Update()
    {
        var kb = Keyboard.current;
        inputX = 0f;
        if (kb != null && LoopManager.I != null && LoopManager.I.IsPlaying && !LoopManager.I.IsPaused)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) inputX -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) inputX += 1f;
            if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
                jumpBufferLeft = JumpBuffer;
        }
        jumpBufferLeft -= Time.deltaTime;

        if (inputX > 0.01f) facing = 1;
        else if (inputX < -0.01f) facing = -1;

        // Animasyon: yururken iki kare arasinda gecis yap
        if (artMode)
        {
            // Kenney astronot karesi dogal olarak SOLA bakar; saga giderken aynala.
            bodySr.flipX = facing > 0;
            if (walking)
            {
                animTimer -= Time.deltaTime;
                if (animTimer <= 0f)
                {
                    walkFrame = !walkFrame;
                    animTimer = 0.12f;
                }
            }
            else walkFrame = false;
            bodySr.sprite = walkFrame ? GameSprites.PlayerWalk : GameSprites.PlayerIdle;
        }
    }

    void FixedUpdate()
    {
        if (LoopManager.I == null || !LoopManager.I.IsPlaying)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        bool grounded = CheckGrounded();
        coyoteLeft = grounded ? CoyoteTime : coyoteLeft - Time.fixedDeltaTime;

        // Yere inis sesi (havadan zemine gecis, asagi dogru hizla)
        if (grounded && !wasGrounded && rb.linearVelocity.y < -2f)
            Sfx.Land();
        wasGrounded = grounded;

        var v = rb.linearVelocity;
        v.x = inputX * MoveSpeed;
        if (jumpBufferLeft > 0f && coyoteLeft > 0f)
        {
            v.y = JumpVelocity;
            jumpBufferLeft = 0f;
            coyoteLeft = 0f;
            Sfx.Jump();
        }
        rb.linearVelocity = v;

        // Yuruyus sesi: niyet edilen hiz degil, GERCEK yer degistirme baz alinir.
        // Boylece duvara/engele dayanip yerinde sayarken adim sesi gelmez.
        float actualDx = Mathf.Abs(rb.position.x - prevPosX);
        prevPosX = rb.position.x;
        walking = grounded && Mathf.Abs(inputX) > 0.01f && actualDx > 0.03f;
        if (walking)
        {
            stepTimer -= Time.fixedDeltaTime;
            if (stepTimer <= 0f) { Sfx.Step(); stepTimer = 0.27f; }
        }
        else stepTimer = 0f;

        Recording.Add(rb.position);
    }

    bool CheckGrounded()
    {
        var hits = Physics2D.OverlapBoxAll(rb.position + Vector2.down * 0.50f, new Vector2(0.66f, 0.12f), 0f);
        foreach (var h in hits)
        {
            if (h.isTrigger) continue;
            if (h.attachedRigidbody == rb) continue;
            return true;
        }
        return false;
    }
}
