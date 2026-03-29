using UnityEngine;
using UnityEngine.Events;

public class LightableTorch : MonoBehaviour, ILightable
{
    public enum TorchType
    {
        Standard,
        Marigold,
        Flamethrower
    }

    [SerializeField] private TorchType torchType = TorchType.Standard;
    [SerializeField] private PuzzleStateBool stateSource;
    [SerializeField] private PuzzleStateVisuals stateVisuals;
    [SerializeField] private PuzzleEventEmitter eventEmitter;
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private bool startsLit;
    [SerializeField] private bool toggleOnLight = true;
    [SerializeField] private Color unlitColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color litColor = new Color(1f, 0.6f, 0.15f, 1f);
    [SerializeField] private UnityEvent onLit;
    [SerializeField] private UnityEvent onExtinguished;

    public TorchType Type => torchType;
    public bool IsLit => stateSource != null ? stateSource.Value : startsLit;

    private void Awake()
    {
        if (stateSource == null)
            stateSource = GetComponent<PuzzleStateBool>();

        if (stateVisuals == null)
            stateVisuals = GetComponent<PuzzleStateVisuals>();

        if (eventEmitter == null)
            eventEmitter = GetComponent<PuzzleEventEmitter>();

        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (stateSource != null)
        {
            stateSource.SetState(startsLit);
            ApplyVisuals(stateSource.Value);
            return;
        }

        ApplyVisuals(startsLit);
    }

    public void Light(PlayerController player)
    {
        bool nextState = toggleOnLight ? !IsLit : true;
        ApplyLitState(nextState, true);
        Debug.Log($"{name} toggled {(IsLit ? "lit" : "unlit")} by lantern from {player.name}.", this);
    }

    public void SetLit(bool isLit)
    {
        ApplyLitState(isLit, false);
    }

    private void ApplyLitState(bool isLit, bool emitEvents)
    {
        bool previousState = IsLit;

        if (stateSource != null)
            stateSource.SetState(isLit);
        else
            startsLit = isLit;

        ApplyVisuals(isLit);

        if (eventEmitter != null)
            eventEmitter.EmitSetState(isLit);

        if (!emitEvents || previousState == isLit)
            return;

        if (isLit)
            onLit?.Invoke();
        else
            onExtinguished?.Invoke();
    }

    private void ApplyVisuals(bool isLit)
    {
        if (targetRenderer != null)
            targetRenderer.color = isLit ? litColor : unlitColor;

        stateVisuals?.Apply(isLit);
    }
}
