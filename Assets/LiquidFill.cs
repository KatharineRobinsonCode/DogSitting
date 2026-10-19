using UnityEngine;

/// <summary>
/// OKAY SO this drives the shader-graph liquid height on a glass, and can draw a green
/// target band (two rings) around the glass for a pour minigame.
/// Put on the same object as Cup.
/// </summary>
public class LiquidFill : MonoBehaviour
{
    [SerializeField] private Renderer liquidRenderer;
    [Tooltip("Recommended. 0 = bottom of the liquid mesh, 1 = top of the liquid mesh. Ignores the Empty/Full Height numbers below.")]
    [SerializeField] private bool autoHeightsFromMesh = true;
    [Tooltip("For stemmed glasses: where the bowl starts, as a fraction of the mesh height from the bottom (0 = bottom of mesh). Liquid below this height is hidden, and 'empty' sits here. Needs the _BottomLevel property in the shader graph.")]
    [SerializeField, Range(0f, 0.9f)] private float bowlStartFraction = 0f;
    private static readonly int BottomLevelId = Shader.PropertyToID("_BottomLevel");

    [Header("Manual heights (only used if Auto Heights is off)")]
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

    [Header("Hide handle (mugs)")]
    [Tooltip("0 = off. Hides any part of the liquid mesh further than this from the glass's middle, so a mug's handle isn't filled. In the mesh's own units (before scaling). Needs the _MaxRadius property in the shader graph. Also trims the foam cap.")]
    [SerializeField] private float bodyRadiusLimit = 0f;
    private static readonly int MaxRadiusId = Shader.PropertyToID("_MaxRadius");

    [Tooltip("Only used if the mesh can't be read (Read/Write off): cap width when the glass is nearly empty, as a fraction of its full width")]
    [SerializeField, Range(0.1f, 1f)] private float emptyWidthRatio = 0.6f;

