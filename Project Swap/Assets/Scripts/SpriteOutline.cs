using System;
using UnityEngine;
using Characters;

[ExecuteInEditMode]
public class SpriteOutline : MonoBehaviour
{
    public Color color;

    [Range(0, 16)]
    public float outlineSize = 6.5f;

    private SpriteRenderer spriteRenderer;

    void OnEnable() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateOutline(true);
    }

    void OnDisable() {
        UpdateOutline(false);
    }
    
    void UpdateOutline(bool outline) {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_Outline", outline ? 1f : 0);
        mpb.SetColor("_OutlineColor", color); // probably need to change this for objects that are not units
        mpb.SetFloat("_OutlineSize", outlineSize);
        spriteRenderer.SetPropertyBlock(mpb);
    }
}
