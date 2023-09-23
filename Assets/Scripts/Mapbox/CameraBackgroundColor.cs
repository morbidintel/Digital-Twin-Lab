using UnityEngine;
using System;
using Mapbox.Unity.Map;
using UnityEngine.Assertions;

namespace GeorgeChew.HiverlabAssessment.Mapbox
{
    /// <summary>
    /// Changes the Background Color of the Camera according to the <see cref="ImagerySourceType"/>
    /// of the <see cref="AbstractMap"/>.
    /// </summary>
    public class CameraBackgroundColor : MonoBehaviour
    {
        [SerializeField]
        private AbstractMap map;

        // length should be same number as ImagerySourceType
        [SerializeField]
        private Color[] colors =
            new Color[Enum.GetNames(typeof(ImagerySourceType)).Length];

        private ImagerySourceType currentImageSource;

        private void Awake()
        {
            Assert.IsNotNull(map);
            Assert.AreEqual(colors.Length, Enum.GetNames(typeof(ImagerySourceType)).Length);
        }

        private void Start()
        {
            UpdateColor(map.ImageLayer.LayerSource);
        }

        private void Update()
        {
            if (currentImageSource != map.ImageLayer.LayerSource)
            {
                UpdateColor(map.ImageLayer.LayerSource);
            }
        }

        private void UpdateColor(ImagerySourceType imageSource)
        {
            currentImageSource = imageSource;
            int colorIndex = (int)currentImageSource;
            if (colorIndex < colors.Length)
                GetComponent<Camera>().backgroundColor = colors[colorIndex];
        }
    }
}