namespace PetriDish.Application
{
    public static class SimulationSpeedCycle
    {
        public static float Next(float currentSpeed)
        {
            if (currentSpeed < 1.5f) return 2f;
            if (currentSpeed < 3f) return 4f;
            return 1f;
        }

        public static string Label(float speed)
        {
            return $"Speed {speed:0.#}×";
        }
    }
}
