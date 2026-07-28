using System;
using PetriDish.Content;
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
        public int schemaVersion = 3;
        public int seed;
        public string organismId;
        public int organismDefinitionVersion;
        public string mediumId;
        public int mediumDefinitionVersion;
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
        public const int CurrentSaveSchemaVersion = 3;

        private const float DishRadiusNormalized = 0.87f;
        private static readonly bool[] DishMask = CreateDishMask();
        private static readonly float[] DishEdgeDistance = CreateDishEdgeDistance();
        private static readonly int DishCellCount = CountDishCells();

        private readonly CellState[] cells = new CellState[GridWidth * GridHeight];
        private readonly float[] nextBiomass = new float[GridWidth * GridHeight];
        private readonly int seed;
        private readonly OrganismSimulationValues organism;
        private readonly MediumSimulationValues medium;
        private DeterministicRandom random;
        private long tick;
        private float elapsedSimSeconds;
        private float temperature = 21f;
        private float targetTemperature = 21f;

        public long Tick => tick;
        public int Seed => seed;
        public string OrganismId => organism.Id;
        public int OrganismDefinitionVersion => organism.DefinitionVersion;
        public string MediumId => medium.Id;
        public int MediumDefinitionVersion => medium.DefinitionVersion;
        public float ElapsedSimSeconds => elapsedSimSeconds;
        public float Temperature => temperature;
        public float TargetTemperature => targetTemperature;

        public PetriSimulation(
            int seed,
            OrganismDefinition organismDefinition,
            MediumDefinition mediumDefinition)
        {
            if (organismDefinition == null)
                throw new ArgumentNullException(nameof(organismDefinition));
            if (mediumDefinition == null)
                throw new ArgumentNullException(nameof(mediumDefinition));

            this.seed = seed;
            organism = organismDefinition.ToSimulationValues();
            medium = mediumDefinition.ToSimulationValues();
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
                bool isDishCell = DishMask[i];
                cells[i] = new CellState
                {
                    moisture = isDishCell ? medium.InitialMoisture : 0f,
                    nutrients = isDishCell ? medium.InitialNutrients : 0f,
                    biomass = 0f,
                    health = isDishCell ? organism.InitialHealth : 0f,
                    stress = 0f
                };
            }

            SeedColony();
        }

        private void SeedColony()
        {
            int cx = GridWidth / 2;
            int cy = GridHeight / 2;
            int seedExtent = Mathf.CeilToInt(organism.SeedRadiusCells);
            for (int y = Mathf.Max(0, cy - seedExtent);
                 y <= Mathf.Min(GridHeight - 1, cy + seedExtent);
                 y++)
            {
                for (int x = Mathf.Max(0, cx - seedExtent);
                     x <= Mathf.Min(GridWidth - 1, cx + seedExtent);
                     x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                    if (distance <= organism.SeedRadiusCells)
                    {
                        cells[Index(x, y)].biomass = Mathf.Lerp(
                            organism.SeedCenterBiomass,
                            organism.SeedEdgeBiomass,
                            distance / organism.SeedRadiusCells);
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
                if (!DishMask[i]) continue;
                float variance = medium.MoistureApplicationVariance;
                float noise = (1f - variance) + random.NextFloat01() * (variance * 2f);
                cells[i].moisture = Mathf.Clamp01(
                    cells[i].moisture + amount * medium.MoistureAbsorptionMultiplier * noise);
            }
        }

        public void Step()
        {
            temperature = Mathf.MoveTowards(
                temperature,
                targetTemperature,
                medium.TemperatureResponseRate);
            Array.Clear(nextBiomass, 0, nextBiomass.Length);

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    int index = Index(x, y);
                    if (!DishMask[index]) continue;

                    CellState cell = cells[index];
                    float edge = DishEdgeDistance[index];
                    float edgeDrying = Mathf.Lerp(
                        medium.EdgeEvaporation,
                        medium.InteriorEvaporation,
                        Mathf.Clamp01(edge / medium.EdgeFalloffDepthCells));
                    float heatDrying = Mathf.Max(
                        0f,
                        temperature - medium.HeatEvaporationStartTemperature) *
                        medium.HeatEvaporationPerDegree;
                    cell.moisture = Mathf.Clamp01(cell.moisture - edgeDrying - heatDrying);

                    float temperatureSuitability = BellSuitability(
                        temperature,
                        organism.PreferredTemperature,
                        organism.TemperatureHalfRange);
                    float moistureSuitability = BellSuitability(
                        cell.moisture,
                        organism.PreferredMoisture,
                        organism.MoistureHalfRange);
                    float nutrientSuitability = Mathf.Clamp01(
                        cell.nutrients / organism.NutrientsForFullSuitability);
                    float suitability = Mathf.Min(temperatureSuitability, moistureSuitability, nutrientSuitability);

                    bool lethalTemperature =
                        temperature < organism.LethalTemperatureMinimum ||
                        temperature > organism.LethalTemperatureMaximum;
                    bool lethalMoisture = cell.moisture < organism.LethalMoistureMinimum;
                    float desiredStress = 1f - suitability;
                    if (lethalTemperature || lethalMoisture) desiredStress = 1f;
                    cell.stress = Mathf.MoveTowards(
                        cell.stress,
                        desiredStress,
                        lethalTemperature || lethalMoisture
                            ? organism.LethalStressResponseRate
                            : organism.NormalStressResponseRate);
                    cell.health = Mathf.Clamp01(
                        cell.health +
                        (suitability > organism.HealthySuitabilityThreshold
                            ? organism.HealthRecoveryRate
                            : -organism.HealthDeclineRate *
                              (organism.HealthDeclineStressFloor + cell.stress)));

                    float carryingRoom = Mathf.Clamp01(1f - cell.biomass);
                    float growth =
                        cell.biomass * organism.GrowthRate * suitability * cell.health * carryingRoom;
                    float death = cell.biomass *
                        (lethalTemperature || lethalMoisture
                            ? organism.LethalDeathRate
                            : organism.StressDeathRate * cell.stress);
                    cell.nutrients = Mathf.Clamp01(
                        cell.nutrients - growth * organism.NutrientConsumptionPerGrowth);
                    cell.moisture = Mathf.Clamp01(
                        cell.moisture - growth * organism.MoistureConsumptionPerGrowth);
                    nextBiomass[index] += Mathf.Max(0f, cell.biomass + growth - death);

                    if (cell.biomass > organism.SpreadMinimumBiomass &&
                        suitability > organism.SpreadMinimumSuitability)
                    {
                        SpreadToNeighbours(
                            x,
                            y,
                            cell.biomass * organism.SpreadRate * suitability,
                            nextBiomass);
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
                    int neighbourIndex = Index(nx, ny);
                    if (!DishMask[neighbourIndex]) continue;
                    destination[neighbourIndex] += amount * Mathf.Lerp(
                        organism.SpreadRandomMinimum,
                        organism.SpreadRandomMaximum,
                        random.NextFloat01());
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
                if (!DishMask[i])
                {
                    biomass[i] = 0f;
                    health[i] = 0f;
                    moisture[i] = 0f;
                    nutrients[i] = 0f;
                    continue;
                }

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
                coverage / DishCellCount, totalHealth / DishCellCount, totalMoisture / DishCellCount,
                totalNutrients / DishCellCount, biomass, health, moisture, nutrients);
        }

        public SimulationMetrics CreateMetrics()
        {
            float coverage = 0f;
            float totalHealth = 0f;
            float totalMoisture = 0f;
            float totalNutrients = 0f;

            for (int i = 0; i < cells.Length; i++)
            {
                if (!DishMask[i]) continue;
                if (cells[i].biomass > 0.06f) coverage += 1f;
                totalHealth += cells[i].health;
                totalMoisture += cells[i].moisture;
                totalNutrients += cells[i].nutrients;
            }

            return new SimulationMetrics(
                temperature,
                coverage / DishCellCount,
                totalHealth / DishCellCount,
                totalMoisture / DishCellCount,
                totalNutrients / DishCellCount);
        }

        public SimulationSaveData CaptureSave()
        {
            var copiedCells = new CellState[cells.Length];
            for (int i = 0; i < cells.Length; i++) copiedCells[i] = cells[i].Clone();

            return new SimulationSaveData
            {
                schemaVersion = CurrentSaveSchemaVersion,
                seed = seed,
                organismId = organism.Id,
                organismDefinitionVersion = organism.DefinitionVersion,
                mediumId = medium.Id,
                mediumDefinitionVersion = medium.DefinitionVersion,
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

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = data.cells[i].Clone();
                if (!DishMask[i])
                {
                    cells[i].moisture = 0f;
                    cells[i].nutrients = 0f;
                    cells[i].biomass = 0f;
                    cells[i].health = 0f;
                    cells[i].stress = 0f;
                }
            }
        }

        private void ValidateSave(SimulationSaveData data)
        {
            if (data == null)
                throw new ArgumentException("Save data cannot be null.", nameof(data));
            if (data.schemaVersion < 1 || data.schemaVersion > CurrentSaveSchemaVersion)
                throw new ArgumentException($"Unsupported save schema version {data.schemaVersion}.", nameof(data));
            if (data.seed != seed)
                throw new ArgumentException("Save seed does not match this simulation instance.", nameof(data));
            ValidateDefinitionSelection(data);
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

        private void ValidateDefinitionSelection(SimulationSaveData data)
        {
            if (data.schemaVersion <= 2)
            {
                if (organism.Id != SimulationDefinitionCatalog.RapidBacteriumId ||
                    medium.Id != SimulationDefinitionCatalog.NutrientAgarId ||
                    organism.DefinitionVersion != 1 ||
                    medium.DefinitionVersion != 1)
                    throw new ArgumentException(
                        "Schema-version-2 simulations can only migrate to definition-version-1 " +
                        "Rapid Bacterium on definition-version-1 Nutrient Agar.",
                        nameof(data));
                return;
            }

            if (!string.Equals(data.organismId, organism.Id, StringComparison.Ordinal))
                throw new ArgumentException(
                    $"Save organism '{data.organismId}' does not match '{organism.Id}'.",
                    nameof(data));
            if (data.organismDefinitionVersion != organism.DefinitionVersion)
                throw new ArgumentException(
                    $"Save organism definition version {data.organismDefinitionVersion} is not supported.",
                    nameof(data));
            if (!string.Equals(data.mediumId, medium.Id, StringComparison.Ordinal))
                throw new ArgumentException(
                    $"Save medium '{data.mediumId}' does not match '{medium.Id}'.",
                    nameof(data));
            if (data.mediumDefinitionVersion != medium.DefinitionVersion)
                throw new ArgumentException(
                    $"Save medium definition version {data.mediumDefinitionVersion} is not supported.",
                    nameof(data));
        }

        private static bool IsUnitValue(float value) => IsFinite(value) && value >= 0f && value <= 1f;

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static bool[] CreateDishMask()
        {
            var mask = new bool[GridWidth * GridHeight];
            float centerX = (GridWidth - 1) * 0.5f;
            float centerY = (GridHeight - 1) * 0.5f;
            float radius = Mathf.Min(GridWidth, GridHeight) * 0.5f * DishRadiusNormalized;
            float radiusSquared = radius * radius;

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    mask[Index(x, y)] = dx * dx + dy * dy <= radiusSquared;
                }
            }

            return mask;
        }

        private static float[] CreateDishEdgeDistance()
        {
            var distances = new float[GridWidth * GridHeight];
            float centerX = (GridWidth - 1) * 0.5f;
            float centerY = (GridHeight - 1) * 0.5f;
            float radius = Mathf.Min(GridWidth, GridHeight) * 0.5f * DishRadiusNormalized;

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    distances[Index(x, y)] = Mathf.Max(0f, radius - Mathf.Sqrt(dx * dx + dy * dy));
                }
            }

            return distances;
        }

        private static int CountDishCells()
        {
            int count = 0;
            for (int i = 0; i < DishMask.Length; i++)
            {
                if (DishMask[i]) count++;
            }

            return count;
        }

        private static int Index(int x, int y) => y * GridWidth + x;
    }
}
