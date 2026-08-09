using UnityEngine;

namespace PetriDish.Presentation.UI
{
    public enum LaboratoryDishStatus { Growing, Healthy, Stressed, Paused, Severe }

    [CreateAssetMenu(fileName = "PetriDishUITheme", menuName = "Petri Dish/UI Theme")]
    public sealed class PetriDishUITheme : ScriptableObject
    {
        [Header("Surfaces")]
        public Color background = new Color(0.018f, 0.024f, 0.027f, 1f);
        public Color bench = new Color(0.026f, 0.035f, 0.038f, 1f);
        public Color panel = new Color(0.043f, 0.057f, 0.062f, 0.98f);
        public Color panelRaised = new Color(0.062f, 0.080f, 0.086f, 1f);
        public Color panelHover = new Color(0.085f, 0.112f, 0.119f, 1f);
        [Header("Signals")]
        public Color cyan = new Color(0.20f, 0.82f, 0.88f, 1f);
        public Color green = new Color(0.28f, 0.75f, 0.45f, 1f);
        public Color amber = new Color(0.95f, 0.65f, 0.20f, 1f);
        public Color red = new Color(0.88f, 0.28f, 0.28f, 1f);
        [Header("Typography")]
        public Color textPrimary = new Color(0.92f, 0.96f, 0.97f, 1f);
        public Color textSecondary = new Color(0.62f, 0.70f, 0.72f, 1f);
        public Color textDisabled = new Color(0.37f, 0.43f, 0.45f, 1f);
        [Header("Layout")]
        [Min(4f)] public float compactSpacing = 12f;
        [Min(4f)] public float standardSpacing = 18f;
        [Min(48f)] public float navigationWidth = 184f;
        [Min(48f)] public float compactNavigationWidth = 76f;
        [Min(240f)] public float activeDishesWidth = 330f;
        [Min(240f)] public float activityWidth = 320f;
        [Min(1.5f)] public float compactLandscapeAspect = 1.95f;
        [Min(800f)] public float compactLandscapeWidth = 1200f;

        public Color GetStatusColor(LaboratoryDishStatus status)
        {
            switch (status)
            {
                case LaboratoryDishStatus.Growing:
                case LaboratoryDishStatus.Healthy: return green;
                case LaboratoryDishStatus.Stressed: return amber;
                case LaboratoryDishStatus.Severe: return red;
                default: return textSecondary;
            }
        }
    }
}
