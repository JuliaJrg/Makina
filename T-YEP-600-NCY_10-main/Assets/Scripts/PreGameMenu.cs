using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PreGameMenu : MonoBehaviour
{
    public TMP_InputField PlantInputField;
    public TMP_InputField NPC1InputField;
    public TMP_InputField NPC2InputField;

    public void OnStartSimulationButtonClicked()
    {
        if (GameSettings.Instance == null)
        {
            return;
        }

        GameSettings.Instance.numberOfPlants = int.Parse(PlantInputField.text);
        GameSettings.Instance.numberOfNPC1 = int.Parse(NPC1InputField.text);
        GameSettings.Instance.numberOfNPC2 = int.Parse(NPC2InputField.text);

        SceneManager.LoadScene("SimulationScene");
    }

    public void BackButtonClicked()
    {
       SceneManager.LoadScene("MainMenuScene");
    }

}