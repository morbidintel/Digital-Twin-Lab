using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using System.Collections;
using System;
using UnityEngine;

namespace GeorgeChew.UnityAssessment.Mapbox
{
    /// <summary>
    /// Injects a mock location into Mapbox and prevents Mapbox from retrieving Location info from the device.<br/>
    /// A mix of <see cref="EditorLocationProvider"/> and <see cref="AbstractEditorLocationProvider"/>,
    /// and edited to work with <see cref="LocationProviderFactory"/>.
    /// </summary>
    public class MockLocationProvider : AbstractLocationProvider
    {
        /// <summary>
        /// The mock "latitude, longitude" location, respresented with a string.
        /// You can search for a place using the embedded "Search" button in the inspector.
        /// This value can be changed at runtime in the inspector.
        /// </summary>
        [SerializeField][Geocode] private string latitudeLongitude;

        [SerializeField] protected int accuracy;
        [SerializeField] private bool autoFireEvent;
        [SerializeField] private float updateInterval;
        [SerializeField] private bool sendEvent;

        private AbstractMap map = null;

        private void Start()
        {
            LocationProviderFactory.Instance.mapManager.OnInitialized += OnMapInitialized;
            StartCoroutine(QueryLocation());
        }

        protected virtual void OnValidate()
        {
            if (sendEvent)
            {
                sendEvent = false;
                SendLocation(currentLocation);
            }
        }

        private IEnumerator QueryLocation()
        {
            // HACK: Let others register before we send our first event.
            // Often this happens in Start.
            yield return new WaitForSeconds(.1f);
            while (true)
            {
                SetLocation();
                if (autoFireEvent)
                {
                    SendLocation(currentLocation);
                }
                yield return new WaitForSeconds(updateInterval);
            }
        }

        // Added to support TouchCamera script.
        public void SendLocationEvent()
        {
            SetLocation();
            SendLocation(currentLocation);
        }

        private Vector2d GetLatitudeLongitude()
        {
            var startingLatLong = Conversions.StringToLatLon(latitudeLongitude);

            if (map == null)
            {
                return startingLatLong;
            }

            var center = map.CenterMercator;
            var scale = map.WorldRelativeScale;
            var position = Conversions
                .GeoToWorldPosition(startingLatLong, center, scale)
                .ToVector3xz();
            var geoPosition = position.GetGeoPosition(center, scale);

            return geoPosition;
        }

        private void OnMapInitialized()
        {
            LocationProviderFactory.Instance.mapManager.OnInitialized -= OnMapInitialized;
            map = LocationProviderFactory.Instance.mapManager;
        }

        protected void SetLocation()
        {
            currentLocation.LatitudeLongitude = GetLatitudeLongitude();
            currentLocation.Accuracy = 5;
            currentLocation.Timestamp = UnixTimestampUtils.To(DateTime.UtcNow);
            currentLocation.IsLocationUpdated = true;
            currentLocation.IsUserHeadingUpdated = true;
            currentLocation.IsLocationServiceEnabled = true;
        }
    }
}