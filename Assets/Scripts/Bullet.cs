using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 3f;
    private float direction = 1f;

    public void Launch(float dir)
    {
        direction = dir;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        EnemyController enemy = col.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.GetStomped();
            Destroy(gameObject);
            return;
        }

        // 地面やブロックに当たったら消える
        if (col.CompareTag("Ground") || col.CompareTag("Block"))
        {
            Destroy(gameObject);
        }
    }
}
