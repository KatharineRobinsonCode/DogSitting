using UnityEngine;

public class LiquidFill : MonoBehaviour
{
    [SerializeField] private Renderer liquidRenderer;
    [SerializeField] private float emptyHeight = -1.0f;  // _FillLevel when empty (relative to glass)
    [SerializeField] private float fullHeight  = 2.8f;   // _FillLevel when full, leaving room for foam

    private MaterialPropertyBlock block;
    private static readonly int FillLevelId = Shader.PropertyToID("_FillLevel");
    private float currentFill;

    private void Awake()
    {
        block = new MaterialPropertyBlock();
        SetFill(0f);
    }

    public void SetFill(float fill01)
    {
        currentFill = Mathf.Clamp01(fill01);
        liquidRenderer.enabled = currentFill > 0.001f;  // hide completely when empty
        Apply();
    }

    // Keeps the liquid level correct while the glass is carried or moved
    private void LateUpdate() { if (liquidRenderer.enabled) Apply(); }

    private void Apply()
    {
        float worldY = transform.position.y + Mathf.Lerp(emptyHeight, fullHeight, currentFill);
        liquidRenderer.GetPropertyBlock(block);
        block.SetFloat(FillLevelId, worldY);
        liquidRenderer.SetPropertyBlock(block);
    }
}