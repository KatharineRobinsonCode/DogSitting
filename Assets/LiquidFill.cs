using UnityEngine;

/// <summary>
/// Drives the shader-graph liquid height on a glass, and can draw a green
/// target band (two rings) around the glass for the pour minigame.
/// Put on the same object as Cup.
/// </summary>
public class LiquidFill : MonoBehaviour
{
    [SerializeField] private Renderer liquidRenderer;
    [SerializeField] private float emptyHeight = -1.0f;
    [SerializeField] private float fullHeight  = 2.8f;

    [Header("Target band (pour minigame)")]
    [SerializeField] private Color bandColor = new Color(0.2f, 1f, 0.3f, 1f);
    [SerializeField] private float bandWidth = 0.01f;
    [Tooltip("Ring size relative to the liquid mesh. Raise if the ring sits inside the glass.")]
    [SerializeField] private float bandRadiusScale = 1.15f;

    private MaterialPropertyBlock block;
    private static readonly int FillLevelId = Shader.PropertyToID("_FillLevel");
    private float currentFill;

    private LineRenderer bandMin, bandMax;
    private float bandMinFill, bandMaxFill;
    private bool bandVisible;
    private const int RingPoints = 48;

    private void Awake()
    {
        block = new MaterialPropertyBlock();
        SetFill(0f);
    }

    public void SetFill(float fill01)
    {
        if (block == null) block = new MaterialPropertyBlock();
        currentFill = Mathf.Clamp01(fill01);
        liquidRenderer.enabled = currentFill > 0.001f;
        Apply();
    }

    private void LateUpdate()
    {
        if (liquidRenderer.enabled) Apply();
        if (bandVisible) UpdateBand();
    }

    private float HeightFor(float fill01)
    {
        return transform.position.y + Mathf.Lerp(emptyHeight, fullHeight, fill01);
    }

    private void Apply()
    {
        liquidRenderer.GetPropertyBlock(block);
        block.SetFloat(FillLevelId, HeightFor(currentFill));
        liquidRenderer.SetPropertyBlock(block);
    }

    // ---- Target band ----

    public void ShowTargetBand(float minFill01, float maxFill01)
    {
        bandMinFill = minFill01;
        bandMaxFill = maxFill01;
        if (bandMin == null) bandMin = CreateRing("BandMin");
        if (bandMax == null) bandMax = CreateRing("BandMax");
        bandVisible = true;
        bandMin.gameObject.SetActive(true);
        bandMax.gameObject.SetActive(true);
        UpdateBand();
    }

    public void HideTargetBand()
    {
        bandVisible = false;
        if (bandMin != null) bandMin.gameObject.SetActive(false);
        if (bandMax != null) bandMax.gameObject.SetActive(false);
    }

    private LineRenderer CreateRing(string ringName)
    {
        GameObject go = new GameObject(ringName);
        go.transform.SetParent(transform, false);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = RingPoints;
        lr.startWidth = lr.endWidth = bandWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = bandColor;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        return lr;
    }

    private void UpdateBand()
    {
        Bounds b = liquidRenderer.bounds;
        float radius = Mathf.Max(b.extents.x, b.extents.z) * bandRadiusScale;
        SetRing(bandMin, b.center, radius, HeightFor(bandMinFill));
        SetRing(bandMax, b.center, radius, HeightFor(bandMaxFill));
    }

    private void SetRing(LineRenderer lr, Vector3 center, float radius, float y)
    {
        for (int i = 0; i < RingPoints; i++)
        {
            float a = i / (float)RingPoints * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(center.x + Mathf.Cos(a) * radius, y, center.z + Mathf.Sin(a) * radius));
        }
    }
}