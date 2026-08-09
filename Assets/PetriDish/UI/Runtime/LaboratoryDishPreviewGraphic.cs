using UnityEngine;
using UnityEngine.UI;

namespace PetriDish.Presentation.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LaboratoryDishPreviewGraphic : MaskableGraphic
    {
        [SerializeField, Range(48, 192)] private int segments = 120;
        [SerializeField] private Color shadowColor = new Color(0.10f, 0.17f, 0.18f, 0.15f);
        [SerializeField] private Color glassDepthColor = new Color(0.58f, 0.75f, 0.76f, 0.28f);
        [SerializeField] private Color glassEdgeColor = new Color(0.66f, 0.84f, 0.84f, 0.62f);
        [SerializeField] private Color glassColor = new Color(0.96f, 0.99f, 0.98f, 0.72f);
        [SerializeField] private Color rimHighlightColor = new Color(1f, 1f, 1f, 0.78f);
        [SerializeField] private Color agarDepthColor = new Color(0.76f, 0.79f, 0.58f, 0.30f);
        [SerializeField] private Color agarColor = new Color(0.93f, 0.94f, 0.78f, 0.98f);
        [SerializeField] private Color agarHighlight = new Color(1f, 1f, 0.94f, 0.42f);
        [SerializeField] private Color colonyColor = new Color(0.30f, 0.59f, 0.34f, 0.92f);
        [SerializeField] private Color colonyLight = new Color(0.55f, 0.73f, 0.43f, 0.82f);

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            float radius = Mathf.Min(rect.width, rect.height) * 0.485f;
            Vector2 centre = rect.center;

            AddEllipse(vh, centre + new Vector2(radius * 0.035f, -radius * 0.075f),
                new Vector2(radius * 1.02f, radius * 0.95f), shadowColor);
            AddEllipse(vh, centre + Vector2.down * radius * 0.025f,
                new Vector2(radius, radius * 0.965f), glassDepthColor);
            AddDisc(vh, centre, radius, glassEdgeColor);
            AddDisc(vh, centre, radius * 0.955f, glassColor);

            AddEllipse(vh, centre + Vector2.down * radius * 0.018f,
                new Vector2(radius * 0.855f, radius * 0.825f), agarDepthColor);
            AddDisc(vh, centre, radius * 0.835f, agarColor);
            AddEllipse(vh, centre + new Vector2(-radius * 0.12f, radius * 0.13f),
                new Vector2(radius * 0.57f, radius * 0.46f), agarHighlight);
            AddDisc(vh, centre, radius * 0.805f, agarColor);

            float agarRadius = radius * 0.805f;
            AddColonyCluster(vh, centre, agarRadius, new Vector2(-0.33f, 0.20f), 0.15f, colonyColor);
            AddColonyCluster(vh, centre, agarRadius, new Vector2(0.18f, 0.28f), 0.09f, colonyLight);
            AddColonyCluster(vh, centre, agarRadius, new Vector2(0.30f, -0.18f), 0.18f, colonyColor);
            AddColonyCluster(vh, centre, agarRadius, new Vector2(-0.15f, -0.30f), 0.08f, colonyLight);
            AddColonyCluster(vh, centre, agarRadius, new Vector2(0.00f, 0.00f), 0.12f, colonyLight);
            AddColonyCluster(vh, centre, agarRadius, new Vector2(-0.45f, -0.07f), 0.05f, colonyColor);
            AddColonyCluster(vh, centre, agarRadius, new Vector2(0.45f, 0.10f), 0.045f, colonyLight);

            AddRing(vh, centre, radius * 0.965f, radius * 0.925f, rimHighlightColor);
            AddRing(vh, centre, radius, radius * 0.982f, glassEdgeColor);
        }

        private void AddColonyCluster(VertexHelper vh, Vector2 centre, float agarRadius,
            Vector2 position, float radius, Color tint)
        {
            Vector2 colonyCentre = centre + position * agarRadius;
            float colonyRadius = agarRadius * radius;
            AddDisc(vh, colonyCentre, colonyRadius, tint);
            AddDisc(vh, colonyCentre + new Vector2(colonyRadius * 0.42f, colonyRadius * 0.12f),
                colonyRadius * 0.55f, tint);
            AddDisc(vh, colonyCentre + new Vector2(-colonyRadius * 0.24f, colonyRadius * 0.36f),
                colonyRadius * 0.48f, tint);
            AddDisc(vh, colonyCentre + new Vector2(-colonyRadius * 0.18f, colonyRadius * 0.26f),
                colonyRadius * 0.28f, colonyLight);
        }

        private void AddDisc(VertexHelper vh, Vector2 centre, float radius, Color tint) =>
            AddEllipse(vh, centre, Vector2.one * radius, tint);

        private void AddEllipse(VertexHelper vh, Vector2 centre, Vector2 radii, Color tint)
        {
            int start = vh.currentVertCount;
            vh.AddVert(centre, tint, Vector2.one * 0.5f);
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                vh.AddVert(centre + Vector2.Scale(direction, radii), tint, (direction + Vector2.one) * 0.5f);
            }
            for (int i = 0; i < segments; i++)
                vh.AddTriangle(start, start + i + 1, start + i + 2);
        }

        private void AddRing(VertexHelper vh, Vector2 centre, float outerRadius, float innerRadius, Color tint)
        {
            int start = vh.currentVertCount;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 uv = (direction + Vector2.one) * 0.5f;
                vh.AddVert(centre + direction * outerRadius, tint, uv);
                vh.AddVert(centre + direction * innerRadius, tint, uv);
            }
            for (int i = 0; i < segments; i++)
            {
                int outer = start + i * 2;
                int inner = outer + 1;
                int nextOuter = outer + 2;
                int nextInner = outer + 3;
                vh.AddTriangle(outer, nextOuter, inner);
                vh.AddTriangle(nextOuter, nextInner, inner);
            }
        }
    }
}
