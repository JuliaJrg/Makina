using UnityEngine;
using TMPro;

public class NpcInfoDisplay : MonoBehaviour
{
    private Npc npc;
    public TextMeshProUGUI infoText;

    void Update()
    {
        if (npc != null)
        {
            infoText.text = 
                "Plantes mangées : " + npc.PlantsEaten + "\n" + 
                "Âge : " + npc.Age + " jours\n" + 
                "Égoïsme : " + npc.egoisme + "\n" + 
                "Altruisme : " + npc.altruisme + "\n" + 
                "Pacifisme : " + npc.pacifisme;
        }
    }

    public void SetNpc(Npc npc)
    {
        this.npc = npc;
    }
}