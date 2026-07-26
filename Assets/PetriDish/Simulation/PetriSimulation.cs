using System;
using UnityEngine;

namespace PetriDish.Simulation
{
    [Serializable]
    public sealed class CellState
    {
        public float moisture;
        public float nutrients;
        public float biomass;
        public float health;
        public float stress;
    }

    [Serializable]
    public sealed class SimulationSaveData
    {
        public int schemaVersion = 1;
        public int seed;
        public long tick;
        public float temperature;
        public float targetTemperature;
        public float elapsedSimSeconds;
        public CellState[] cells;
    }

    public readonly struct SimulationSnapshot
    {
        public readonly int Width;
        public readonly int Height;
        public readonly long Tick;
        public readonly float Temperature;
        public readonly float Coverage;
        public readonly float AverageHealth;
        public readonly float AverageMoisture;
        public readonly float AverageNutrients;
        public readonly float[] Biomass;
        public readonly float[] Health;
        public readonly float[] Moisture;

        public SimulationSnapshot(int width, int height, long tick, float temperature,
            float coverage, float averageHealth, float averageMoisture, float averageNutrients,
            float[] biomass, float[] health, float[] moisture)
        {
            Width = width;
            Height = height;
            Tick = tick;
            Temperature = temperature;
            Coverage = coverage;
            AverageHealth = averageHealth;
            AverageMoisture = averageMoisture;
            AverageNutrients = averageNutrients;
            Biomass = biomass;
            Health = health;
            Moisture = moisture;
        }
    }

    public sealed class PetriSimulation
    {
        public const int GridWidth = 48;
        public const int GridHeight = 48;
        public const float FixedStepSeconds = 0.25f;

        private readonly CellState[] cells = new CellState[GridWidth * GridHeight];
        private readonly int seed;
        private System.Random random;
        private long tick;
        private float elapsedSimSeconds;
        private float temperature = 21f;
        private float targetTemperature = 21f;

        public long Tick => tick;
        public float ElapsedSimSeconds => elapsedSimSeconds;
        public float Temperature => temperature;
        public float TargetTemperature => targetTemperature;

        public PetriSimulation(int seed)
        {
            this.seed = seed;
            Reset();
        }

        public void Reset()
        {
            random = new System.Random(seed);
            tick = 0;
            elapsedSimSeconds = 0f;
            temperature = 21f;
            targetTemperature = 21f;

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new CellState
                {
                    moisture = 0.72f,
                    nutrients = 1f,
                    biomass = 0f,
                    health = 1f,
                    stress = 0f
                };
            }

            SeedColony();
        }

        private void SeedColony()
        {
            int cx = GridWidth / 2;
            int cy = GridHeight / 2;
            for (int y = cy - 2; y <= cy + 2; y++)
            {
                for (int x = cx - 2; x <= cx + 2; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                    if (distance <= 2.3f)
                    {
                        cells[Index(x, y)].biomass = Mathf.Lerp(0.28f, 0.08f, distance / 2.3f);
                    }
                }
            }
        }

        public void SetTargetTemperature(float value)
        {
            targetTemperature = Mathf.Clamp(value, 8f, 42f);
        }

