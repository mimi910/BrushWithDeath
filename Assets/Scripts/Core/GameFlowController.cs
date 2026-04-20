using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class GameFlowController : MonoBehaviour
{
    private static GameFlowController instance;

    [SerializeField] private UnityEvent onGameOver;

    public static GameFlowController Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<GameFlowController>();

            return instance;
        }
    }

    public bool IsGameOver { get; private set; }

    public static GameFlowController EnsureInstance()
    {
        if (Instance != null)
            return instance;

        GameObject root = new("GameFlowController");
        instance = root.AddComponent<GameFlowController>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void HandleGameOver()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;
        onGameOver?.Invoke();

        Debug.Log("Game over triggered. TODO: Replace GameFlowController.HandleGameOver() stub with the real game-over flow.", this);
    }
}
