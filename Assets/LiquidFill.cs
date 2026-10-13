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
    [Tooltip("The glass's world scale when Empty/Full Height were tuned (the test scene had it at 100). The heights shrink/grow with the glass so a smaller held glass still fills correctly.")]
    [SerializeField] private float heightsTunedAtScale = 100f;

    [Header("Foam")]
    [Tooltip("Foam thickness at the top of the liquid, in the same units as the heights (scales with the glass). Needs the _FoamHeight property in the shader graph.")]
    [SerializeField] private float foamHeight = 0.25f;
    private static readonly int FoamHeightId = Shader.PropertyToID("_FoamHeight");

    [Header("Target band (pour minigame)")]
    [SerializeField] private Color bandColor = new Color(0.2f, 1f, 0.3f, 1f);
    [Tooltip("Ring thickness, in the same units as the heights above (scales with the glass)")]
    [SerializeField] private float bandWidth = 0.08f;
    [Tooltip("0 = work it out from the liquid mesh. Set a number (world units) to force the ring size.")]
    [SerializeField] private float bandRadiusOverride = 0f;
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
        return transform.position.y + Mathf.Lerp(emptyHeight, fullHeight, fill01) * ScaleFactor;
    }

    // 1 when the glass is the size the heights were tuned at
    private float ScaleFactor => Mathf.Abs(transform.lossyScale.x) / Mathf.Max(heightsTunedAtScale, 0.0001f);

    private void Apply()
    {
        liquidRenderer.GetPropertyBlock(block);
        block.SetFloat(FillLevelId, HeightFor(currentFill));
        block.SetFloat(FoamHeightId, foamHeight * ScaleFactor);
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
        Shader sh = Shader.Find("Sprites/Default");
        if (sh == null) sh = Shader.Find("Universal Render Pipeline/Unlit");
        lr.material = new Material(sh);
        lr.startColor = lr.endColor = bandColor;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        return lr;
    }

    private void UpdateBand()
    {
        // Use the mesh bounds (works even while the liquid renderer is disabled at 0 fill)
        Vector3 center = liquidRenderer.transform.position;
        float radius = bandRadiusOverride;
        MeshFilter mf = liquidRenderer.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            Bounds mb = mf.sharedMesh.bounds;
            center = liquidRenderer.transform.TransformPoint(mb.center);
            Vector3 sc = liquidRenderer.transform.lossyScale;
            if (radius <= 0f)
                radius = Mathf.Max(mb.extents.x * Mathf.Abs(sc.x), mb.extents.z * Mathf.Abs(sc.z)) * bandRadiusScale;
        }
        if (radius <= 0f) radius = 0.1f;
        bandMin.startWidth = bandMin.endWidth = bandWidth * ScaleFactor;
        bandMax.startWidth = bandMax.endWidth = bandWidth * ScaleFactor;
        SetRing(bandMin, center, radius, HeightFor(bandMinFill));
        SetRing(bandMax, center, radius, HeightFor(bandMaxFill));
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