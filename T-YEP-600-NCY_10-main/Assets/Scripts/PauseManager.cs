using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        GameObject eventSystem = GameObject.Find("EventSystem");
        if (eventSystem != null)
        {
            eventSystem.SetActive(true);
        }

        SceneManager.UnloadSceneAsync("PauseMenuScene");
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        GameObject eventSystem = GameObject.Find("EventSystem");
        if (eventSystem != null)
        {
            eventSystem.SetActive(false);
        }

        SceneManager.LoadScene("PauseMenuScene", LoadSceneMode.Additive);
    }

    public void LoadPreGameScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("PreGameScene");
    }
}