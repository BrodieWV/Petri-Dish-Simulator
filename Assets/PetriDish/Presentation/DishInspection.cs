using System;
using PetriDish.Simulation;

namespace PetriDish.Presentation
{
    public readonly struct CellInspection
    {
        public readonly int X;
        public readonly int Y;
        public readonly float Biomass;
        public readonly float Health;
        public readonly float Moisture;
        public readonly float Nutrients;
        public readonly string Condition;

        public CellInspection(int x, int y, float biomass, float health, float moisture, float nutrients, string condition)
        {
            X = x;
            Y = y;
            Biomass = biomass;
            Health = health;
            Moisture = moisture;
            Nutrients = nutrients;
            Condition = condition;
        }

        public string ToDisplayText()
        {
            return $"Cell {X + 1}, {Y + 1} — {Condition}\n" +
                   $"Biomass {Biomass * 100f:0}% • Health {Health * 100f:0}%\n" +
                   $"Moisture {Moisture * 100f:0}% • Nutrients {Nutrients * 100f:0}%";
        }
    }

    public static class DishInspection
    {
        public static bool TryMapNormalizedPoint(float normalizedX, float normalizedY, int width, int height, out int x, out int y)
        {
            x = -1;
            y = -1;
            if (width <= 0 || height <= 0) return false;
            if (normalizedX < 0f || normalizedX > 1f || normalizedY < 0f || normalizedY > 1f) return false;

            x = Math.Min(width - 1, (int)(normalizedX * width));
            y = Math.Min(height - 1, (int)(normalizedY * height));
            return true;
        }

        public static bool TryInspect(
            SimulationSnapshot snapshot,
            SimulationSaveData saveData,
            float normalizedX,
            float normalizedY,
            out CellInspection inspection)
        {
            inspection = default;
            if (!TryMapNormalizedPoint(normalizedX, normalizedY, snapshot.Width, snapshot.Height, out int x, out int y))
                return false;

            return TryInspect(snapshot, saveData, x, y, out inspection);
        }

        public static bool TryInspect(
            SimulationSnapshot snapshot,
            SimulationSaveData saveData,
            int x,
            int y,
            out CellInspection inspection)
        {
            inspection = default;
            if (x < 0 || x >= snapshot.Width || y < 0 || y >= snapshot.Height) return false;

            int expectedLength = snapshot.Width * snapshot.Height;
            if (snapshot.Biomass == null || snapshot.Health == null || snapshot.Moisture == null) return false;
            if (snapshot.Biomass.Length != expectedLength || snapshot.Health.Length != expectedLength || snapshot.Moisture.Length != expectedLength)
                return false;
            if (saveData?.cells == null || saveData.cells.Length != expectedLength) return false;

            int index = y * snapshot.Width + x;
            CellState cell = saveData.cells[index];
            if (cell == null) return false;

            float biomass = snapshot.Biomass[index];
            float health = snapshot.Health[index];
            float moisture = snapshot.Moisture[index];
            float nutrients = cell.nutrients;
            string condition = GetCondition(snapshot.Temperature, biomass, health, moisture, nutrients);
            inspection = new CellInspection(x, y, biomass, health, moisture, nutrients, condition);
            return true;
        }

        public static string GetCondition(float temperature, float biomass, float health, float moisture, float nutrients)
        {
            if (biomass < 0.01f) return "No visible colony";
            if (temperature > 38f || temperature < 11f) return "Lethal temperature";
            if (moisture < 0.16f) return "Critically dry";
            if (nutrients < 0.12f) return "Nutrients exhausted";
            if (health < 0.35f) return "Colony declining";
            if (moisture < 0.30f) return "Drying out";
            if (nutrients < 0.25f) return "Nutrient limited";
            if (health > 0.75f && temperature >= 24f && temperature <= 29f) return "Healthy growth";
            return "Stable";
        }
    }
}
