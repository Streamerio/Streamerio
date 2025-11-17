using UnityEngine;

public class PlayerSingleton : MonoBehaviour
{
    public static GameObject Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this.gameObject;
        }
        else if (Instance != this.gameObject)
        {
            Destroy(gameObject);
        }
    }
}