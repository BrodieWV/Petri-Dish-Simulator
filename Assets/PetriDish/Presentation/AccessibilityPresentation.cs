using PetriDish.Simulation;

namespace PetriDish.Presentation
{
    public enum SimulationCondition
    {
        Stable,
        SlowGrowth,
        HeatStress,
        Dry,
        NutrientLimited,
        Declining
    }

    public static class AccessibilityPresentation
    {
        public static SimulationCondition GetCondition(SimulationSnapshot snapshot)
        {
            if (snapshot.Temperature > 34f) return SimulationCondition.HeatStress;
            if (snapshot.AverageMoisture < 0.30f) return SimulationCondition.Dry;
            if (snapshot.AverageNutrients < 0.18f) return SimulationCondition.NutrientLimited;
            if (snapshot.AverageHealth < 0.45f) return SimulationCondition.Declining;
            if (snapshot.Temperature >= 24f && snapshot.Temperature <= 29f) return SimulationCondition.Stable;
            return SimulationCondition.SlowGrowth;
        }

        public static string ConditionLabel(SimulationCondition condition)
        {
            switch (condition)
            {
                case SimulationCondition.Stable: return "OK — Growing well";
                case SimulationCondition.SlowGrowth: return "INFO — Growing slowly";
                case SimulationCondition.HeatStress: return "WARNING — Heat stressed";
                case SimulationCondition.Dry: return "WARNING — Too dry";
                case SimulationCondition.NutrientLimited: return "WARNING — Nutrient limited";
                case SimulationCondition.Declining: return "ALERT — Colony declining";
                default: return "INFO — Status unavailable";
            }
        }

        public static string PauseButtonLabel(bool isPaused)
        {
            return isPaused ? "Resume" : "Pause";
        }

        public static string SimulationStateLabel(bool isPaused, float speed)
        {
            return isPaused ? "Simulation paused" : $"Simulation running at {speed:0.#}×";
        }
    }
}
