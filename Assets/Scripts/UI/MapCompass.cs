using DG.Tweening;
using Gamelogic.Extensions;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine;

namespace GeorgeChew.UnityAssessment.UI
{
    /// <summary>
    /// Controls the on screen compass and events triggered by clicks on that compass.
    /// </summary>
    public class MapCompass : MonoBehaviour
    {
        [SerializeField] private Image compassImage = null;
        [SerializeField] private Button button = null;
        [SerializeField] private new OrbitalCamera camera;

        private static float fadeDuration = 0.3f;

        private void Awake()
        {
            Assert.IsNotNull(compassImage);
            Assert.IsNotNull(button);
            Assert.IsNotNull(camera);
        }

        private void Start()
        {
            compassImage.color = new Color(1, 1, 1, 0);
            button.onClick.AddListener(() => OnClickCompass());
        }

        private void Update()
        {
            // Fade out the compass when the camera orientation is at 'origin'
            bool isAzimuthZero = Mathf.Approximately(camera.Azimuth, 0f);
            bool isAzimuth360 = Mathf.Approximately(camera.Azimuth, 360f);
            bool isElevationMax = Mathf.Approximately(camera.Elevation, camera.defaultMaxAngle);
            if ((isAzimuthZero || isAzimuth360) && isElevationMax)
            {
                // fade out only if we haven't already faded
                // this prevents see-sawing of the fading
                if (compassImage.color.a == 1)
                {
                    compassImage.DOFade(0, fadeDuration);
                }
            }
            else if (compassImage.color.a == 0)
            {
                compassImage.DOFade(1, fadeDuration);
            }

            // Orientate the compass to match MapCamera orientation
            Vector3 rotation = transform.rotation.eulerAngles
                .WithX(90f - camera.Elevation)
                .WithZ(camera.Azimuth);
            transform.rotation = Quaternion.Euler(rotation);
        }

        /// <summary>
        /// Resets the MapCamera.
        /// </summary>
        public void OnClickCompass()
        {
            // move the camera a bit to stop the camera spinning
            camera.Azimuth = camera.Azimuth < 180f ? 0 : 360;
            camera.Elevation = 89;
        }
    }
}