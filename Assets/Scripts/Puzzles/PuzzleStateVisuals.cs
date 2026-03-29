using UnityEngine;

public class PuzzleStateVisuals : MonoBehaviour
{
    [SerializeField] private PuzzleStateBool stateSource;
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Color offColor = Color.white;
    [SerializeField] private Color onColor = Color.white;
    [SerializeField] private GameObject[] enableWhenOn;
    [SerializeField] private GameObject[] enableWhenOff;
    [SerializeField] private Collider2D[] collidersEnabledWhenOn;
    [SerializeField] private Collider2D[] collidersEnabledWhenOff;

    private void Awake()
    {
        if (stateSource == null)
            stateSource = GetComponent<PuzzleStateBool>();

        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (stateSource != null)
            stateSource.ValueChanged += Apply;

        ApplyCurrentState();
    }

    private void OnDisable()
    {
        if (stateSource != null)
            stateSource.ValueChanged -= Apply;
    }

    public void ApplyCurrentState()
    {
        if (stateSource == null)
            return;

        Apply(stateSource.Value);
    }

    public void Apply(bool isOn)
    {
        if (targetRenderer != null)
            targetRenderer.color = isOn ? onColor : offColor;

        SetActive(enableWhenOn, isOn);
        SetActive(enableWhenOff, !isOn);
        SetColliders(collidersEnabledWhenOn, isOn);
        SetColliders(collidersEnabledWhenOff, !isOn);
    }

    private static void SetActive(GameObject[] targets, bool value)
    {
        if (targets == null)
            return;

        foreach (GameObject target in targets)
        {
            if (target != null)
                target.SetActive(value);
        }
    }

    private static void SetColliders(Collider2D[] targets, bool value)
    {
        if (targets == null)
            return;

        foreach (Collider2D target in targets)
        {
            if (target != null)
                target.enabled = value;
        }
    }
}
