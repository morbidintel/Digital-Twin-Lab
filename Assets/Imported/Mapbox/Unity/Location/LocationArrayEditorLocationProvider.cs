namespace Mapbox.Unity.Location
{
	using System;
	using Mapbox.Unity.Utilities;
	using Mapbox.Utils;
	using UnityEngine;

	/// <summary>
	/// The EditorLocationProvider is responsible for providing mock location and heading data
	/// for testing purposes in the Unity editor.
	/// </summary>
	public class LocationArrayEditorLocationProvider : AbstractEditorLocationProvider
	{
		/// <summary>
		/// The mock "latitude, longitude" location, respresented with a string.
		/// You can search for a place using the embedded "Search" button in the inspector.
		/// This value can be changed at runtime in the inspector.
		/// </summary>
		[SerializeField]
		[Geocode]
		string[] _latitudeLongitude;

		/// <summary>
		/// The mock heading value.
		/// </summary>
		[SerializeField]
		[Range(0, 359)]
		float _heading;


		private int idx = -1;
		Vector2d LatitudeLongitude
		{
			get
			{
				idx++;
				// reset index to keep looping through the location array
				if (idx >= _latitudeLongitude.Length) { idx = 0; }
				return Conversions.StringToLatLon(_latitudeLongitude[idx]);
			}
		}

		protected override void SetLocation()
		{
			currentLocation.UserHeading = _heading;
			currentLocation.LatitudeLongitude = LatitudeLongitude;
			currentLocation.Accuracy = _accuracy;
			currentLocation.Timestamp = UnixTimestampUtils.To(DateTime.UtcNow);
			currentLocation.IsLocationUpdated = true;
			currentLocation.IsUserHeadingUpdated = true;
		}
	}
}
