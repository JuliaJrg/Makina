using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    public int numberOfPlants;
    public int numberOfNPC1;
    public int numberOfNPC2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
