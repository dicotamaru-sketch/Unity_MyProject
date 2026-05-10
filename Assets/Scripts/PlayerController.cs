using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float jumpForce = 16f;
    public float stompBounce = 6f;

    [Header("地面判定")]
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("射撃設定")]
    public float shootCooldown = 0.3f;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool isGrounded;
    private float shootTimer = 0f;
    private float facingDir = 1f;
    private bool isDead = false;

    // 向き目印オブジェクト
    private GameObject directionIndicator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 1.8f; // jumpForce=16, gravity=1.8 → 最大高さ≒7.3ユニット

        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Ground");

        CreateDirectionIndicator();
    }

    void CreateDirectionIndicator()
    {
        // ピクセルアートプレイヤーは顔で向きが分かるので目印は小さめに
        directionIndicator = new GameObject("DirectionIndicator");
        directionIndicator.transform.SetParent(transform);
        directionIndicator.transform.localPosition = new Vector3(0.4f, 0.15f, -0.1f);
        directionIndicator.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

        SpriteRenderer sr = directionIndicator.AddComponent<SpriteRenderer>();
        sr.sprite = CreateTriangleSprite();
        sr.color = new Color(1f, 1f, 0f, 0.8f);
        sr.sortingOrder = 3;
    }

    Sprite CreateTriangleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        // 右向き三角形を描く
        for (int y = 0; y < size; y++)
        {
            float t = (float)y / size;
            int xStart = (int)(size * 0.1f);
            int xEnd = (int)(size * (0.1f + 0.8f * (1f - Mathf.Abs(t - 0.5f) * 2f)));
            for (int x = xStart; x < xEnd; x++)
                pixels[y * size + x] = Color.white;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0f, 0.5f), 32f);
    }

    Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                pixels[y * size + x] = Vector2.Distance(new Vector2(x, y), center) <= radius
                    ? Color.white : Color.clear;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    void Update()
    {
        if (isDead) return;

        transform.rotation = Quaternion.identity;

        // 地面判定（BoxCast）
        if (boxCollider != null)
        {
            Vector2 size = new Vector2(boxCollider.bounds.size.x * 0.9f, 0.05f);
            Vector2 origin = new Vector2(transform.position.x, boxCollider.bounds.min.y);
            isGrounded = Physics2D.BoxCast(origin, size, 0f, Vector2.down, groundCheckRadius, groundLayer);
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 移動
        float moveInput = 0f;
        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed)
            moveInput = -1f;
        if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed)
            moveInput = 1f;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput != 0f)
        {
            facingDir = moveInput;
            transform.localScale = new Vector3(facingDir, 1, 1);
        }

        // ジャンプ
        if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        // 射撃（Zキー）
        shootTimer -= Time.deltaTime;
        if (keyboard.zKey.wasPressedThisFrame && shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootCooldown;
        }

        // 落下死亡判定
        if (transform.position.y < -10f)
            Die();
    }

    void Shoot()
    {
        GameObject bulletObj = new GameObject("Bullet");
        bulletObj.transform.position = transform.position + new Vector3(facingDir * 0.6f, 0, 0);
        bulletObj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        SpriteRenderer sr = bulletObj.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.CreateBullet();
        sr.sortingOrder = 2;

        CircleCollider2D col = bulletObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.Launch(facingDir);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (isDead) return;

        EnemyController enemy = col.gameObject.GetComponent<EnemyController>();
        if (enemy != null)
        {
            HandleEnemyContact(enemy);
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (isDead) return;

        EnemyController enemy = col.gameObject.GetComponent<EnemyController>();
        if (enemy != null)
        {
            HandleEnemyContact(enemy);
        }
    }

    void HandleEnemyContact(EnemyController enemy)
    {
        if (enemy == null || isDead) return;

        // プレイヤーの足元Y vs 敵の中心Y
        // 足元が敵中心より上にある = 上から踏んでいる
        float playerBottom = transform.position.y - 0.5f;
        float enemyMid     = enemy.transform.position.y;

        if (playerBottom >= enemyMid)
        {
            // 上から踏む
            enemy.GetStomped();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, stompBounce);
        }
        else
        {
            // 横・下から当たる → やられ
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        rb.linearVelocity = new Vector2(0, 8f);
        rb.gravityScale = 2f;
        GetComponent<SpriteRenderer>().color = new Color(1f, 0.3f, 0.3f, 0.5f);

        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        if (gm != null) gm.PlayerDied();
    }
}
