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

        public CellState Clone()
        {
            return new CellState
            {
                moisture = moisture,
                nutrients = nutrients,
                biomass = biomass,
                health = health,
                stress = stress
            };
        }
    }

    [Serializable]
    public sealed class SimulationSaveData
    {
        public int schemaVersion = 2;
        public int seed;
        public long tick;
        public float temperature;
        public float targetTemperature;
        public float elapsedSimSeconds;
        public uint randomState;
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
        public readonly float[] Nutrients;

        public SimulationSnapshot(int width, int height, long tick, float temperature,
            float coverage, float averageHealth, float averageMoisture, float averageNutrients,
            float[] biomass, float[] health, float[] moisture, float[] nutrients)
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
            Nutrients = nutrients;
        }
    }

    public readonly struct SimulationMetrics
    {
        public readonly float Temperature;
        public readonly float Coverage;
        public readonly float AverageHealth;
        public readonly float AverageMoisture;
        public readonly float AverageNutrients;

        public SimulationMetrics(float temperature, float coverage, float averageHealth,
            float averageMoisture, float averageNutrients)
        {
            Temperature = temperature;
            Coverage = coverage;
            AverageHealth = averageHealth;
            AverageMoisture = averageMoisture;
            AverageNutrients = averageNutrients;
        }
    }

    internal struct DeterministicRandom
    {
        private uint state;

        public uint State => state;

        public DeterministicRandom(int seed)
        {
            state = MixSeed(seed);
        }

        public DeterministicRandom(uint state)
        {
            this.state = state == 0u ? 0x6D2B79F5u : state;
        }

        public float NextFloat01()
        {
            uint value = NextUInt();
            return (value >> 8) * (1f / 16777216f);
        }

        private uint NextUInt()
        {
            uint value = state;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            state = value == 0u ? 0x6D2B79F5u : value;
            return state;
        }

        private static uint MixSeed(int seed)
        {
            uint value = unchecked((uint)seed) + 0x9E3779B9u;
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return value == 0u ? 0x6D2B79F5u : value;
        }
    }

    public sealed class PetriSimulation
    {
        public const int GridWidth = 48;
        public const int GridHeight = 48;
        public const float FixedStepSeconds = 0.25f;
        public const int CurrentSaveSchemaVersion = 2;

        private readonly CellState[] cells = new CellState[GridWidth * GridHeight];
        private readonly float[] nextBiomass = new float[GridWidth * GridHeight];
        private readonly int seed;
        private DeterministicRandom random;
        private long tick;
        private float elapsedSimSeconds;
        private float temperature = 21f;
        private float targetTemperature = 21f;

        public long Tick => tick;
        public int Seed => seed;
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
            random = new DeterministicRandom(seed);
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
            if (!IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value), "Temperature must be finite.");
            targetTemperature = Mathf.Clamp(value, 8f, 42f);
        }

        public void AddMoisture(float amount)
        {
            if (!IsFinite(amount) || amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), "Moisture amount must be finite and non-negative.");

            for (int i = 0; i < cells.Length; i++)
            {
                float noise = 0.85f + random.NextFloat01() * 0.3f;
                cells[i].moisture = Mathf.Clamp01(cells[i].moisture + amount * noise);
            }
        }

        public void Step()
        {
            temperature = Mathf.MoveTowards(temperature, targetTemperature, 0.18f);
            Array.Clear(nextBiomass, 0, nextBiomass.Length);

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
                    destination[Index(nx, ny)] += amount * (0.75f + random.NextFloat01() * 0.5f);
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
            var nutrients = new float[cells.Length];
            float coverage = 0f;
            float totalHealth = 0f;
            float totalMoisture = 0f;
            float totalNutrients = 0f;

            for (int i = 0; i < cells.Length; i++)
            {
                biomass[i] = cells[i].biomass;
                health[i] = cells[i].health;
                moisture[i] = cells[i].moisture;
                nutrients[i] = cells[i].nutrients;
                if (cells[i].biomass > 0.06f) coverage += 1f;
                totalHealth += cells[i].health;
                totalMoisture += cells[i].moisture;
                totalNutrients += cells[i].nutrients;
            }

            return new SimulationSnapshot(GridWidth, GridHeight, tick, temperature,
                coverage / cells.Length, totalHealth / cells.Length, totalMoisture / cells.Length,
                totalNutrients / cells.Length, biomass, health, moisture, nutrients);
        }

        public SimulationMetrics CreateMetrics()
        {
            float coverage = 0f;
            float totalHealth = 0f;
            float totalMoisture = 0f;
            float totalNutrients = 0f;

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i].biomass > 0.06f) coverage += 1f;
                totalHealth += cells[i].health;
                totalMoisture += cells[i].moisture;
                totalNutrients += cells[i].nutrients;
            }

            return new SimulationMetrics(
                temperature,
                coverage / cells.Length,
                totalHealth / cells.Length,
                totalMoisture / cells.Length,
                totalNutrients / cells.Length);
        }

        public SimulationSaveData CaptureSave()
        {
            var copiedCells = new CellState[cells.Length];
            for (int i = 0; i < cells.Length; i++) copiedCells[i] = cells[i].Clone();

            return new SimulationSaveData
            {
                schemaVersion = CurrentSaveSchemaVersion,
                seed = seed,
                tick = tick,
                temperature = temperature,
                targetTemperature = targetTemperature,
                elapsedSimSeconds = elapsedSimSeconds,
                randomState = random.State,
                cells = copiedCells
            };
        }

        public void Restore(SimulationSaveData data)
        {
            ValidateSave(data);

            tick = data.tick;
            temperature = Mathf.Clamp(data.temperature, 8f, 42f);
            targetTemperature = Mathf.Clamp(data.targetTemperature, 8f, 42f);
            elapsedSimSeconds = Mathf.Max(0f, data.elapsedSimSeconds);
            random = data.schemaVersion >= 2
                ? new DeterministicRandom(data.randomState)
                : new DeterministicRandom(seed ^ unchecked((int)tick));

            for (int i = 0; i < cells.Length; i++) cells[i] = data.cells[i].Clone();
        }

        private void ValidateSave(SimulationSaveData data)
        {
            if (data == null)
                throw new ArgumentException("Save data cannot be null.", nameof(data));
            if (data.schemaVersion < 1 || data.schemaVersion > CurrentSaveSchemaVersion)
                throw new ArgumentException($"Unsupported save schema version {data.schemaVersion}.", nameof(data));
            if (data.seed != seed)
                throw new ArgumentException("Save seed does not match this simulation instance.", nameof(data));
            if (data.cells == null || data.cells.Length != cells.Length)
                throw new ArgumentException("Save data has an invalid cell array.", nameof(data));
            if (data.tick < 0)
                throw new ArgumentException("Save data has a negative simulation tick.", nameof(data));
            if (!IsFinite(data.temperature) || !IsFinite(data.targetTemperature) ||
                !IsFinite(data.elapsedSimSeconds) || data.elapsedSimSeconds < 0f)
                throw new ArgumentException("Save data contains an invalid simulation value.", nameof(data));
            if (data.temperature < 8f || data.temperature > 42f ||
                data.targetTemperature < 8f || data.targetTemperature > 42f)
                throw new ArgumentException("Save data contains an unsupported temperature.", nameof(data));
            if (data.schemaVersion >= 2 && data.randomState == 0u)
                throw new ArgumentException("Save data contains an invalid random state.", nameof(data));

            for (int i = 0; i < data.cells.Length; i++)
            {
                if (data.cells[i] == null)
                    throw new ArgumentException($"Save data contains a null cell at index {i}.", nameof(data));
                CellState cell = data.cells[i];
                if (!IsUnitValue(cell.moisture) || !IsUnitValue(cell.nutrients) ||
                    !IsUnitValue(cell.biomass) || !IsUnitValue(cell.health) ||
                    !IsUnitValue(cell.stress))
                    throw new ArgumentException($"Save data contains an invalid cell at index {i}.", nameof(data));
            }
        }

        private static bool IsUnitValue(float value) => IsFinite(value) && value >= 0f && value <= 1f;

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static int Index(int x, int y) => y * GridWidth + x;
    }
}
