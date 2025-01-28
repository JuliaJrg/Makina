using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIGame : MonoBehaviour
{
    public TextMeshProUGUI populationText;
    // public TextMeshProUGUI npcInfoText; // Ajouté pour afficher les infos du NPC sélectionné
    public TMP_FontAsset customFont;

    void Start()
    {
        if (customFont != null)
        {
            populationText.font = customFont;
            populationText.color = Color.black;

            // npcInfoText.font = customFont; // Configurer la police pour npcInfoText
            // npcInfoText.color = Color.black;
        }
    }

    void Update()
    {
        UpdatePopulationInfo();
    }

    void UpdatePopulationInfo()
    {
        if (populationText != null)
        {
            int time = (int)Time.time;
            int populationPlant = Environment.GetPopulation(Species.Plant);
            int populationNpc1 = Environment.GetPopulation(Species.Npc1);
            int populationNpc2 = Environment.GetPopulation(Species.Npc2);
            int day = TimeManager.GetCurrentDay();
            if (populationNpc2 != 0)
            {
                populationText.text =
                "Temps : " + time +
                "s\nPopulation NPC1 : " + populationNpc1 +
                "\nPopulation NPC2 : " + populationNpc2 +
                "\nPopulation Plante : " + populationPlant +
                "\nJour : " + day;
            }
            else
            {
                populationText.text =
                "Temps : " + time +
                "s\nPopulation NPC : " + populationNpc1 +
                "\nPopulation Plante : " + populationPlant +
                "\nJour : " + day;
            }
        }
        else
        {
            Debug.LogWarning("UIGame: populationText is null in Update.");
        }
    }

}
