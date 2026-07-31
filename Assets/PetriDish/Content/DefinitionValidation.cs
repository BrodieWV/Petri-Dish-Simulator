using System;

namespace PetriDish.Content
{
    public sealed class DefinitionValidationException : InvalidOperationException
    {
        public DefinitionValidationException(string message) : base(message)
        {
        }
    }

    internal static class DefinitionValidation
    {
        public static void ValidateId(string id, string definitionType)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new DefinitionValidationException($"{definitionType} requires a stable ID.");
            if (id.Length > 64)
                throw new DefinitionValidationException($"{definitionType} ID '{id}' exceeds 64 characters.");
            if (!IsLowercaseSlug(id))
                throw new DefinitionValidationException(
                    $"{definitionType} ID '{id}' must be a lowercase ASCII slug.");
        }

        public static void Finite(float value, string id, string field)
        {
            Require(!float.IsNaN(value) && !float.IsInfinity(value), id, $"{field} must be finite.");
        }

        public static void NonNegative(float value, string id, string field)
        {
            Finite(value, id, field);
            Require(value >= 0f, id, $"{field} must be non-negative.");
        }

        public static void Positive(float value, string id, string field)
        {
            Finite(value, id, field);
            Require(value > 0f, id, $"{field} must be greater than zero.");
        }

        public static void Unit(float value, string id, string field)
        {
            Finite(value, id, field);
            Require(value >= 0f && value <= 1f, id, $"{field} must be between 0 and 1.");
        }

        public static void UnitPositive(float value, string id, string field)
        {
            Unit(value, id, field);
            Require(value > 0f, id, $"{field} must be greater than zero.");
        }

        public static void Range(float value, float minimum, float maximum, string id, string field)
        {
            Finite(value, id, field);
            Require(
                value >= minimum && value <= maximum,
                id,
                $"{field} must be between {minimum} and {maximum}.");
        }

        public static void Require(bool condition, string id, string message)
        {
            if (!condition)
                throw new DefinitionValidationException($"Definition '{id ?? "<missing>"}': {message}");
        }

        private static bool IsLowercaseSlug(string value)
        {
            bool previousWasHyphen = true;
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                bool isAlphaNumeric =
                    (character >= 'a' && character <= 'z') ||
                    (character >= '0' && character <= '9');
                if (isAlphaNumeric)
                {
                    previousWasHyphen = false;
                    continue;
                }

                if (character != '-' || previousWasHyphen) return false;
                previousWasHyphen = true;
            }

            return !previousWasHyphen;
        }
    }
}
