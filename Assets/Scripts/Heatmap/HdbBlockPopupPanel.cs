using Gamelogic.Extensions;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine;

namespace GeorgeChew.UnityAssessment.Heatmap
{
    using HeatmapEvents = EventMessaging.Registry.Heatmap;

    /// <summary>
    /// Handles the position and the data shown on the popup panel
    /// </summary>
    public class HdbBlockPopupPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;

        [Header("UI References")]
        [SerializeField] private LineRenderer locatorLine;
        [SerializeField] private Button closeButton;

        [Header("Text Labels")]
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI energySubtitleLabel;
        [SerializeField] private TextMeshProUGUI energyValueLabel;
        [SerializeField] private TextMeshProUGUI waterSubtitleLabel;
        [SerializeField] private TextMeshProUGUI waterValueLabel;

        [Header("Configuration")]
        [SerializeField] private float popupHeight = 200f;

        [Header("Data")]
        [SerializeField, ReadOnly] private HdbBlockObject currentHdbBlock;

        private RectTransform rectTransform;

        private void Awake()
        {
            Assert.IsNotNull(panel);
            Assert.IsNotNull(locatorLine);
            Assert.IsNotNull(closeButton);
            Assert.IsNotNull(titleLabel);
            Assert.IsNotNull(energySubtitleLabel);
            Assert.IsNotNull(energyValueLabel);
            Assert.IsNotNull(waterSubtitleLabel);
            Assert.IsNotNull(waterValueLabel);
        }

        private void Start()
        {
            rectTransform = transform as RectTransform;

            closeButton.onClick.AddListener(ClosePopup);
            HeatmapEvents.OnClickHdbBlock += OpenPopup;
        }

        private void Update()
        {
            if (currentHdbBlock == null) return;

            // the map has constant position in the scene,
            // and the buildings move around in world space
            rectTransform.position = currentHdbBlock.transform.position;

            locatorLine.SetPosition(0, transform.position);
            locatorLine.SetPosition(1, panel.transform.position);
        }

        private void OpenPopup(object obj)
        {
            currentHdbBlock = obj as HdbBlockObject;
            var consumptionData = currentHdbBlock.ConsumptionData;

            locatorLine.gameObject.SetActive(true);
            panel.SetActive(true);

            // conform the popup the scale of the building, so when the map is zoomed out,
            // the popup should look the same size
            float height = popupHeight * currentHdbBlock.transform.lossyScale.z;
            rectTransform.sizeDelta = new(0, height);

            // populate data
            titleLabel.text = currentHdbBlock.HdbData.address;
            energyValueLabel.text = $"{consumptionData.energy:N2}";
            waterValueLabel.text = $"{consumptionData.water:N2}";
        }

        private void ClosePopup()
        {
            currentHdbBlock = null;
            locatorLine.gameObject.SetActive(false);
            panel.SetActive(false);
        }
    }
}