        public void AddMoisture(float amount)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                float noise = 0.85f + (float)random.NextDouble() * 0.3f;
                cells[i].moisture = Mathf.Clamp01(cells[i].moisture + amount * noise);
            }
        }

        public void Step()
        {
            temperature = Mathf.MoveTowards(temperature, targetTemperature, 0.18f);
            var nextBiomass = new float[cells.Length];

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    int index = Index(x, y);
                    CellState cell = cells[index];
                    float edge = Mathf.Min(Mathf.Min(x, GridWidth - 1 - x), Mathf.Min(y, GridHeight - 1 - y));
                    float edgeDrying = Mathf.Lerp(0.0017f, 0.00045f, Mathf.Clamp01(edge / 12f));
                    float heatDrying = Mathf.Max(0f, temperature - 24f) * 0.00012f;
                    cell.moisture = Mathf.Clamp01(cell.moisture - edgeDrying - heatDrying);

                    float temperatureSuitability = BellSuitability(temperature, 26f, 7.5f);
                    float moistureSuitability = BellSuitability(cell.moisture, 0.70f, 0.35f);
                    float nutrientSuitability = Mathf.Clamp01(cell.nutrients / 0.25f);
                    float suitability = Mathf.Min(temperatureSuitability, moistureSuitability, nutrientSuitability);

                    bool lethalTemperature = temperature < 11f || temperature > 38f;
                    bool lethalMoisture = cell.moisture < 0.16f;
                    float desiredStress = 1f - suitability;
                    if (lethalTemperature || lethalMoisture) desiredStress = 1f;
                    cell.stress = Mathf.MoveTowards(cell.stress, desiredStress, lethalTemperature || lethalMoisture ? 0.06f : 0.025f);
                    cell.health = Mathf.Clamp01(cell.health + (suitability > 0.55f ? 0.015f : -0.018f * (0.35f + cell.stress)));

                    float carryingRoom = Mathf.Clamp01(1f - cell.biomass);
                    float growth = cell.biomass * 0.07f * suitability * cell.health * carryingRoom;
                    float death = cell.biomass * (lethalTemperature || lethalMoisture ? 0.035f : 0.004f * cell.stress);
                    cell.nutrients = Mathf.Clamp01(cell.nutrients - growth * 0.42f);
                    cell.moisture = Mathf.Clamp01(cell.moisture - growth * 0.025f);
                    nextBiomass[index] += Mathf.Max(0f, cell.biomass + growth - death);

                    if (cell.biomass > 0.035f && suitability > 0.28f)
                    {
                        SpreadToNeighbours(x, y, cell.biomass * 0.006f * suitability, nextBiomass);
                    }
                }
            }

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i].biomass = Mathf.Clamp01(nextBiomass[i]);
            }

            tick++;
            elapsedSimSeconds += FixedStepSeconds;
        }

        private void SpreadToNeighbours(int x, int y, float amount, float[] destination)
        {
            for (int oy = -1; oy <= 1; oy++)
            {
                for (int ox = -1; ox <= 1; ox++)
                {
                    if (ox == 0 && oy == 0) continue;
                    int nx = x + ox;
                    int ny = y + oy;
                    if (nx < 0 || nx >= GridWidth || ny < 0 || ny >= GridHeight) continue;
                    destination[Index(nx, ny)] += amount * (0.75f + (float)random.NextDouble() * 0.5f);
                }
            }
        }

        private static float BellSuitability(float value, float ideal, float halfRange)
        {
            float distance = Mathf.Abs(value - ideal) / Mathf.Max(0.001f, halfRange);
            return Mathf.Clamp01(1f - distance * distance);
        }

        public SimulationSnapshot CreateSnapshot()
        {
            var biomass = new float[cells.Length];
            var health = new float[cells.Length];
            var moisture = new float[cells.Length];
            float coverage = 0f;
            float totalHealth = 0f;
            float totalMoisture = 0f;
            float totalNutrients = 0f;

            for (int i = 0; i < cells.Length; i++)
            {
                biomass[i] = cells[i].biomass;
                health[i] = cells[i].health;
                moisture[i] = cells[i].moisture;
                if (cells[i].biomass > 0.06f) coverage += 1f;
                totalHealth += cells[i].health;
                totalMoisture += cells[i].moisture;
                totalNutrients += cells[i].nutrients;
            }

            return new SimulationSnapshot(GridWidth, GridHeight, tick, temperature,
                coverage / cells.Length, totalHealth / cells.Length, totalMoisture / cells.Length,
                totalNutrients / cells.Length, biomass, health, moisture);
        }

        public SimulationSaveData CaptureSave()
        {
            return new SimulationSaveData
            {
                seed = seed,
                tick = tick,
                temperature = temperature,
                targetTemperature = targetTemperature,
                elapsedSimSeconds = elapsedSimSeconds,
                cells = cells
            };
        }

        public void Restore(SimulationSaveData data)
        {
            if (data == null || data.cells == null || data.cells.Length != cells.Length)
                throw new ArgumentException("Invalid petri simulation save data.");

            tick = data.tick;
            temperature = data.temperature;
            targetTemperature = data.targetTemperature;
            elapsedSimSeconds = data.elapsedSimSeconds;
            random = new System.Random(seed ^ (int)tick);
            for (int i = 0; i < cells.Length; i++) cells[i] = data.cells[i];
        }

        private static int Index(int x, int y) => y * GridWidth + x;
    }
}
