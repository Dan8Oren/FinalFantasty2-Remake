using UnityEngine;

public class RedBlockScript : MonoBehaviour
{
    [SerializeField] private int levelToUnlock;

    private void Start()
    {
        if (GameManager.Instance.CompletedFightLevels.Contains(levelToUnlock)) Destroy(gameObject);
    }
}