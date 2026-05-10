using UnityEngine;

public class Coin : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null) gm.AddCoin();
            Destroy(gameObject);
        }
    }
}
