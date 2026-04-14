using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DialogueBoxUI : MonoBehaviour
{
    private static DialogueBoxUI instance;

    [Header("References")]
    [SerializeField] private RectTransform dialogueRoot;
    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Portrait Defaults")]
    [SerializeField] private Sprite defaultSignPortrait;
    [SerializeField] private Sprite defaultPistaPortrait;

    [Header("Timing")]
    [SerializeField, Min(0.5f)] private float defaultDisplayDuration = 3.5f;

    private Coroutine hideRoutine;

    public static DialogueBoxUI Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<DialogueBoxUI>();

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        CacheReferences();
        HideImmediate();
    }

    private void OnValidate()
    {
        CacheReferences();
    }

    public void ShowSign(string message, Sprite portraitOverride = null, float duration = -1f)
    {
        Show(message, portraitOverride != null ? portraitOverride : GetDefaultSignPortrait(), duration);
    }

    public void ShowPista(string message, Sprite portraitOverride = null, float duration = -1f)
    {
        Show(message, portraitOverride != null ? portraitOverride : GetDefaultPistaPortrait(), duration);
    }

    public Sprite GetDefaultSignPortrait()
    {
        return defaultSignPortrait;
    }

    public Sprite GetDefaultPistaPortrait()
    {
        if (defaultPistaPortrait != null)
            return defaultPistaPortrait;

        PistaController pista = FindAnyObjectByType<PistaController>();
        if (pista == null)
            return null;

        SpriteRenderer spriteRenderer = pista.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = pista.GetComponentInChildren<SpriteRenderer>();

        return spriteRenderer != null ? spriteRenderer.sprite : null;
    }

    public void HideImmediate()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        SetVisible(false);
    }

    private void Show(string message, Sprite portrait, float duration)
    {
        CacheReferences();

        if (dialogueRoot == null)
        {
            Debug.LogWarning("DialogueBoxUI is missing a dialogue root reference.", this);
            return;
        }

        if (!TrySetDialogueText(message))
        {
            Debug.LogWarning("DialogueBoxUI is missing a TextMeshProUGUI reference.", this);
            return;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
            portraitImage.preserveAspect = true;
        }

        SetVisible(true);

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        float resolvedDuration = duration > 0f ? duration : defaultDisplayDuration;
        hideRoutine = StartCoroutine(HideAfterSeconds(resolvedDuration));
    }

    private IEnumerator HideAfterSeconds(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        HideImmediate();
    }

    private void CacheReferences()
    {
        if (dialogueRoot == null)
            dialogueRoot = transform as RectTransform;

        if (dialogueRoot != null && dialogueCanvasGroup == null)
        {
            dialogueCanvasGroup = dialogueRoot.GetComponent<CanvasGroup>();
            if (dialogueCanvasGroup == null)
                dialogueCanvasGroup = dialogueRoot.gameObject.AddComponent<CanvasGroup>();
        }

        if (dialogueText == null)
            dialogueText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (portraitImage == null)
            portraitImage = GetComponentInChildren<Image>(true);
    }

    private bool TrySetDialogueText(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        if (dialogueText == null)
            return false;

        dialogueText.text = message;
        return true;
    }

    private void SetVisible(bool isVisible)
    {
        if (dialogueCanvasGroup == null)
            return;

        dialogueCanvasGroup.alpha = isVisible ? 1f : 0f;
        dialogueCanvasGroup.interactable = isVisible;
        dialogueCanvasGroup.blocksRaycasts = isVisible;
    }
}
