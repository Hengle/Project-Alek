using UnityEngine;

namespace Utils
{
    [ExecuteInEditMode] public class SpriteOutline : MonoBehaviour
    {
        public Color color;

        private SpriteRenderer spriteRenderer;

        [Range(0, 16)] [SerializeField] 
        private float outlineSize = 6.5f;

        private readonly int outlineId = Shader.PropertyToID("_Outline");
        private readonly int outlineColorId = Shader.PropertyToID("_OutlineColor");
        private readonly int outlineSizeId = Shader.PropertyToID("_OutlineSize");

        private void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();
    
        private void OnEnable() => UpdateOutline(true);

        private void OnDisable() => UpdateOutline(false); 

        private void UpdateOutline(bool outline)
        {
            var mpb = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(mpb);
        
            mpb.SetFloat(outlineId, outline ? 1f : 0);
            mpb.SetColor(outlineColorId, color);
            mpb.SetFloat(outlineSizeId, outlineSize);
            spriteRenderer.SetPropertyBlock(mpb);
        }
    }
}
