using UnityEngine;

public class DontDestroyOnLoadAndChangeScene : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Invoke(nameof(LoadMenuScene), 7f);
    }

    void LoadMenuScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}
