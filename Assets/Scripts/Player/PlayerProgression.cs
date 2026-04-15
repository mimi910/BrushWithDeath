using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerProgression : MonoBehaviour
{
    private bool hasKey;

    public event Action<bool> KeyStateChanged;

    public bool HasKey => hasKey;

    private void Awake()
    {
        PlayerKeyUI.EnsureInstance()?.Bind(this);
        NotifyKeyStateChanged();
    }

    public void CollectKey()
    {
        if (hasKey)
            return;

        hasKey = true;
        NotifyKeyStateChanged();
    }

    public bool ConsumeKey()
    {
        if (!hasKey)
            return false;

        hasKey = false;
        NotifyKeyStateChanged();
        return true;
    }

    private void NotifyKeyStateChanged()
    {
        KeyStateChanged?.Invoke(hasKey);
    }
}
