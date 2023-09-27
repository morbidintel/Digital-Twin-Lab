using System.Linq;
using System;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine;

namespace GeorgeChew.UnityAssessment.Heatmap
{
    /// <summary>
    /// Facilitates changing the HDB Blocks heatmap with a Dropdown
    /// </summary>
    public class ChooseConsumptionHeatmap : MonoBehaviour
    {
        [Header("Script References")]
        [SerializeField] private HdbBlocksHeatmap heatmap;

        [Header("UI References")]
        [SerializeField] private Dropdown dropdown;
        [SerializeField] private GameObject legendParent;
        [SerializeField] private Text legendSubtitle;
        [SerializeField] private Text minValue;
        [SerializeField] private Text maxValue;
        [SerializeField] private UIGradient legendGradient;
        [SerializeField] private Image minLine;
        [SerializeField] private Image maxLine;

        private void Awake()
        {
            Assert.IsNotNull(heatmap);
            Assert.IsNotNull(dropdown);
            Assert.IsNotNull(legendParent);
            Assert.IsNotNull(legendSubtitle);
            Assert.IsNotNull(minValue);
            Assert.IsNotNull(maxValue);
            Assert.IsNotNull(legendGradient);
            Assert.IsNotNull(minLine);
            Assert.IsNotNull(maxLine);
        }

        private void Start()
        {
            PopulateDropdown();

            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);

            // populate the gradients in the legends
            legendGradient.m_color1 = heatmap.EnergyMinColor;
            legendGradient.m_color2 = heatmap.EnergyMaxColor;
        }

        private void PopulateDropdown()
        {
            var options = Enum.GetNames(typeof(HdbBlocksHeatmap.HeatmapMode))
                .Select(n => new Dropdown.OptionData() { text = n });

            dropdown.options.Clear();
            dropdown.options.AddRange(options);
            dropdown.value = (int)HdbBlocksHeatmap.HeatmapMode.None;
        }

        public void OnDropdownValueChanged(int value)
        {
            var mode = (HdbBlocksHeatmap.HeatmapMode)value;
            heatmap.ChangeHeatmapMode(mode);
            legendParent.SetActive(false);

            switch (mode)
            {
                case HdbBlocksHeatmap.HeatmapMode.Energy:
                    legendSubtitle.text = "Energy Consumption Per Unit (kWh)";
                    minValue.text = $"{heatmap.EnergyMinValue:N2}";
                    maxValue.text = $"{heatmap.EnergyMaxValue:N2}";
                    legendGradient.m_color1 = minLine.color = heatmap.EnergyMinColor;
                    legendGradient.m_color2 = maxLine.color = heatmap.EnergyMaxColor;
                    legendParent.SetActive(true);
                    break;

                case HdbBlocksHeatmap.HeatmapMode.Water:
                    legendSubtitle.text = "Water Consumption Per Unit (Cu M)";
                    minValue.text = $"{heatmap.WaterMinValue:N2}";
                    maxValue.text = $"{heatmap.WaterMaxValue:N2}";
                    legendGradient.m_color1 = minLine.color = heatmap.WaterMinColor;
                    legendGradient.m_color2 = maxLine.color = heatmap.WaterMaxColor;
                    legendParent.SetActive(true);
                    break;

                case HdbBlocksHeatmap.HeatmapMode.None:
                    break;
            }
        }
    }
}