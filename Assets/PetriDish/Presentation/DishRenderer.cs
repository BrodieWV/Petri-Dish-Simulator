using PetriDish.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace PetriDish.Presentation
{
    [RequireComponent(typeof(RawImage))]
    public sealed class DishRenderer : MonoBehaviour
    {
        private RawImage target;
        private Texture2D texture;

        private void Awake()
        {
            target = GetComponent<RawImage>();
            texture = new Texture2D(PetriSimulation.GridWidth, PetriSimulation.GridHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            target.texture = texture;
        }

        public void Render(SimulationSnapshot snapshot)
        {
            var pixels = new Color32[snapshot.Biomass.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                float biomass = Mathf.Clamp01(snapshot.Biomass[i]);
                float health = Mathf.Clamp01(snapshot.Health[i]);
                float moisture = Mathf.Clamp01(snapshot.Moisture[i]);
                Color medium = Color.Lerp(new Color(0.27f, 0.20f, 0.10f), new Color(0.20f, 0.39f, 0.32f), moisture);
                Color colony = Color.Lerp(new Color(0.30f, 0.20f, 0.12f), new Color(0.70f, 0.93f, 0.58f), health);
                pixels[i] = Color.Lerp(medium, colony, Mathf.SmoothStep(0f, 1f, biomass));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false);
        }
    }
}
