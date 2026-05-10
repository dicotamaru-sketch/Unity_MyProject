using UnityEngine;

public class GoalFlag : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null) gm.StageClear();
        }
    }
}
