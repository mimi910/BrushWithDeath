using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerKeyUI : MonoBehaviour
{
    private static PlayerKeyUI instance;

    [Header("Layout")]
    [SerializeField] private Vector2 iconSize = new(48f, 48f);
    [SerializeField] private Vector2 iconOffset = new(18f, -18f);

    [Header("Visuals")]
    [SerializeField] private Sprite keySprite;

    private PlayerProgression progression;
    [System.NonSerialized] private RectTransform iconRoot;
    [System.NonSerialized] private Image keyIcon;

    public static PlayerKeyUI Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<PlayerKeyUI>();

            return instance;
        }
    }

    public static PlayerKeyUI EnsureInstance()
    {
        if (Instance != null)
            return instance;

        GameObject root = new("PlayerKeyUI", typeof(RectTransform));
        instance = root.AddComponent<PlayerKeyUI>();
        instance.EnsureRuntimeSetup();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        EnsureRuntimeSetup();
    }

    private void Start()
    {
        if (progression == null)
            Bind(FindAnyObjectByType<PlayerProgression>());
        else
            Refresh(progression.HasKey);
    }

    private void OnDestroy()
    {
        if (progression != null)
            progression.KeyStateChanged -= HandleKeyStateChanged;

        if (instance == this)
            instance = null;
    }

    public void Bind(PlayerProgression targetProgression)
    {
        if (progression == targetProgression)
        {
            Refresh(progression != null && progression.HasKey);
            return;
        }

        if (progression != null)
            progression.KeyStateChanged -= HandleKeyStateChanged;

        progression = targetProgression;

        if (progression != null)
            progression.KeyStateChanged += HandleKeyStateChanged;

        EnsureRuntimeSetup();
        Refresh(progression != null && progression.HasKey);
    }

    private void EnsureRuntimeSetup()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 50;

        CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
        if (canvasScaler == null)
            canvasScaler = gameObject.AddComponent<CanvasScaler>();

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        if (iconRoot == null)
            iconRoot = CreateIconRoot();

        if (keyIcon == null)
            keyIcon = CreateIcon(iconRoot);

        if (keyIcon.sprite != keySprite)
            keyIcon.sprite = keySprite;
    }

    private RectTransform CreateIconRoot()
    {
        GameObject iconObject = new("KeyIcon", typeof(RectTransform));
        RectTransform rectTransform = iconObject.GetComponent<RectTransform>();
        rectTransform.SetParent(transform, false);
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = iconOffset;
        rectTransform.sizeDelta = iconSize;

        return rectTransform;
    }

    private static Image CreateIcon(RectTransform parent)
    {
        GameObject iconObject = new("Image", typeof(RectTransform), typeof(Image));
        RectTransform rectTransform = iconObject.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = iconObject.GetComponent<Image>();
        image.raycastTarget = false;
        image.color = Color.white;
        image.preserveAspect = true;
        return image;
    }

    private void HandleKeyStateChanged(bool hasKey)
    {
        Refresh(hasKey);
    }

    private void Refresh(bool hasKey)
    {
        EnsureRuntimeSetup();

        if (keyIcon != null)
            keyIcon.enabled = hasKey && keySprite != null;
    }
}
