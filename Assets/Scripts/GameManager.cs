using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int coins = 0;
    private bool isDead = false;

    void Awake()
    {
        Instance = this;
    }

    public void AddCoin()
    {
        coins++;
        Debug.Log("コイン: " + coins);
    }

    public void PlayerDied()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("やられた！3秒後にリスタート");
        Invoke("Restart", 3f);
    }

    public void StageClear()
    {
        Debug.Log("ステージクリア！コイン数: " + coins);
        Invoke("Restart", 3f);
    }

    void Restart()
    {
        isDead = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