    private const int CapSectors = 48;
    private GameObject capGO;
    private Mesh capMesh;
    private Vector3[] capVerts;
    private float[] sectorRadius = new float[CapSectors];
    private Vector3 capCentreOffset;       // cross-section centre relative to the liquid object
    private Vector3[] meshVerts;
    private int[] meshTris;
    private Vector3[] worldVerts;
    private readonly System.Collections.Generic.List<Vector3> slicePoints = new System.Collections.Generic.List<Vector3>();
    private float lastCapFill = -999f;
    private Quaternion lastCapRot = Quaternion.identity;

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
        if (capMesh != null) Destroy(capMesh);
    }

    // ---- Foam cap ----
    // The cap is the exact cross-section of the liquid mesh at the liquid surface, so it
    // follows the glass shape, the taper, and any tilt of the glass.

    private void UpdateCap(bool show)
    {
        if (!useCap || !show)
        {
            if (capGO != null) capGO.SetActive(false);
            return;
        }

        if (capGO == null && !BuildCap()) return;

        Transform t = liquidRenderer.transform;
        bool moved = Mathf.Abs(currentFill - lastCapFill) > 0.0005f
                  || Quaternion.Angle(t.rotation, lastCapRot) > 0.05f;
        if (moved)
        {
            lastCapFill = currentFill;
            lastCapRot = t.rotation;
            RecalculateCapShape();
        }

        // Rebuild the disc in world space (cheap: ~50 vertices)
        float y = HeightFor(currentFill);
        Vector3 centre = t.position + capCentreOffset;
        capVerts[0] = new Vector3(centre.x, y, centre.z);
        for (int i = 0; i < CapSectors; i++)
        {
            float a = (i + 0.5f) / CapSectors * Mathf.PI * 2f - Mathf.PI;
            float r = sectorRadius[i] * capRadiusScale;
            capVerts[i + 1] = new Vector3(centre.x + Mathf.Cos(a) * r, y, centre.z + Mathf.Sin(a) * r);
        }
        capMesh.vertices = capVerts;
        capMesh.RecalculateBounds();
        capGO.SetActive(true);
    }

    private bool BuildCap()
    {
        MeshFilter mf = liquidRenderer.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return false;

        if (mf.sharedMesh.isReadable)
        {
            meshVerts = mf.sharedMesh.vertices;
            meshTris = mf.sharedMesh.triangles;
            worldVerts = new Vector3[meshVerts.Length];
        }
        else
        {
            meshVerts = null;
            Debug.LogWarning("[LiquidFill] Mesh '" + mf.sharedMesh.name + "' isn't readable, so the foam cap can't follow the glass shape. Tick Read/Write on the model's import settings for a better fit.", this);
        }

        // Flat fan of triangles: centre vertex + one vertex per sector
        capVerts = new Vector3[CapSectors + 1];
        Vector3[] n = new Vector3[CapSectors + 1];
        int[] tris = new int[CapSectors * 3];
        for (int i = 0; i < n.Length; i++) n[i] = Vector3.up;
        for (int i = 0; i < CapSectors; i++)
        {
            tris[i * 3]     = 0;
            tris[i * 3 + 1] = (i + 1) % CapSectors + 1;
            tris[i * 3 + 2] = i + 1;
        }
        capMesh = new Mesh { name = "LiquidCapMesh" };
        capMesh.MarkDynamic();
        capMesh.vertices = capVerts;
        capMesh.normals = n;
        capMesh.triangles = tris;

        capGO = new GameObject("LiquidCap");   // lives at the world origin, vertices are in world space
        capGO.AddComponent<MeshFilter>().sharedMesh = capMesh;
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

    private void RecalculateCapShape()
    {
        Transform t = liquidRenderer.transform;

        if (meshVerts == null || !SliceMesh(t))
        {
            // Fallback: a circle from the overall width, narrower when nearly empty
            Bounds bb = liquidRenderer.bounds;
            float fullR = Mathf.Max(bb.extents.x, bb.extents.z);
            float r = fullR * Mathf.Lerp(emptyWidthRatio, 1f, currentFill);
            for (int i = 0; i < CapSectors; i++) sectorRadius[i] = r;
            capCentreOffset = new Vector3(bb.center.x - t.position.x, 0f, bb.center.z - t.position.z);
        }
    }

    // Cuts the liquid mesh with the horizontal plane at the surface and measures the outline
    private bool SliceMesh(Transform t)
    {
        float y = HeightFor(currentFill);
        Matrix4x4 m = t.localToWorldMatrix;
        for (int i = 0; i < meshVerts.Length; i++) worldVerts[i] = m.MultiplyPoint3x4(meshVerts[i]);

        slicePoints.Clear();
        for (int i = 0; i < meshTris.Length; i += 3)
        {
            Vector3 a = worldVerts[meshTris[i]];
            Vector3 b = worldVerts[meshTris[i + 1]];
            Vector3 c = worldVerts[meshTris[i + 2]];
            AddCrossing(a, b, y);
            AddCrossing(b, c, y);
            AddCrossing(c, a, y);
        }

        // Ignore points out on the handle
        if (bodyRadiusLimit > 0f)
        {
            for (int i = slicePoints.Count - 1; i >= 0; i--)
            {
                Vector3 lp = t.InverseTransformPoint(slicePoints[i]);
                if (lp.x * lp.x + lp.z * lp.z > bodyRadiusLimit * bodyRadiusLimit) slicePoints.RemoveAt(i);
            }
        }
        if (slicePoints.Count < 3) return false;

        // Centre of the outline
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < slicePoints.Count; i++) sum += slicePoints[i];
        Vector3 centre = sum / slicePoints.Count;

        // Furthest point in each direction
        for (int i = 0; i < CapSectors; i++) sectorRadius[i] = 0f;
        for (int i = 0; i < slicePoints.Count; i++)
        {
            float dx = slicePoints[i].x - centre.x;
            float dz = slicePoints[i].z - centre.z;
            float ang = Mathf.Atan2(dz, dx);
            int s = Mathf.Clamp((int)((ang + Mathf.PI) / (Mathf.PI * 2f) * CapSectors), 0, CapSectors - 1);
            float d = Mathf.Sqrt(dx * dx + dz * dz);
            if (d > sectorRadius[s]) sectorRadius[s] = d;
        }

        // Fill any empty sectors from their neighbours
        for (int pass = 0; pass < 3; pass++)
        {
            for (int i = 0; i < CapSectors; i++)
            {
                if (sectorRadius[i] > 0f) continue;
                float p = sectorRadius[(i + CapSectors - 1) % CapSectors];
                float n = sectorRadius[(i + 1) % CapSectors];
                if (p > 0f && n > 0f) sectorRadius[i] = (p + n) * 0.5f;
                else if (p > 0f) sectorRadius[i] = p;
                else if (n > 0f) sectorRadius[i] = n;
            }
        }

        capCentreOffset = new Vector3(centre.x - t.position.x, 0f, centre.z - t.position.z);
        return true;
    }

    private void AddCrossing(Vector3 p, Vector3 q, float y)
    {
        if ((p.y - y) * (q.y - y) >= 0f) return;
        float k = (y - p.y) / (q.y - p.y);
        slicePoints.Add(Vector3.Lerp(p, q, k));
    }

    private MeshFilter liquidMF;

    // World-space bottom and top of the liquid mesh (works even while the renderer is disabled)
    private bool TryGetMeshHeights(out float lo, out float hi)
    {
        lo = hi = 0f;
        if (liquidRenderer == null) return false;
        if (liquidMF == null) liquidMF = liquidRenderer.GetComponent<MeshFilter>();
        if (liquidMF == null || liquidMF.sharedMesh == null) return false;

        Bounds b = liquidMF.sharedMesh.bounds;
        Transform t = liquidRenderer.transform;
        float minY = float.MaxValue, maxY = float.MinValue;
        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = b.center + Vector3.Scale(b.extents,
                new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
            float y = t.TransformPoint(corner).y;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }
        lo = minY;
        hi = maxY;
        return true;
    }

    private float HeightFor(float fill01)
    {
        if (autoHeightsFromMesh && TryGetMeshHeights(out float lo, out float hi))
            return Mathf.Lerp(lo + (hi - lo) * bowlStartFraction, hi, fill01);
        return transform.position.y + Mathf.Lerp(emptyHeight, fullHeight, fill01) * ScaleFactor;
    }

    private float HeightSpan()
    {
        if (autoHeightsFromMesh && TryGetMeshHeights(out float lo, out float hi)) return (hi - lo) * (1f - bowlStartFraction);
        return Mathf.Abs(fullHeight - emptyHeight) * ScaleFactor;
    }

    // 1 when the glass is the size the heights were tuned at
    private float ScaleFactor => Mathf.Abs(transform.lossyScale.x) / Mathf.Max(heightsTunedAtScale, 0.0001f);

    private void Apply()
    {
        liquidRenderer.GetPropertyBlock(block);
        block.SetFloat(FillLevelId, HeightFor(currentFill));
        block.SetFloat(BottomLevelId, HeightFor(0f) - 0.0005f);
        block.SetFloat(FoamHeightId, foamHeight * ScaleFactor);
        block.SetFloat(MaxRadiusId, bodyRadiusLimit > 0f ? bodyRadiusLimit : 100000f);
        liquidRenderer.SetPropertyBlock(block);

        // Also write straight onto this glass's own material copy (most reliable route)
        Material mat = liquidRenderer.material;
        if (!checkedProps)
        {
            checkedProps = true;
            CheckProp(mat, "_FillLevel");
            CheckProp(mat, "_BottomLevel");
            CheckProp(mat, "_FoamHeight");
        }
        if (mat.HasProperty(FillLevelId))   mat.SetFloat(FillLevelId, HeightFor(currentFill));
        if (mat.HasProperty(BottomLevelId)) mat.SetFloat(BottomLevelId, HeightFor(0f) - 0.0005f);
        if (mat.HasProperty(FoamHeightId))  mat.SetFloat(FoamHeightId, foamHeight * ScaleFactor);
        if (mat.HasProperty(MaxRadiusId))   mat.SetFloat(MaxRadiusId, bodyRadiusLimit > 0f ? bodyRadiusLimit : 100000f);

        if (debugLog && Time.unscaledTime > nextDebugTime)
        {
            nextDebugTime = Time.unscaledTime + 0.5f;
            TryGetMeshHeights(out float dlo, out float dhi);
            Debug.Log($"[LiquidFill DEBUG] {name}: fill={currentFill:F2} meshBottom={dlo:F3} meshTop={dhi:F3} " +
                      $"bottomLevel={HeightFor(0f) - 0.0005f:F3} fillLevel={HeightFor(currentFill):F3} " +
                      $"foam={foamHeight * ScaleFactor:F3} lossyScale={liquidRenderer.transform.lossyScale} " +
                      $"mat={liquidRenderer.sharedMaterial.name} shader={liquidRenderer.sharedMaterial.shader.name}", this);
        }
    }

    private bool checkedProps;
    private void CheckProp(Material m, string prop)
    {
        if (!m.HasProperty(prop))
            Debug.LogError($"[LiquidFill] Material '{m.name}' (shader '{m.shader.name}') has NO property called '{prop}'. " +
                           "In the Shader Graph, select that property in the Blackboard and set its Reference to exactly '" + prop + "', make sure Exposed is ticked, then Save Asset.", this);
    }

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;
    private float nextDebugTime;

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