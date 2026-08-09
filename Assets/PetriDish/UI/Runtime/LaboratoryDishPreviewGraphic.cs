using UnityEngine;
using UnityEngine.UI;

namespace PetriDish.Presentation.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LaboratoryDishPreviewGraphic : MaskableGraphic
    {
        [SerializeField, Range(24, 128)] private int segments = 64;
        [SerializeField, Range(0.55f, 0.95f)] private float agarRadius = 0.82f;
        [SerializeField] private Color rimColor = new Color(0.27f, 0.84f, 0.88f, 0.34f);
        [SerializeField] private Color agarColor = new Color(0.16f, 0.28f, 0.21f, 0.95f);
        [SerializeField] private Color colonyColor = new Color(0.42f, 0.82f, 0.48f, 0.9f);

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            Vector2 centre = rect.center;
            AddDisc(vh, centre, radius, rimColor);
            AddDisc(vh, centre, radius * agarRadius, agarColor);
            float r = radius * agarRadius;
            AddDisc(vh, centre + new Vector2(-0.31f, 0.18f) * r, r * 0.18f, colonyColor);
            AddDisc(vh, centre + new Vector2(0.20f, 0.26f) * r, r * 0.12f, colonyColor * new Color(1f, 1f, 1f, 0.82f));
            AddDisc(vh, centre + new Vector2(0.29f, -0.18f) * r, r * 0.23f, colonyColor);
            AddDisc(vh, centre + new Vector2(-0.12f, -0.30f) * r, r * 0.10f, colonyColor);
            AddDisc(vh, centre + new Vector2(0.02f, 0.01f) * r, r * 0.15f, colonyColor * new Color(1f, 1f, 1f, 0.72f));
        }

        private void AddDisc(VertexHelper vh, Vector2 centre, float radius, Color tint)
        {
            int start = vh.currentVertCount;
            vh.AddVert(centre, tint, Vector2.one * 0.5f);
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 d = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                vh.AddVert(centre + d * radius, tint, (d + Vector2.one) * 0.5f);
            }
            for (int i = 0; i < segments; i++) vh.AddTriangle(start, start + i + 1, start + i + 2);
        }
    }
}
