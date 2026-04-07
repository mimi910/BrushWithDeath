using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OrderedSwitchPuzzle : MonoBehaviour
{
    [Header("Switch Order")]
    [SerializeField] private InteractSwitch[] switchOrder;
    [SerializeField] private bool resetAssignedSwitchesOnAwake = true;
    [SerializeField] private bool lockAfterSolved = true;

    [Header("Completion")]
    [SerializeField] private PuzzleStateBool completionState;
    [SerializeField] private PuzzleEventEmitter completionEmitter;
    [SerializeField] private UnityEvent onSolved;
    [SerializeField] private UnityEvent onReset;

    private int nextExpectedIndex;
    private bool isSolved;

    private void Awake()
    {
        if (completionState == null)
            completionState = GetComponent<PuzzleStateBool>();

        if (completionEmitter == null)
            completionEmitter = GetComponent<PuzzleEventEmitter>();

        if (resetAssignedSwitchesOnAwake)
            ResetPuzzle(false);
    }

    private void OnEnable()
    {
        SubscribeToSwitches();
    }

    private void OnDisable()
    {
        UnsubscribeFromSwitches();
    }

    public void ResetPuzzle()
    {
        ResetPuzzle(true);
    }

    private void HandleSwitchActivated(InteractSwitch activatedSwitch, bool resultingState)
    {
        if (activatedSwitch == null)
            return;

        if (switchOrder == null || switchOrder.Length == 0)
            return;

        if (isSolved && lockAfterSolved)
            return;

        int activatedIndex = GetSwitchIndex(activatedSwitch);
        if (activatedIndex < 0)
            return;

        if (!resultingState || activatedIndex != nextExpectedIndex)
        {
            ResetPuzzle(true);
            return;
        }

        nextExpectedIndex++;

        if (nextExpectedIndex >= switchOrder.Length)
            SolvePuzzle();
    }

    private void SolvePuzzle()
    {
        isSolved = true;
        nextExpectedIndex = switchOrder != null ? switchOrder.Length : 0;
        SetCompletionState(true);
        onSolved?.Invoke();
    }

    private void ResetPuzzle(bool invokeResetEvent)
    {
        isSolved = false;
        nextExpectedIndex = 0;
        SetCompletionState(false);
        ResetAssignedSwitches();

        if (invokeResetEvent)
            onReset?.Invoke();
    }

    private void SetCompletionState(bool isComplete)
    {
        completionState?.SetState(isComplete);
        completionEmitter?.EmitSetState(isComplete);
    }

    private void ResetAssignedSwitches()
    {
        if (switchOrder == null)
            return;

        HashSet<int> resetSwitches = new();

        foreach (InteractSwitch puzzleSwitch in switchOrder)
        {
            if (puzzleSwitch == null)
                continue;

            int instanceId = puzzleSwitch.GetInstanceID();
            if (!resetSwitches.Add(instanceId))
                continue;

            puzzleSwitch.SetOff();
        }
    }

    private void SubscribeToSwitches()
    {
        if (switchOrder == null)
            return;

        HashSet<int> subscribedSwitches = new();

        foreach (InteractSwitch puzzleSwitch in switchOrder)
        {
            if (puzzleSwitch == null)
                continue;

            int instanceId = puzzleSwitch.GetInstanceID();
            if (!subscribedSwitches.Add(instanceId))
                continue;

            puzzleSwitch.Activated -= HandleSwitchActivated;
            puzzleSwitch.Activated += HandleSwitchActivated;
        }
    }

    private void UnsubscribeFromSwitches()
    {
        if (switchOrder == null)
            return;

        HashSet<int> unsubscribedSwitches = new();

        foreach (InteractSwitch puzzleSwitch in switchOrder)
        {
            if (puzzleSwitch == null)
                continue;

            int instanceId = puzzleSwitch.GetInstanceID();
            if (!unsubscribedSwitches.Add(instanceId))
                continue;

            puzzleSwitch.Activated -= HandleSwitchActivated;
        }
    }

    private int GetSwitchIndex(InteractSwitch targetSwitch)
    {
        if (targetSwitch == null || switchOrder == null)
            return -1;

        for (int i = 0; i < switchOrder.Length; i++)
        {
            if (switchOrder[i] == targetSwitch)
                return i;
        }

        return -1;
    }
}
