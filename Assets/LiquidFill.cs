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

    [Header("Foam cap (fills the open top of the glass)")]
    [SerializeField] private bool useCap = true;
    [Tooltip("Match this to the Foam Color on the liquid material")]
    [SerializeField] private Color capColor = new Color(0.96f, 0.92f, 0.82f, 1f);
    [Tooltip("Shrink or grow the cap if it pokes through the glass or leaves a gap")]
    [SerializeField] private float capRadiusScale = 0.97f;

    [Tooltip("Only used if the mesh can't be read (Read/Write off): cap width when the glass is nearly empty, as a fraction of its full width")]
    [SerializeField, Range(0.1f, 1f)] private float emptyWidthRatio = 0.6f;

    private GameObject capGO;
    private Transform capT;
    private Vector3[] meshVerts;
    private Vector3 meshCentreLocal;
    private float capRadius;
    private float lastCapFill = -1f;

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
        bool liquidShowing = liquidRenderer.enabled && liquidRenderer.gameObject.activeInHierarchy;
        if (liquidRenderer.enabled) Apply();
        UpdateCap(liquidShowing);
        if (bandVisible) UpdateBand();
    }

    private void OnDisable()
    {
        if (capGO != null) capGO.SetActive(false);
    }

    private void OnDestroy()
    {
        if (capGO != null) Destroy(capGO);
    }

    // ---- Foam cap ----

    private void UpdateCap(bool show)
    {
        if (!useCap || !show)
        {
            if (capGO != null) capGO.SetActive(false);
            return;
        }

        if (capGO == null && !BuildCap()) return;

        // Re-measure the glass width at the surface whenever the fill changes
        if (Mathf.Abs(currentFill - lastCapFill) > 0.001f)
        {
            lastCapFill = currentFill;
            RecalculateCapRadius();
        }

        Vector3 centre = liquidRenderer.transform.TransformPoint(meshCentreLocal);
        capT.position = new Vector3(centre.x, HeightFor(currentFill), centre.z);
        capT.localScale = new Vector3(capRadius, 1f, capRadius);
        capGO.SetActive(true);
    }

    private bool BuildCap()
    {
        MeshFilter mf = liquidRenderer.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return false;

        meshCentreLocal = mf.sharedMesh.bounds.center;
        if (mf.sharedMesh.isReadable)
        {
            meshVerts = mf.sharedMesh.vertices;
        }
        else
        {
            meshVerts = null;
            Debug.LogWarning("[LiquidFill] Mesh '" + mf.sharedMesh.name + "' isn't readable, so the foam cap can't follow the glass shape. Tick Read/Write on the model's import settings for a better fit.", this);
        }

        // Flat disc, radius 1, facing up
        const int seg = 40;
        Vector3[] v = new Vector3[seg + 1];
        Vector3[] n = new Vector3[seg + 1];
        int[] tris = new int[seg * 3];
        v[0] = Vector3.zero;
        for (int i = 0; i < seg; i++)
        {
            float a = i / (float)seg * Mathf.PI * 2f;
            v[i + 1] = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
        }
        for (int i = 0; i < n.Length; i++) n[i] = Vector3.up;
        for (int i = 0; i < seg; i++)
        {
            tris[i * 3]     = 0;
            tris[i * 3 + 1] = (i + 1) % seg + 1;
            tris[i * 3 + 2] = i + 1;
        }
        Mesh disc = new Mesh { name = "LiquidCapDisc", vertices = v, normals = n, triangles = tris };

        capGO = new GameObject("LiquidCap");   // not parented: the glass's scale would distort it
        capT = capGO.transform;
        capGO.AddComponent<MeshFilter>().sharedMesh = disc;
        MeshRenderer mr = capGO.AddComponent<MeshRenderer>();

        Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (sh == null) sh = Shader.Find("Sprites/Default");
        Material m = new Material(sh);
        m.color = capColor;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", capColor);
        if (m.HasProperty("_Cull")) m.SetFloat("_Cull", 0f);   // visible from both sides
        mr.sharedMaterial = m;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        return true;
    }

    private void RecalculateCapRadius()
    {
        if (meshVerts == null)   // can't read the mesh: simple taper from narrow (bottom) to full width (top)
        {
            Bounds bb = liquidRenderer.bounds;
            float fullR = Mathf.Max(bb.extents.x, bb.extents.z);
            capRadius = fullR * Mathf.Lerp(emptyWidthRatio, 1f, currentFill) * capRadiusScale;
            return;
        }

        Transform t = liquidRenderer.transform;
        Vector3 centre = t.TransformPoint(meshCentreLocal);
        float surfaceY = HeightFor(currentFill);
        float tol = Mathf.Abs(fullHeight - emptyHeight) * ScaleFactor * 0.04f;

        float best = 0f;
        for (int i = 0; i < meshVerts.Length; i++)
        {
            Vector3 w = t.TransformPoint(meshVerts[i]);
            if (Mathf.Abs(w.y - surfaceY) > tol) continue;
            float d = new Vector2(w.x - centre.x, w.z - centre.z).magnitude;
            if (d > best) best = d;
        }

        if (best <= 0f)   // nothing near the surface: fall back to the overall width
        {
            Bounds b = liquidRenderer.bounds;
            best = Mathf.Max(b.extents.x, b.extents.z);
        }
        capRadius = best * capRadiusScale;
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