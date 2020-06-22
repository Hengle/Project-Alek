using UnityEngine;
using Characters;

[ExecuteInEditMode]
public class SpriteOutline : MonoBehaviour {
    [HideInInspector]
    public Color color = Color.green;

    [Range(0, 16)]
    public float outlineSize = 6.5f;

    private SpriteRenderer spriteRenderer;
    private Unit unit;

    void OnEnable() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        unit = GetComponent<Unit>();

        UpdateOutline(true);
    }

    void OnDisable() {
        UpdateOutline(false);
    }
    
    void UpdateOutline(bool outline) {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_Outline", outline ? 1f : 0);
        mpb.SetColor("_OutlineColor", unit.currentColor);
        mpb.SetFloat("_OutlineSize", outlineSize);
        spriteRenderer.SetPropertyBlock(mpb);
    }
}
