using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditMenu : MonoBehaviour
{
    public void BackButtonClicked()
    {
       SceneManager.LoadScene("MainMenuScene");
    }
}
