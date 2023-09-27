using Random = UnityEngine.Random;
using System;

namespace GeorgeChew.UnityAssessment.Data
{
    [Serializable]
    public class ConsumptionData
    {
        private const float AverageEnergyPerUnit = 400f, AverageWaterPerUnit = 18f;

        public float energy;
        public float water;
        public float energyPerUnit;
        public float waterPerUnit;

        // generate random data based on number of residential units in the block
        public static ConsumptionData GenerateFromHdbData(float total_dwelling_units)
        {
            float energyPerUnit = AverageEnergyPerUnit * Random.Range(0.9f, 1.1f);
            float waterPerUnit = AverageWaterPerUnit * Random.Range(0.9f, 1.1f);

            return new()
            {
                energyPerUnit = energyPerUnit,
                waterPerUnit = waterPerUnit,
                energy = total_dwelling_units * energyPerUnit,
                water = total_dwelling_units * waterPerUnit,
            };
        }
    }
}