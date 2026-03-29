using System;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleEventEmitter : MonoBehaviour
{
    public enum EmitCommand
    {
        Activate,
        Deactivate,
        Toggle,
        MatchState
    }

    [Serializable]
    private struct ReceiverBinding
    {
        public PuzzleEventReceiver receiver;
        public EmitCommand command;
    }

    [Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    [SerializeField] private PuzzleStateBool stateSource;
    [SerializeField] private ReceiverBinding[] receivers;
    [SerializeField] private UnityEvent onEmitActivated;
    [SerializeField] private UnityEvent onEmitDeactivated;
    [SerializeField] private BoolEvent onEmitState;

    private void Awake()
    {
        if (stateSource == null)
            stateSource = GetComponent<PuzzleStateBool>();
    }

    public void EmitActivate()
    {
        EmitCommandToReceivers(EmitCommand.Activate, true);
    }

    public void EmitDeactivate()
    {
        EmitCommandToReceivers(EmitCommand.Deactivate, false);
    }

    public void EmitToggle()
    {
        foreach (ReceiverBinding binding in receivers)
        {
            if (binding.receiver == null)
                continue;

            binding.receiver.ReceiveToggle();
        }
    }

    public void EmitCurrentState()
    {
        if (stateSource == null)
            return;

        EmitSetState(stateSource.Value);
    }

    public void EmitSetState(bool isOn)
    {
        EmitCommandToReceivers(EmitCommand.MatchState, isOn);
    }

    private void EmitCommandToReceivers(EmitCommand defaultCommand, bool stateValue)
    {
        foreach (ReceiverBinding binding in receivers)
        {
            if (binding.receiver == null)
                continue;

            EmitCommand command = binding.command == EmitCommand.MatchState ? defaultCommand : binding.command;

            switch (command)
            {
                case EmitCommand.Activate:
                    binding.receiver.ReceiveActivate();
                    break;
                case EmitCommand.Deactivate:
                    binding.receiver.ReceiveDeactivate();
                    break;
                case EmitCommand.Toggle:
                    binding.receiver.ReceiveToggle();
                    break;
                case EmitCommand.MatchState:
                    binding.receiver.ReceiveSetState(stateValue);
                    break;
            }
        }

        onEmitState?.Invoke(stateValue);

        if (stateValue)
            onEmitActivated?.Invoke();
        else
            onEmitDeactivated?.Invoke();
    }
}
