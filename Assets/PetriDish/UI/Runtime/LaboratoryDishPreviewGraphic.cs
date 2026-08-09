using UnityEngine;
using UnityEngine.UI;

namespace PetriDish.Presentation.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LaboratoryDishPreviewGraphic : MaskableGraphic
    {
        [SerializeField, Range(32, 160)] private int segments = 96;
        [SerializeField] private Color shadowColor = new Color(0.12f, 0.20f, 0.22f, 0.12f);
        [SerializeField] private Color glassEdgeColor = new Color(0.50f, 0.76f, 0.78f, 0.72f);
        [SerializeField] private Color glassColor = new Color(0.89f, 0.97f, 0.97f, 0.82f);
        [SerializeField] private Color agarColor = new Color(0.90f, 0.94f, 0.76f, 1f);
        [SerializeField] private Color agarHighlight = new Color(0.98f, 0.99f, 0.91f, 0.72f);
        [SerializeField] private Color colonyColor = new Color(0.32f, 0.62f, 0.34f, 0.92f);
        [SerializeField] private Color colonyLight = new Color(0.50f, 0.74f, 0.42f, 0.86f);

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            float radius = Mathf.Min(rect.width, rect.height) * 0.46f;
            Vector2 centre = rect.center;

            AddDisc(vh, centre + new Vector2(radius * 0.025f, -radius * 0.045f), radius * 1.035f, shadowColor);
            AddDisc(vh, centre, radius, glassEdgeColor);
            AddDisc(vh, centre, radius * 0.955f, glassColor);
            AddDisc(vh, centre, radius * 0.835f, agarColor);
            AddDisc(vh, centre + new Vector2(-radius * 0.10f, radius * 0.12f), radius * 0.68f, agarHighlight);
            AddDisc(vh, centre, radius * 0.790f, agarColor);

            float agarRadius = radius * 0.79f;
            AddColony(vh, centre, agarRadius, new Vector2(-0.33f, 0.20f), 0.17f, colonyColor);
            AddColony(vh, centre, agarRadius, new Vector2(0.20f, 0.29f), 0.10f, colonyLight);
            AddColony(vh, centre, agarRadius, new Vector2(0.31f, -0.17f), 0.20f, colonyColor);
            AddColony(vh, centre, agarRadius, new Vector2(-0.15f, -0.31f), 0.085f, colonyLight);
            AddColony(vh, centre, agarRadius, new Vector2(0.01f, 0.00f), 0.13f, colonyLight);
            AddColony(vh, centre, agarRadius, new Vector2(-0.43f, -0.08f), 0.055f, colonyColor);
            AddColony(vh, centre, agarRadius, new Vector2(0.44f, 0.10f), 0.045f, colonyLight);
        }

        private void AddColony(VertexHelper vh, Vector2 centre, float agarRadius,
            Vector2 position, float radius, Color tint)
        {
            Vector2 colonyCentre = centre + position * agarRadius;
            AddDisc(vh, colonyCentre, agarRadius * radius, tint);
            AddDisc(vh, colonyCentre - new Vector2(radius, radius) * agarRadius * 0.18f,
                agarRadius * radius * 0.58f, tint * new Color(1f, 1f, 1f, 0.72f));
        }

        private void AddDisc(VertexHelper vh, Vector2 centre, float radius, Color tint)
        {
            int start = vh.currentVertCount;
            vh.AddVert(centre, tint, Vector2.one * 0.5f);
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                vh.AddVert(centre + direction * radius, tint, (direction + Vector2.one) * 0.5f);
            }
            for (int i = 0; i < segments; i++)
                vh.AddTriangle(start, start + i + 1, start + i + 2);
        }
    }
}
