using UnityEngine;

[DisallowMultipleComponent]
public class SignDialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField, TextArea(2, 6)] private string dialogueText = "A weathered sign creaks in the wind.";
    [SerializeField] private Sprite portraitOverride;
    [SerializeField] private bool useSpriteRendererPortrait = true;
    [SerializeField, Min(0.5f)] private float displayDuration = 3.5f;

    public void Interact(PlayerController player)
    {
        if (string.IsNullOrWhiteSpace(dialogueText))
            return;

        DialogueBoxUI dialogueBox = DialogueBoxUI.Instance;
        dialogueBox.ShowSign(dialogueText, ResolvePortrait(dialogueBox), displayDuration);
    }

    private Sprite ResolvePortrait(DialogueBoxUI dialogueBox)
    {
        if (portraitOverride != null)
            return portraitOverride;

        if (useSpriteRendererPortrait)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null && spriteRenderer.sprite != null)
                return spriteRenderer.sprite;
        }

        return dialogueBox.GetDefaultSignPortrait();
    }
}
