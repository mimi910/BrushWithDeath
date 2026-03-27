using UnityEngine;
using UnityEngine.Events;

public class TestTorch : MonoBehaviour, ILightable
{
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private bool startsLit;
    [SerializeField] private Color unlitColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color litColor = new Color(1f, 0.6f, 0.15f, 1f);
    [SerializeField] private UnityEvent onLit;
    [SerializeField] private UnityEvent onExtinguished;

    public bool IsLit { get; private set; }

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

        ApplyLitState(startsLit, false);
    }

    public void Light(PlayerController player)
    {
        ApplyLitState(!IsLit, true);
        Debug.Log($"Test torch toggled {(IsLit ? "lit" : "unlit")} by lantern from {player.name}.", this);
    }

    private void ApplyColor()
    {
        if (targetRenderer == null)
            return;

        targetRenderer.color = IsLit ? litColor : unlitColor;
    }

    private void ApplyLitState(bool isLit, bool invokeEvents)
    {
        if (IsLit == isLit && targetRenderer != null)
        {
            ApplyColor();
            return;
        }

        bool wasLit = IsLit;
        IsLit = isLit;
        ApplyColor();

        if (!invokeEvents || wasLit == IsLit)
            return;

        if (IsLit)
            onLit?.Invoke();
        else
            onExtinguished?.Invoke();
    }
}
