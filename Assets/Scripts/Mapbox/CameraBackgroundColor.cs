using Mapbox.Unity.Map;
using System;
using UnityEngine.Assertions;
using UnityEngine;

namespace GeorgeChew.UnityAssessment.Mapbox
{
    using Events = EventMessaging.Registry.Mapbox;

    /// <summary>
    /// Changes the Background Color of the Camera according to the 
    /// <see cref="ImagerySourceType"/> of the <see cref="AbstractMap"/>.
    /// </summary>
    [RequireComponent(typeof(Camera))]
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

            Events.OnChangeImageSource += UpdateColor;
        }

        private void UpdateColor(object obj)
        {
            currentImageSource = (ImagerySourceType)obj;

            int colorIndex = (int)currentImageSource;

            if (colorIndex < colors.Length)
            {
                GetComponent<Camera>().backgroundColor = colors[colorIndex];
            }
        }
    }
}