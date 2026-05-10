using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public bool isFlying = false;       // 上空浮遊タイプか
    public float flyHeight = 2f;        // 浮遊Y座標
    public float flyAmplitude = 1.5f;   // 上下の揺れ幅
    public float flySpeed = 2f;         // 上下の揺れ速度

    private Rigidbody2D rb;
    private bool movingLeft = true;
    private float directionCooldown = 0f;
    private float startY;
    private float timeOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startY = transform.position.y;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);

        if (isFlying)
        {
            rb.gravityScale = 0f; // 浮遊敵は重力なし
        }
    }

    void Update()
    {
        if (directionCooldown > 0f)
            directionCooldown -= Time.deltaTime;

        if (isFlying)
        {
            // 上下に揺れながら横移動
            float dir = movingLeft ? -1f : 1f;
            float newY = startY + Mathf.Sin(Time.time * flySpeed + timeOffset) * flyAmplitude;
            transform.position = new Vector3(transform.position.x + dir * moveSpeed * Time.deltaTime, newY, 0f);
            transform.localScale = new Vector3(movingLeft ? 1 : -1, 1, 1);
        }
        else
        {
            float dir = movingLeft ? -1f : 1f;
            rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(movingLeft ? 1 : -1, 1, 1);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (isFlying) return;

        if (col.gameObject.CompareTag("Ground") ||
            col.gameObject.CompareTag("Block") ||
            col.gameObject.GetComponent<EnemyController>() != null)
        {
            foreach (ContactPoint2D contact in col.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.5f && directionCooldown <= 0f)
                {
                    movingLeft = !movingLeft;
                    directionCooldown = 0.3f;
                    break;
                }
            }
        }

        // 敵側のダメージ判定はPlayerController側に一本化するため何もしない
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // 浮遊敵がプレイヤーに触れた
        if (isFlying && col.CompareTag("Player"))
        {
            GameManager gm = Object.FindAnyObjectByType<GameManager>();
            if (gm != null) gm.PlayerDied();
        }
    }

    // 踏まれたとき・弾に当たったとき
    public void GetStomped()
    {
        // 弾けて消えるアニメーション代わりにスケールを変えてから削除
        rb.linearVelocity = new Vector2(Random.Range(-3f, 3f), 5f);
        rb.gravityScale = 2f;
        // コライダー無効化（二重判定防止）
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;
        Destroy(gameObject, 0.5f);
    }

    public void FoldRight()
    {
        movingLeft = false;
        directionCooldown = 0.3f;
    }
    public void FoldLeft()
    {
        movingLeft = true;
        directionCooldown = 0.3f;
    }
}
