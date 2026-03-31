using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputReader : MonoBehaviour
{
    [SerializeField] private float pistaDoubleTapWindow = 0.3f;

    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction interactAction;
    private InputAction lanternAction;
    private InputAction guitarAction;
    private InputAction tempoAction;
    private InputAction tempoIntenseAction;
    private InputAction tempoSlowAction;
    private InputAction tempoMidAction;
    private InputAction tempoFastAction;
    private InputAction pistaAction;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LastMoveInput { get; private set; } = Vector2.down;

    public bool InteractPressed { get; private set; }
    public bool LanternPressed { get; private set; }
    public bool GuitarPressed { get; private set; }
    public bool TempoPressed { get; private set; }
    public bool TempoHeld { get; private set; }
    public bool TempoIntensePressed { get; private set; }
    public bool TempoSlowPressed { get; private set; }
    public bool TempoMidPressed { get; private set; }
    public bool TempoFastPressed { get; private set; }
    public bool TempoIntenseHeld { get; private set; }
    public bool TempoSlowHeld { get; private set; }
    public bool TempoMidHeld { get; private set; }
    public bool TempoFastHeld { get; private set; }
    public bool PistaPressed { get; private set; }
    public bool PistaHeld { get; private set; }
    public bool PistaRecallPressed { get; private set; }

    private float lastPistaPressTime = float.NegativeInfinity;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        interactAction = playerInput.actions["Interact"];
        lanternAction = playerInput.actions["Lantern"];
        guitarAction = playerInput.actions["Guitar"];
        tempoAction = playerInput.actions["Tempo"];
        tempoIntenseAction = playerInput.actions["TempoIntense"];
        tempoSlowAction = playerInput.actions["TempoSlow"];
        tempoMidAction = playerInput.actions["TempoMid"];
        tempoFastAction = playerInput.actions["TempoFast"];
        pistaAction = playerInput.actions["Pista"];
    }

    private void Update()
    {
        ReadMovement();
        ReadButtons();
    }

    private void ReadMovement()
    {
        MoveInput = moveAction.ReadValue<Vector2>();

        if (MoveInput.sqrMagnitude > 1f)
            MoveInput = MoveInput.normalized;

        if (MoveInput.sqrMagnitude > 0.01f)
            LastMoveInput = MoveInput.normalized;
    }

    private void ReadButtons()
    {
        InteractPressed = interactAction.WasPressedThisFrame();
        LanternPressed = lanternAction.WasPressedThisFrame();
        GuitarPressed = guitarAction.WasPressedThisFrame();
        TempoPressed = tempoAction.WasPressedThisFrame();
        TempoHeld = tempoAction.IsPressed();
        TempoIntensePressed = tempoIntenseAction.WasPressedThisFrame();
        TempoSlowPressed = tempoSlowAction.WasPressedThisFrame();
        TempoMidPressed = tempoMidAction.WasPressedThisFrame();
        TempoFastPressed = tempoFastAction.WasPressedThisFrame();
        TempoIntenseHeld = tempoIntenseAction.IsPressed();
        TempoSlowHeld = tempoSlowAction.IsPressed();
        TempoMidHeld = tempoMidAction.IsPressed();
        TempoFastHeld = tempoFastAction.IsPressed();
        PistaPressed = pistaAction.WasPressedThisFrame();
        PistaHeld = pistaAction.IsPressed();
        PistaRecallPressed = false;

        if (PistaPressed)
        {
            float currentTime = Time.time;

            if (currentTime - lastPistaPressTime <= pistaDoubleTapWindow)
                PistaRecallPressed = true;

            lastPistaPressTime = currentTime;
        }
    }

    public void ClearFrameButtons()
    {
        InteractPressed = false;
        LanternPressed = false;
        GuitarPressed = false;
        TempoPressed = false;
        TempoIntensePressed = false;
        TempoSlowPressed = false;
        TempoMidPressed = false;
        TempoFastPressed = false;
        PistaPressed = false;
        PistaRecallPressed = false;
    }

    public bool TryGetTempoSelectionHeld(out TempoBand tempoBand)
    {
        int heldCount = 0;
        tempoBand = TempoBand.Mid;

        if (TempoIntenseHeld)
        {
            tempoBand = TempoBand.Intense;
            heldCount++;
        }

        if (TempoSlowHeld)
        {
            tempoBand = TempoBand.Slow;
            heldCount++;
        }

        if (TempoMidHeld)
        {
            tempoBand = TempoBand.Mid;
            heldCount++;
        }

        if (TempoFastHeld)
        {
            tempoBand = TempoBand.Fast;
            heldCount++;
        }

        return heldCount == 1;
    }
}
