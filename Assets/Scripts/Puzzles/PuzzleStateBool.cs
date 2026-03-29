using System;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleStateBool : MonoBehaviour
{
    [Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    [SerializeField] private bool startsOn;
    [SerializeField] private bool currentValue;
    [SerializeField] private UnityEvent onActivated;
    [SerializeField] private UnityEvent onDeactivated;
    [SerializeField] private BoolEvent onValueChanged;

    public bool Value => currentValue;
    public event Action<bool> ValueChanged;

    private void Awake()
    {
        currentValue = startsOn;
    }

    public void SetState(bool isOn)
    {
        if (currentValue == isOn)
            return;

        currentValue = isOn;
        ValueChanged?.Invoke(currentValue);
        onValueChanged?.Invoke(currentValue);

        if (currentValue)
            onActivated?.Invoke();
        else
            onDeactivated?.Invoke();
    }

    public void SetOn()
    {
        SetState(true);
    }

    public void SetOff()
    {
        SetState(false);
    }

    public void Toggle()
    {
        SetState(!currentValue);
    }

    public void Refresh()
    {
        ValueChanged?.Invoke(currentValue);
        onValueChanged?.Invoke(currentValue);

        if (currentValue)
            onActivated?.Invoke();
        else
            onDeactivated?.Invoke();
    }
}
