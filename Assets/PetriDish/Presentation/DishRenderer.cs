using System;
using PetriDish.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PetriDish.Presentation
{
    [RequireComponent(typeof(RawImage))]
    public sealed class DishRenderer : MonoBehaviour, IPointerClickHandler
    {
        private const float OuterRadius = 0.98f;
        private const float AgarRadius = 0.87f;

        private RawImage target;
        private Texture2D texture;
        private Color32[] pixels;

        public event Action<Vector2> DishTapped;

        private void Awake()
        {
            target = GetComponent<RawImage>();
            texture = new Texture2D(PetriSimulation.GridWidth, PetriSimulation.GridHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            target.texture = texture;
            pixels = new Color32[PetriSimulation.GridWidth * PetriSimulation.GridHeight];
        }

        private void OnDestroy()
        {
            if (texture == null) return;
            if (UnityEngine.Application.isPlaying)
                Destroy(texture);
            else
                DestroyImmediate(texture);
        }

        public void Render(SimulationSnapshot snapshot)
        {
            if (snapshot.Biomass == null || snapshot.Health == null || snapshot.Moisture == null)
                throw new ArgumentException("Snapshot rendering arrays cannot be null.", nameof(snapshot));
            if (snapshot.Biomass.Length != snapshot.Health.Length ||
                snapshot.Biomass.Length != snapshot.Moisture.Length)
                throw new ArgumentException("Snapshot rendering arrays must have equal lengths.", nameof(snapshot));
            if (pixels == null || pixels.Length != snapshot.Biomass.Length)
                pixels = new Color32[snapshot.Biomass.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % PetriSimulation.GridWidth;
                int y = i / PetriSimulation.GridWidth;
                float dishX = ((x + 0.5f) / PetriSimulation.GridWidth * 2f) - 1f;
                float dishY = ((y + 0.5f) / PetriSimulation.GridHeight * 2f) - 1f;
                float radius = Mathf.Sqrt(dishX * dishX + dishY * dishY);

                if (radius > OuterRadius)
                {
                    pixels[i] = new Color32(0, 0, 0, 0);
                    continue;
                }

                if (radius > AgarRadius)
                {
                    float rimPosition = Mathf.InverseLerp(OuterRadius, AgarRadius, radius);
                    float highlight = GlassHighlight(dishX, dishY);
                    Color rim = Color.Lerp(
                        new Color(0.10f, 0.18f, 0.17f, 0.72f),
                        new Color(0.52f, 0.78f, 0.70f, 0.92f),
                        Mathf.Clamp01(rimPosition * 0.65f + highlight));
                    pixels[i] = rim;
                    continue;
                }

                float biomass = Mathf.Clamp01(snapshot.Biomass[i]);
                float health = Mathf.Clamp01(snapshot.Health[i]);
                float moisture = Mathf.Clamp01(snapshot.Moisture[i]);

                float edgeShade = Mathf.SmoothStep(0.55f, 1f, radius / AgarRadius);
                Color medium = Color.Lerp(
                    new Color(0.30f, 0.19f, 0.07f),
                    new Color(0.66f, 0.49f, 0.20f),
                    moisture);
                medium = Color.Lerp(medium, new Color(0.18f, 0.13f, 0.07f), edgeShade * 0.28f);

                Color colony = Color.Lerp(
                    new Color(0.35f, 0.18f, 0.10f),
                    new Color(0.67f, 0.94f, 0.50f),
                    health);
                float colonyTexture = ColonyTexture(x, y);
                colony = Color.Lerp(colony * 0.82f, colony * 1.08f, colonyTexture);
                float colonyWeight = Mathf.SmoothStep(0.03f, 0.82f, biomass);
                Color result = Color.Lerp(medium, colony, colonyWeight);

                if (biomass > 0.08f && IsGrowthEdge(snapshot.Biomass, x, y, biomass))
                {
                    float edgeStrength = Mathf.SmoothStep(0.08f, 0.55f, biomass) * health;
                    result = Color.Lerp(result, new Color(0.90f, 1f, 0.66f), edgeStrength * 0.72f);
                }

                float healthStress = Mathf.Clamp01((0.58f - health) / 0.58f);
                float heatStress = Mathf.InverseLerp(32f, 38f, snapshot.Temperature);
                float dryStress = Mathf.InverseLerp(0.38f, 0.18f, moisture);
                float stress = Mathf.Max(healthStress, Mathf.Max(heatStress, dryStress));
                if (biomass > 0.10f && stress > 0f)
                {
                    float pattern = StressPattern(x, y, snapshot.Temperature, moisture);
                    result = Color.Lerp(result, new Color(0.24f, 0.10f, 0.08f), stress * pattern * 0.62f);
                }

                float glassHighlight = GlassHighlight(dishX, dishY) * 0.16f;
                result = Color.Lerp(result, Color.white, glassHighlight);
                result.a = 1f;
                pixels[i] = result;
            }
            texture.SetPixels32(pixels);
            texture.Apply(false);
        }

        private static bool IsGrowthEdge(float[] biomass, int x, int y, float value)
        {
            return IsLower(biomass, x - 1, y, value) ||
                   IsLower(biomass, x + 1, y, value) ||
                   IsLower(biomass, x, y - 1, value) ||
                   IsLower(biomass, x, y + 1, value);
        }

        private static bool IsLower(float[] biomass, int x, int y, float value)
        {
            if (x < 0 || x >= PetriSimulation.GridWidth || y < 0 || y >= PetriSimulation.GridHeight)
                return true;

            return biomass[y * PetriSimulation.GridWidth + x] < value * 0.62f;
        }

        private static float StressPattern(int x, int y, float temperature, float moisture)
        {
            float heatBands = temperature > 34f && (x + y) % 7 < 2 ? 1f : 0.25f;
            int hash = unchecked(x * 73856093 ^ y * 19349663);
            float drySpeckle = moisture < 0.35f && (hash & 7) == 0 ? 1f : 0.15f;
            return Mathf.Max(heatBands, drySpeckle);
        }

        private static float ColonyTexture(int x, int y)
        {
            int hash = unchecked(x * 374761393 + y * 668265263);
            hash = unchecked((hash ^ (hash >> 13)) * 1274126177);
            float noise = (hash & 1023) / 1023f;
            float broadVariation = Mathf.Sin(x * 0.43f + y * 0.27f) * 0.5f + 0.5f;
            return Mathf.Lerp(noise, broadVariation, 0.58f);
        }

        private static float GlassHighlight(float x, float y)
        {
            float diagonal = Mathf.Abs((y - x) - 0.58f);
            float upperLeft = Mathf.Clamp01((-x + y + 0.35f) * 0.7f);
            float band = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(diagonal / 0.16f));
            return band * upperLeft;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            RectTransform rect = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector2 local))
                return;

            Rect bounds = rect.rect;
            if (bounds.width <= 0f || bounds.height <= 0f) return;

            var normalized = new Vector2(
                Mathf.InverseLerp(bounds.xMin, bounds.xMax, local.x),
                Mathf.InverseLerp(bounds.yMin, bounds.yMax, local.y));
            float interactiveRadius = AgarRadius * 0.5f;
            if ((normalized - new Vector2(0.5f, 0.5f)).sqrMagnitude >
                interactiveRadius * interactiveRadius)
                return;

            DishTapped?.Invoke(normalized);
        }
    }
}
