using System;

namespace PetriDish.Presentation.UI
{
    public sealed class LaboratoryDishViewData
    {
        public string Id { get; }
        public string Name { get; }
        public string OrganismName { get; }
        public string MediumName { get; }

        public LaboratoryDishViewData(string id, string name, string organismName, string mediumName)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Dish ID is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Dish name is required.", nameof(name));
            Id = id;
            Name = name;
            OrganismName = organismName ?? string.Empty;
            MediumName = mediumName ?? string.Empty;
        }
    }

    /// <summary>Replace this provider when authoritative multi-dish persistence is introduced.</summary>
    public interface ILaboratoryDishProvider
    {
        int Count { get; }
        LaboratoryDishViewData GetDish(int index);
    }

    public sealed class SingleLaboratoryDishProvider : ILaboratoryDishProvider
    {
        private readonly LaboratoryDishViewData dish;
        public int Count => 1;

        public SingleLaboratoryDishProvider()
            : this(new LaboratoryDishViewData(
                "current-experiment", "Dish A", "Bacillus subtilis", "Nutrient Agar"))
        {
        }

        public SingleLaboratoryDishProvider(LaboratoryDishViewData dish)
        {
            this.dish = dish ?? throw new ArgumentNullException(nameof(dish));
        }

        public LaboratoryDishViewData GetDish(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
            return dish;
        }
    }

    public sealed class LaboratoryHubDishSelection
    {
        private readonly ILaboratoryDishProvider provider;
        private int selectedIndex;

        public int Count => provider.Count;
        public int SelectedIndex => selectedIndex;
        public LaboratoryDishViewData SelectedDish => provider.GetDish(selectedIndex);
        public bool CanSelectPrevious => selectedIndex > 0;
        public bool CanSelectNext => selectedIndex + 1 < Count;
        public string PositionLabel => $"{SelectedDish.Name}     {selectedIndex + 1} / {Count}";

        public LaboratoryHubDishSelection(ILaboratoryDishProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            if (provider.Count < 1)
                throw new ArgumentException("The Laboratory Hub requires at least one real dish entry.", nameof(provider));
            for (int i = 0; i < provider.Count; i++)
                if (provider.GetDish(i) == null)
                    throw new ArgumentException($"Dish provider returned no dish at index {i}.", nameof(provider));
        }

        public bool SelectPrevious()
        {
            if (!CanSelectPrevious) return false;
            selectedIndex--;
            return true;
        }

        public bool SelectNext()
        {
            if (!CanSelectNext) return false;
            selectedIndex++;
            return true;
        }
    }
}
