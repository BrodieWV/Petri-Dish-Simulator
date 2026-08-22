using System;
using UnityEngine.SceneManagement;

namespace PetriDish.Presentation.UI
{
    public enum LaboratoryHubAction
    {
        Lab,
        NewExperiment,
        OpenDish,
        Compare,
        Journal,
        Collection,
        Challenges,
        Settings
    }

    public enum ExperimentEntryIntent
    {
        OpenSelectedDish,
        NewExperimentSetup
    }

    public readonly struct ExperimentEntryRequest
    {
        public ExperimentEntryIntent Intent { get; }
        public string DishId { get; }

        public ExperimentEntryRequest(ExperimentEntryIntent intent, string dishId)
        {
            Intent = intent;
            DishId = dishId;
        }
    }

    /// <summary>
    /// Carries presentation-only entry intent across the Hub/experiment scene boundary.
    /// The experiment remains authoritative for simulation and save state.
    /// </summary>
    public static class LaboratoryHubExperimentEntry
    {
        private static ExperimentEntryRequest? pending;

        public static bool HasPendingRequest => pending.HasValue;

        public static void RequestOpenDish(string dishId)
        {
            if (string.IsNullOrWhiteSpace(dishId))
                throw new ArgumentException("A selected dish ID is required.", nameof(dishId));
            pending = new ExperimentEntryRequest(ExperimentEntryIntent.OpenSelectedDish, dishId);
        }

        public static void RequestNewExperiment() =>
            pending = new ExperimentEntryRequest(ExperimentEntryIntent.NewExperimentSetup, null);

        public static bool TryConsume(out ExperimentEntryRequest request)
        {
            if (!pending.HasValue)
            {
                request = default;
                return false;
            }

            request = pending.Value;
            pending = null;
            return true;
        }

        public static void Clear() => pending = null;
    }

    public readonly struct LaboratoryHubNavigationContext
    {
        public LaboratoryDishViewData SelectedDish { get; }
        public int DishCount { get; }

        public LaboratoryHubNavigationContext(LaboratoryDishViewData selectedDish, int dishCount)
        {
            SelectedDish = selectedDish;
            DishCount = dishCount;
        }
    }

    public readonly struct LaboratoryHubNavigationResult
    {
        public bool Succeeded { get; }
        public string Message { get; }

        private LaboratoryHubNavigationResult(bool succeeded, string message)
        {
            Succeeded = succeeded;
            Message = message;
        }

        public static LaboratoryHubNavigationResult Success(string message = null) =>
            new LaboratoryHubNavigationResult(true, message);

        public static LaboratoryHubNavigationResult Unavailable(string message) =>
            new LaboratoryHubNavigationResult(false, message);
    }

    public interface ILaboratoryHubNavigator
    {
        LaboratoryHubNavigationResult Navigate(
            LaboratoryHubAction action,
            LaboratoryHubNavigationContext context);
    }

    /// <summary>
    /// Unity scene adapter for Hub navigation. Simulation setup is deliberately not duplicated here.
    /// </summary>
    public sealed class UnityLaboratoryHubNavigator : ILaboratoryHubNavigator
    {
        public const string HubSceneName = "LaboratoryHub";
        public const string ExperimentSceneName = "PetriDishVerticalSlice";

        public LaboratoryHubNavigationResult Navigate(
            LaboratoryHubAction action,
            LaboratoryHubNavigationContext context)
        {
            switch (action)
            {
                case LaboratoryHubAction.Lab:
                    if (SceneManager.GetActiveScene().name == HubSceneName)
                        return LaboratoryHubNavigationResult.Success("Laboratory Hub is already open.");
                    SceneManager.LoadScene(HubSceneName);
                    return LaboratoryHubNavigationResult.Success();

                case LaboratoryHubAction.NewExperiment:
                    LaboratoryHubExperimentEntry.RequestNewExperiment();
                    SceneManager.LoadScene(ExperimentSceneName);
                    return LaboratoryHubNavigationResult.Success();

                case LaboratoryHubAction.OpenDish:
                    if (context.SelectedDish == null)
                        return LaboratoryHubNavigationResult.Unavailable("No dish is available to open.");
                    LaboratoryHubExperimentEntry.RequestOpenDish(context.SelectedDish.Id);
                    SceneManager.LoadScene(ExperimentSceneName);
                    return LaboratoryHubNavigationResult.Success();

                case LaboratoryHubAction.Compare:
                    return context.DishCount < 2
                        ? LaboratoryHubNavigationResult.Unavailable("Comparison requires at least two dishes.")
                        : LaboratoryHubNavigationResult.Unavailable("Comparison is not available in this milestone.");
                case LaboratoryHubAction.Journal:
                    return LaboratoryHubNavigationResult.Unavailable("Journal is planned for a later milestone.");
                case LaboratoryHubAction.Collection:
                    return LaboratoryHubNavigationResult.Unavailable("Collection is planned for a later milestone.");
                case LaboratoryHubAction.Challenges:
                    return LaboratoryHubNavigationResult.Unavailable("Challenges are planned for a later milestone.");
                case LaboratoryHubAction.Settings:
                    return LaboratoryHubNavigationResult.Unavailable("Settings are not available yet.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }
    }
}
