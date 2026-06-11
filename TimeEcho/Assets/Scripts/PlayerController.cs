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
    Transform eyes;
    float inputX;
    float jumpBufferLeft;
    float coyoteLeft;
    int facing = 1;
    bool wasGrounded;
    float stepTimer;
    float prevPosX;

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

        var body = new Color(1f, 0.71f, 0.28f);
        SpriteFactory.NewQuad("Body", go.transform, pos, new Vector2(0.8f, 0.95f), body, 10);

        var pc = go.AddComponent<PlayerController>();
        pc.eyes = new GameObject("Eyes").transform;
        pc.eyes.SetParent(go.transform, false);
        pc.eyes.localPosition = new Vector3(0.10f, 0.18f, 0f);
        var eyeC = new Color(0.18f, 0.12f, 0.05f);
        SpriteFactory.NewQuad("EyeL", pc.eyes, pos, new Vector2(0.12f, 0.16f), eyeC, 11).transform.localPosition = new Vector3(-0.14f, 0.18f, 0f);
        SpriteFactory.NewQuad("EyeR", pc.eyes, pos, new Vector2(0.12f, 0.16f), eyeC, 11).transform.localPosition = new Vector3(0.14f, 0.18f, 0f);
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
        eyes.localPosition = new Vector3(0.10f * facing, 0.18f, 0f);
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
        bool reallyWalking = grounded && Mathf.Abs(inputX) > 0.01f && actualDx > 0.03f;
        if (reallyWalking)
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
