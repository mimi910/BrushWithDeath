using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class TempoGroundIndicator : MonoBehaviour
{
    private const string VisualRootName = "_TempoIndicator";
    private const string BaseRendererName = "CurrentTempo";
    private const string ChannelRendererName = "ChannelTarget";
    private const int CircleResolution = 128;

    [Header("References")]
    [SerializeField] private TempoService tempoService;

    [Header("Placement")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.3f, 0f);
    [SerializeField] private Vector2 worldSize = new Vector2(0.9f, 0.9f);

    [Header("Render Order")]
    [SerializeField] private int baseSortingOrder = -2;
    [SerializeField] private int channelSortingOrder = -1;

    [Header("Opacity")]
    [SerializeField, Range(0f, 1f)] private float idleAlpha = 0.24f;
    [SerializeField, Range(0f, 1f)] private float channelStartAlpha = 0.18f;
    [SerializeField, Range(0f, 1f)] private float channelFullAlpha = 0.72f;

    [Header("Tempo Colors")]
    [SerializeField] private Color slowColor = new Color(0.27f, 0.78f, 0.92f, 1f);
    [SerializeField] private Color midColor = new Color(0.93f, 0.77f, 0.34f, 1f);
    [SerializeField] private Color fastColor = new Color(0.96f, 0.49f, 0.25f, 1f);
    [SerializeField] private Color intenseColor = new Color(0.88f, 0.2f, 0.28f, 1f);

    private Transform visualRoot;
    private SpriteRenderer baseRenderer;
    private SpriteRenderer channelRenderer;
    private SpriteRenderer playerSpriteRenderer;
    private SortingGroup sortingGroup;

    private static Sprite sharedCircleSprite;

    private void Awake()
    {
        CacheReferences();
        EnsureVisuals();
        ApplySnapshot(GetSnapshot());
    }

    private void LateUpdate()
    {
        CacheReferences();
        EnsureVisuals();
        ApplySnapshot(GetSnapshot());
    }

    private void OnDisable()
    {
        if (baseRenderer != null)
            baseRenderer.enabled = false;

        if (channelRenderer != null)
            channelRenderer.enabled = false;
    }

    private void CacheReferences()
    {
        if (tempoService == null)
            tempoService = TempoService.Instance != null ? TempoService.Instance : FindAnyObjectByType<TempoService>();

        if (playerSpriteRenderer == null)
            TryGetComponent(out playerSpriteRenderer);

        if (sortingGroup == null)
            TryGetComponent(out sortingGroup);

        if (sortingGroup == null)
        {
            sortingGroup = gameObject.AddComponent<SortingGroup>();

            if (playerSpriteRenderer != null)
            {
                sortingGroup.sortingLayerID = playerSpriteRenderer.sortingLayerID;
                sortingGroup.sortingOrder = playerSpriteRenderer.sortingOrder;
            }
        }
        else if (playerSpriteRenderer != null)
        {
            sortingGroup.sortingLayerID = playerSpriteRenderer.sortingLayerID;
            sortingGroup.sortingOrder = playerSpriteRenderer.sortingOrder;
        }
    }

    private void EnsureVisuals()
    {
        if (visualRoot == null)
        {
            Transform existingRoot = transform.Find(VisualRootName);

            if (existingRoot != null)
            {
                visualRoot = existingRoot;
            }
            else
            {
                GameObject rootObject = new GameObject(VisualRootName);
                visualRoot = rootObject.transform;
                visualRoot.SetParent(transform, false);
            }
        }

        visualRoot.localPosition = localOffset;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;

        baseRenderer = GetOrCreateRenderer(BaseRendererName, ref baseRenderer);
        channelRenderer = GetOrCreateRenderer(ChannelRendererName, ref channelRenderer);

        ConfigureRenderer(baseRenderer, baseSortingOrder);
        ConfigureRenderer(channelRenderer, channelSortingOrder);
    }

    private SpriteRenderer GetOrCreateRenderer(string childName, ref SpriteRenderer cachedRenderer)
    {
        if (cachedRenderer != null)
            return cachedRenderer;

        Transform child = visualRoot.Find(childName);

        if (child == null)
        {
            GameObject childObject = new GameObject(childName);
            child = childObject.transform;
            child.SetParent(visualRoot, false);
        }

        if (!child.TryGetComponent(out cachedRenderer))
            cachedRenderer = child.gameObject.AddComponent<SpriteRenderer>();

        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
        return cachedRenderer;
    }

    private void ConfigureRenderer(SpriteRenderer renderer, int sortingOrder)
    {
        if (renderer == null)
            return;

        renderer.sprite = GetSharedCircleSprite();
        renderer.sortingOrder = sortingOrder;
        renderer.maskInteraction = SpriteMaskInteraction.None;
        renderer.drawMode = SpriteDrawMode.Simple;

        if (playerSpriteRenderer != null)
            renderer.sortingLayerID = playerSpriteRenderer.sortingLayerID;
    }

    private TempoStateSnapshot GetSnapshot()
    {
        if (tempoService != null)
            return tempoService.GetCurrentSnapshot();

        return new TempoStateSnapshot(
            TempoBand.Mid,
            TempoBand.Mid,
            false,
            1f,
            0f,
            0f,
            TempoUpdateType.Initialized);
    }

    private void ApplySnapshot(TempoStateSnapshot snapshot)
    {
        if (baseRenderer == null || channelRenderer == null)
            return;

        baseRenderer.enabled = true;
        baseRenderer.color = WithAlpha(GetTempoColor(snapshot.CurrentTempo), idleAlpha);
        baseRenderer.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);

        bool showChannel = snapshot.IsChanneling && snapshot.TargetTempo != snapshot.CurrentTempo;

        if (!showChannel)
        {
            channelRenderer.enabled = false;
            channelRenderer.transform.localScale = Vector3.zero;
            return;
        }

        float progress = Mathf.Clamp01(snapshot.ChannelProgress);
        channelRenderer.enabled = progress > Mathf.Epsilon;
        channelRenderer.color = WithAlpha(
            GetTempoColor(snapshot.TargetTempo),
            Mathf.Lerp(channelStartAlpha, channelFullAlpha, progress));
        channelRenderer.transform.localScale = new Vector3(
            worldSize.x * progress,
            worldSize.y * progress,
            1f);
    }

    private Color GetTempoColor(TempoBand tempoBand)
    {
        return tempoBand switch
        {
            TempoBand.Slow => slowColor,
            TempoBand.Fast => fastColor,
            TempoBand.Intense => intenseColor,
            _ => midColor
        };
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = Mathf.Clamp01(alpha);
        return color;
    }

    private static Sprite GetSharedCircleSprite()
    {
        if (sharedCircleSprite != null)
            return sharedCircleSprite;

        Texture2D texture = new Texture2D(CircleResolution, CircleResolution, TextureFormat.RGBA32, false);
        texture.name = "TempoIndicatorCircle";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.hideFlags = HideFlags.HideAndDontSave;

        Color[] pixels = new Color[CircleResolution * CircleResolution];
        Vector2 center = new Vector2((CircleResolution - 1) * 0.5f, (CircleResolution - 1) * 0.5f);
        float radius = (CircleResolution * 0.5f) - 2f;
        float edgeSoftness = 2f;

        for (int y = 0; y < CircleResolution; y++)
        {
            for (int x = 0; x < CircleResolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = 1f - Mathf.InverseLerp(radius - edgeSoftness, radius + edgeSoftness, distance);
                pixels[(y * CircleResolution) + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);

        sharedCircleSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, CircleResolution, CircleResolution),
            new Vector2(0.5f, 0.5f),
            CircleResolution);
        sharedCircleSprite.name = "TempoIndicatorCircle";
        sharedCircleSprite.hideFlags = HideFlags.HideAndDontSave;
        return sharedCircleSprite;
    }
}
