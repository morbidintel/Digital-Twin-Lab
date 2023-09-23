using System;
using Random = UnityEngine.Random;

namespace GeorgeChew.HiverlabAssessment.Data
{
    [Serializable]
    public class ConsumptionData
    {
        private const float AverageEnergyPerUnit = 400f, AverageWaterPerUnit = 18f;

        public float energy;
        public float water;
        public float energyPerUnit;
        public float waterPerUnit;

        public static ConsumptionData GenerateFromHdbData(HdbBlockData hdbData)
        {
            int units = hdbData.total_dwelling_units;
            ConsumptionData consumption = new()
            {
                energyPerUnit = AverageEnergyPerUnit * Random.Range(0.9f, 1.1f),
                waterPerUnit = AverageWaterPerUnit * Random.Range(0.9f, 1.1f),
            };
            consumption.energy = units * consumption.energyPerUnit;
            consumption.water = units * consumption.waterPerUnit;
            return consumption;
        }
    }
}