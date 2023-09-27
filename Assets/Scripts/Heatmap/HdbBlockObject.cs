using Gamelogic.Extensions;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine;

namespace GeorgeChew.HiverlabAssessment.Heatmap
{
    using Data;
    using Events = EventMessaging.Registry.Heatmap;

    /// <summary>
    /// Attached to each generated HDB Block. Holds data and handles click events.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class HdbBlockObject : MonoBehaviour, IPointerClickHandler
    {
        public HdbBlockData HdbData => hdbData;
        public ConsumptionData ConsumptionData => consumptionData;

        [SerializeField, ReadOnly] private HdbBlockData hdbData;
        [SerializeField, ReadOnly] private ConsumptionData consumptionData;

        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        private Material material;
        private Color originalColor;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
        }

        private void Start()
        {
            meshCollider.sharedMesh = GetComponent<MeshFilter>()?.mesh ?? null;

            originalColor = meshRenderer.material.color;
            material = meshRenderer.material;
        }

        public void Initialize(HdbBlockData hdbData)
        {
            this.hdbData = hdbData;
            consumptionData = ConsumptionData.GenerateFromHdbData(hdbData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Events.OnClickHdbBlock.Publish(this);
        }

        public void ShowEnergyHeatmap(
            float energyMinValue,
            float energyMaxValue,
            Color energyMinColor,
            Color energyMaxColor)
        {
            float t = Mathf.InverseLerp(energyMinValue, energyMaxValue, consumptionData.energyPerUnit);
            material.color = Color.Lerp(energyMinColor, energyMaxColor, t);
        }

        public void ShowWaterHeatmap(
            float waterMinValue,
            float waterMaxValue,
            Color waterMinColor,
            Color waterMaxColor)
        {
            float t = Mathf.InverseLerp(waterMinValue, waterMaxValue, consumptionData.waterPerUnit);
            material.color = Color.Lerp(waterMinColor, waterMaxColor, t);
        }

        public void RemoveHeatmap()
        {
            material.color = originalColor;
        }
    }
}