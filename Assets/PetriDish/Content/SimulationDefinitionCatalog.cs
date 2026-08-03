using System;
using System.Collections.Generic;
using UnityEngine;

namespace PetriDish.Content
{
    [CreateAssetMenu(
        fileName = "SimulationDefinitionCatalog",
        menuName = "Petri Dish/Definitions/Simulation Catalog")]
    public sealed class SimulationDefinitionCatalog : ScriptableObject
    {
        public const string DefaultResourcePath = "PetriDish/DefaultSimulationDefinitionCatalog";
        public const string RapidBacteriumId = "rapid-bacterium";
        public const string NutrientAgarId = "nutrient-agar";
        public const string LowNutrientAgarId = "low-nutrient-agar";

        [SerializeField] private OrganismDefinition defaultOrganism;
        [SerializeField] private MediumDefinition defaultMedium;
        [SerializeField] private OrganismDefinition[] organisms;
        [SerializeField] private MediumDefinition[] media;

        public OrganismDefinition DefaultOrganism => defaultOrganism;
        public MediumDefinition DefaultMedium => defaultMedium;
        public int OrganismCount => organisms?.Length ?? 0;
        public int MediumCount => media?.Length ?? 0;

        public static SimulationDefinitionCatalog LoadDefaultOrThrow()
        {
            var catalog = Resources.Load<SimulationDefinitionCatalog>(DefaultResourcePath);
            if (catalog == null)
                throw new DefinitionValidationException(
                    $"Required definition catalog Resources/{DefaultResourcePath}.asset is missing.");
            catalog.ValidateOrThrow();
            return catalog;
        }

        public void ValidateOrThrow()
        {
            if (defaultOrganism == null)
                throw new DefinitionValidationException("The default organism definition is missing.");
            if (defaultMedium == null)
                throw new DefinitionValidationException("The default medium definition is missing.");

            Dictionary<string, OrganismDefinition> organismMap = BuildOrganismMap();
            Dictionary<string, MediumDefinition> mediumMap = BuildMediumMap();
            if (!organismMap.ContainsKey(defaultOrganism.Id))
                throw new DefinitionValidationException("The default organism is not present in the catalog.");
            if (!mediumMap.ContainsKey(defaultMedium.Id))
                throw new DefinitionValidationException("The default medium is not present in the catalog.");
        }

        public OrganismDefinition ResolveOrganism(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new DefinitionValidationException("An organism ID is required.");
            Dictionary<string, OrganismDefinition> map = BuildOrganismMap();
            if (!map.TryGetValue(id, out OrganismDefinition definition))
                throw new DefinitionValidationException($"Organism definition '{id}' is not available.");
            return definition;
        }

        public MediumDefinition ResolveMedium(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new DefinitionValidationException("A medium ID is required.");
            Dictionary<string, MediumDefinition> map = BuildMediumMap();
            if (!map.TryGetValue(id, out MediumDefinition definition))
                throw new DefinitionValidationException($"Medium definition '{id}' is not available.");
            return definition;
        }

        public OrganismDefinition GetOrganismAt(int index)
        {
            ValidateOrThrow();
            if (index < 0 || index >= organisms.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return organisms[index];
        }

        public MediumDefinition GetMediumAt(int index)
        {
            ValidateOrThrow();
            if (index < 0 || index >= media.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return media[index];
        }

        private Dictionary<string, OrganismDefinition> BuildOrganismMap()
        {
            if (organisms == null || organisms.Length == 0)
                throw new DefinitionValidationException("The organism definition list is empty.");

            var map = new Dictionary<string, OrganismDefinition>(StringComparer.Ordinal);
            for (int i = 0; i < organisms.Length; i++)
            {
                OrganismDefinition definition = organisms[i];
                if (definition == null)
                    throw new DefinitionValidationException($"Organism definition at index {i} is missing.");
                definition.ValidateOrThrow();
                if (!map.TryAdd(definition.Id, definition))
                    throw new DefinitionValidationException(
                        $"Duplicate organism ID '{definition.Id}' is not supported.");
            }

            return map;
        }

        private Dictionary<string, MediumDefinition> BuildMediumMap()
        {
            if (media == null || media.Length == 0)
                throw new DefinitionValidationException("The medium definition list is empty.");

            var map = new Dictionary<string, MediumDefinition>(StringComparer.Ordinal);
            for (int i = 0; i < media.Length; i++)
            {
                MediumDefinition definition = media[i];
                if (definition == null)
                    throw new DefinitionValidationException($"Medium definition at index {i} is missing.");
                definition.ValidateOrThrow();
                if (!map.TryAdd(definition.Id, definition))
                    throw new DefinitionValidationException(
                        $"Duplicate medium ID '{definition.Id}' is not supported.");
            }

            return map;
        }
    }
}
