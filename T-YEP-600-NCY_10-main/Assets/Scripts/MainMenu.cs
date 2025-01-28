using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void OnStartSimulationButtonClicked()
    {
        SceneManager.LoadScene("PreGameScene"); 
    }

    public void CreditButtonClicked()
    {
        SceneManager.LoadScene("CreditScene"); 
    }

    public void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
