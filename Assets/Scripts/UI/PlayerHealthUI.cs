using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Layout")]
    [SerializeField] private Vector2 heartSize = new(144f, 144f);
    [SerializeField] private Vector2 hudOffset = new(18f, -18f);
    [SerializeField, Min(0f)] private float heartSpacing = 24f;
    [SerializeField] private int canvasSortingOrder = 40;

    [Header("Visuals")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite hurtHeartSprite;
    [SerializeField] private Vector2 hurtHeartOffset = new(0f, -8f);

    private Canvas runtimeCanvas;
    private RectTransform heartsRoot;
    private Image[] hurtHeartImages = Array.Empty<Image>();
    private Image[] fullHeartImages = Array.Empty<Image>();
    private PlayerHealth boundHealth;

    private void Awake()
    {
        EnsureRuntimeSetup();
    }

    private void Start()
    {
        Bind(playerHealth != null ? playerHealth : FindAnyObjectByType<PlayerHealth>());
    }

    private void OnDestroy()
    {
        Unbind();

        if (runtimeCanvas != null)
            Destroy(runtimeCanvas.gameObject);
    }

    public void Bind(PlayerHealth targetHealth)
    {
        if (boundHealth == targetHealth)
        {
            Refresh(boundHealth != null ? boundHealth.CurrentHealth : 0f, boundHealth != null ? boundHealth.MaxHealth : 0f);
            return;
        }

        Unbind();
        boundHealth = targetHealth;
        playerHealth = targetHealth;

        if (boundHealth != null)
        {
            boundHealth.HealthChanged += HandleHealthChanged;
            Refresh(boundHealth.CurrentHealth, boundHealth.MaxHealth);
            return;
        }

        Refresh(0f, 0f);
    }

    private void Unbind()
    {
        if (boundHealth == null)
            return;

        boundHealth.HealthChanged -= HandleHealthChanged;
        boundHealth = null;
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        Refresh(currentHealth, maxHealth);
    }

    private void Refresh(float currentHealth, float maxHealth)
    {
        EnsureRuntimeSetup();
        EnsureHeartSlots(maxHealth);

        int heartCount = hurtHeartImages.Length;
        int visibleHeartCount = Mathf.Clamp(Mathf.CeilToInt(maxHealth), 0, heartCount);

        for (int i = 0; i < heartCount; i++)
        {
            bool isVisible = i < visibleHeartCount;
            bool hasHealthInSlot = currentHealth > i;

            ApplyHeartImageLayout(hurtHeartImages[i], hurtHeartOffset);
            ApplyHeartImageLayout(fullHeartImages[i], Vector2.zero);

            if (hurtHeartImages[i] != null)
            {
                hurtHeartImages[i].sprite = hurtHeartSprite;
                hurtHeartImages[i].enabled = isVisible && hurtHeartSprite != null && !hasHealthInSlot;
            }

            if (fullHeartImages[i] != null)
            {
                fullHeartImages[i].sprite = fullHeartSprite;
                fullHeartImages[i].enabled = isVisible && fullHeartSprite != null && hasHealthInSlot;
            }
        }
    }

    private void EnsureRuntimeSetup()
    {
        if (runtimeCanvas != null)
            return;

        GameObject root = new("PlayerHealthUI", typeof(RectTransform));
        runtimeCanvas = root.AddComponent<Canvas>();
        runtimeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        runtimeCanvas.overrideSorting = true;
        runtimeCanvas.sortingOrder = canvasSortingOrder;

        CanvasScaler canvasScaler = root.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        heartsRoot = CreateHeartsRoot(root.transform as RectTransform);
    }

    private RectTransform CreateHeartsRoot(RectTransform parent)
    {
        GameObject root = new("Hearts", typeof(RectTransform));
        RectTransform rectTransform = root.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = hudOffset;
        rectTransform.sizeDelta = heartSize;
        return rectTransform;
    }

    private void EnsureHeartSlots(float maxHealth)
    {
        int heartCount = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(1f, maxHealth)));

        if (heartsRoot == null)
            return;

        if (hurtHeartImages.Length == heartCount && fullHeartImages.Length == heartCount)
        {
            LayoutHeartSlots(heartCount);
            return;
        }

        ClearHeartSlots();
        hurtHeartImages = new Image[heartCount];
        fullHeartImages = new Image[heartCount];

        for (int i = 0; i < heartCount; i++)
        {
            GameObject slot = new($"Heart_{i + 1}", typeof(RectTransform));
            RectTransform slotTransform = slot.GetComponent<RectTransform>();
            slotTransform.SetParent(heartsRoot, false);
            slotTransform.anchorMin = new Vector2(0f, 1f);
            slotTransform.anchorMax = new Vector2(0f, 1f);
            slotTransform.pivot = new Vector2(0f, 1f);
            slotTransform.sizeDelta = heartSize;

            hurtHeartImages[i] = CreateHeartImage(slotTransform, "Hurt", hurtHeartSprite);
            fullHeartImages[i] = CreateHeartImage(slotTransform, "Full", fullHeartSprite);
        }

        LayoutHeartSlots(heartCount);
    }

    private void LayoutHeartSlots(int heartCount)
    {
        if (heartsRoot == null)
            return;

        float width = (heartSize.x * heartCount) + (heartSpacing * Mathf.Max(0, heartCount - 1));
        heartsRoot.anchoredPosition = hudOffset;
        heartsRoot.sizeDelta = new Vector2(width, heartSize.y);

        for (int i = 0; i < heartsRoot.childCount; i++)
        {
            RectTransform slotTransform = heartsRoot.GetChild(i) as RectTransform;
            if (slotTransform == null)
                continue;

            slotTransform.anchoredPosition = new Vector2(i * (heartSize.x + heartSpacing), 0f);
            slotTransform.sizeDelta = heartSize;
        }
    }

    private void ClearHeartSlots()
    {
        if (heartsRoot == null)
            return;

        for (int i = heartsRoot.childCount - 1; i >= 0; i--)
            Destroy(heartsRoot.GetChild(i).gameObject);
    }

    private static Image CreateHeartImage(RectTransform parent, string objectName, Sprite sprite)
    {
        GameObject heartObject = new(objectName, typeof(RectTransform), typeof(Image));
        RectTransform rectTransform = heartObject.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;

        Image image = heartObject.GetComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;
        image.sprite = sprite;
        image.color = Color.white;
        return image;
    }

    private void ApplyHeartImageLayout(Image image, Vector2 offset)
    {
        if (image == null)
            return;

        RectTransform rectTransform = image.rectTransform;
        rectTransform.anchoredPosition = offset;
        rectTransform.sizeDelta = heartSize;
    }
}
