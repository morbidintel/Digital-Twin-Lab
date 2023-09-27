using Gamelogic.Extensions;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace GeorgeChew.UnityAssessment.Heatmap
{
    using CityJsonEvents = EventMessaging.Registry.CityJson;

    /// <summary>
    ///
    /// </summary>
    public class HdbBlocksHeatmap : MonoBehaviour
    {
        public enum HeatmapMode
        { None, Energy, Water }

        [Header("Config")]
        [SerializeField] private Color energyMinColor;
        [SerializeField] private Color energyMaxColor;
        [SerializeField] private Color waterMinColor;
        [SerializeField] private Color waterMaxColor;

        // min max values for Energy and Water, to create our heatmap gradients
        [SerializeField, ReadOnly] private float energyMinValue;
        [SerializeField, ReadOnly] private float energyMaxValue;
        [SerializeField, ReadOnly] private float waterMinValue;
        [SerializeField, ReadOnly] private float waterMaxValue;

        public Color EnergyMinColor => energyMinColor;
        public Color EnergyMaxColor => energyMaxColor;
        public Color WaterMinColor => waterMinColor;
        public Color WaterMaxColor => waterMaxColor;
        public float EnergyMinValue => energyMinValue;
        public float EnergyMaxValue => energyMaxValue;
        public float WaterMinValue => waterMinValue;
        public float WaterMaxValue => waterMaxValue;

        private List<HdbBlockObject> hdbBlocks;

        // Start is called before the first frame update
        private void Start()
        {
            CityJsonEvents.OnLoadedAllHdbBlocks += OnCityJsonLoaded;
        }

        private void OnCityJsonLoaded(object obj)
        {
            hdbBlocks = obj as List<HdbBlockObject>;

            if (hdbBlocks == null || hdbBlocks.Count == 0)
            {
                Debug.Log("[HdbBlocksHeatmap] hdbBlocks null or empty!");
                return;
            }

            energyMinValue = hdbBlocks.Min(b => b.ConsumptionData.energyPerUnit);
            energyMaxValue = hdbBlocks.Max(b => b.ConsumptionData.energyPerUnit);
            waterMinValue = hdbBlocks.Min(b => b.ConsumptionData.waterPerUnit);
            waterMaxValue = hdbBlocks.Max(b => b.ConsumptionData.waterPerUnit);
        }

        public void ChangeHeatmapMode(HeatmapMode mode)
        {
            Action<HdbBlockObject> heatmapAction = null;

            // do the switch-case here, so we don't do it for every (9k+) block
            heatmapAction = mode switch
            {
                HeatmapMode.None => block => block.RemoveHeatmap(),
                HeatmapMode.Energy => block => block.ShowEnergyHeatmap(
                    energyMinValue, energyMaxValue, energyMinColor, energyMaxColor),
                HeatmapMode.Water => block => block.ShowWaterHeatmap(
                    waterMinValue, waterMaxValue, waterMinColor, waterMaxColor),
                _ => throw new NotImplementedException(),
            };

            foreach (var block in hdbBlocks)
            {
                heatmapAction(block);
            }
        }
    }
}