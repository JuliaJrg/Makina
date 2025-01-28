using UnityEngine;
using UnityEngine.UI;

public class NpcSelector : MonoBehaviour
{
    public LayerMask npcLayerMask;
    public GameObject npcInfoPrefab; // Prefab pour afficher les informations du NPC
    private GameObject currentNpcInfo;
    private Npc currentNpc;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectNpc();
        }

        if (currentNpcInfo != null && currentNpc != null)
        {
            UpdateNpcInfoPosition();
        }
    }

    void SelectNpc()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, npcLayerMask))
        {
            Npc npc = hit.collider.GetComponent<Npc>();
            if (npc != null)
            {
                DisplayNpcInfo(npc);
            }
        }
    }

    void DisplayNpcInfo(Npc npc)
    {
        if (currentNpcInfo != null)
        {
            Destroy(currentNpcInfo);
        }

        currentNpc = npc;
        currentNpcInfo = Instantiate(npcInfoPrefab, npc.transform.position + Vector3.up * 2, Quaternion.identity);
        currentNpcInfo.GetComponent<NpcInfoDisplay>().SetNpc(npc);
    }

    void UpdateNpcInfoPosition()
    {
        if (currentNpc != null)
        {
            currentNpcInfo.transform.position = currentNpc.transform.position + Vector3.up * 2;
        }
    }
}