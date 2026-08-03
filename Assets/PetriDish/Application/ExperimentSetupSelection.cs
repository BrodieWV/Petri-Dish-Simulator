using System;
using PetriDish.Content;

namespace PetriDish.Application
{
    public sealed class ExperimentSetupSelection
    {
        private readonly SimulationDefinitionCatalog catalog;
        private int organismIndex;
        private int mediumIndex;

        public OrganismDefinition Organism => catalog.GetOrganismAt(organismIndex);
        public MediumDefinition Medium => catalog.GetMediumAt(mediumIndex);

        public ExperimentSetupSelection(
            SimulationDefinitionCatalog catalog,
            string organismId,
            string mediumId)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            catalog.ValidateOrThrow();
            organismIndex = FindOrganismIndex(organismId);
            mediumIndex = FindMediumIndex(mediumId);
        }

        public void SelectNextOrganism()
        {
            organismIndex = Wrap(organismIndex + 1, catalog.OrganismCount);
        }

        public void SelectPreviousOrganism()
        {
            organismIndex = Wrap(organismIndex - 1, catalog.OrganismCount);
        }

        public void SelectNextMedium()
        {
            mediumIndex = Wrap(mediumIndex + 1, catalog.MediumCount);
        }

        public void SelectPreviousMedium()
        {
            mediumIndex = Wrap(mediumIndex - 1, catalog.MediumCount);
        }

        private int FindOrganismIndex(string id)
        {
            catalog.ResolveOrganism(id);
            for (int i = 0; i < catalog.OrganismCount; i++)
            {
                if (string.Equals(catalog.GetOrganismAt(i).Id, id, StringComparison.Ordinal))
                    return i;
            }
            throw new DefinitionValidationException($"Organism definition '{id}' is not available.");
        }

        private int FindMediumIndex(string id)
        {
            catalog.ResolveMedium(id);
            for (int i = 0; i < catalog.MediumCount; i++)
            {
                if (string.Equals(catalog.GetMediumAt(i).Id, id, StringComparison.Ordinal))
                    return i;
            }
            throw new DefinitionValidationException($"Medium definition '{id}' is not available.");
        }

        private static int Wrap(int index, int count)
        {
            return (index % count + count) % count;
        }
    }
}
