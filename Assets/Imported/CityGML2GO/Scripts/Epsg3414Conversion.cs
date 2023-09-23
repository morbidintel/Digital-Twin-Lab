using DotSpatial.Projections;
using UnityEngine;
namespace CityGML2GO
{
    public class Epsg3414Conversion
    {
        // from https://epsg.io/3414
        public static ProjectionInfo epsg3414 = ProjectionInfo.FromProj4String(
                "+proj=tmerc +lat_0=1.366666666666667 +lon_0=103.8333333333333 +k=1 +x_0=28001.642 +y_0=38744.572 +ellps=WGS84 +units=m +no_defs ");
        // from https://epsg.io/4326
        static ProjectionInfo epsg4326 = ProjectionInfo.FromProj4String(
            "+proj=longlat +datum=WGS84 +no_defs ");

        /// <summary>
        /// The center coordinates of the EPSG:3414 coordinate system 
        /// in easting (x) and northing (z).
        /// </summary>
        public static Vector3 Epsg3414Center =>
            new Vector3(27630.67f, 0, 31372.91f); // from https://epsg.io/3414

        /// <summary>
        /// The center coordinates of the EPSG:3414 coordinate system 
        /// in latitude (x) and longitude (z).
        /// </summary>
        public static Vector3 Epsg3414CenterLatLong =>
            new Vector3(4.1f / 3f, 0, 311.5f / 3f);

        /// <summary>
        /// <para>Get the distance in meters of a coordinate from the center coordinates 
        /// of EPSG:3414.</para>
        /// <para>This can be used to get Unity coordinates based on the Unity origin (0,0,0) 
        /// being the EPSG3414 center coordinates (27630.67 31372.91).</para>
        /// </summary>
        /// <param name="epsg3414Coord">Coordinates in EPSG:3414 
        /// easting (x) and northing (z).</param>
        /// <returns></returns>
        public static Vector3 DistanceFromCenter(Vector3 epsg3414Coord)
        {
            return epsg3414Coord - Epsg3414Center;
        }

        public static Vector3 DistanceFromCenterLatLong(Vector3 latlng)
        {
            return Epsg3414FromLatLong(latlng) - Epsg3414Center;
        }

        public static Vector3 DistanceFromCenterLatLong(float latitude, float longitude)
        {
            return DistanceFromCenterLatLong(new Vector3(latitude, 0, longitude));
        }

        /// <summary>
        /// Get Lat/Long coordinates from EPSG:3414 coordinates.
        /// </summary>
        /// <param name="epsg3414Coord">Coordinates in EPSG:3414 
        /// easting (x) and northing (z).</param>
        /// <returns>Coordinates in latidude (x) and longitude (z).</returns>
        public static Vector3 LatLongFromEpsg3414(Vector3 epsg3414Coord)
        {
            var xy = new double[] { epsg3414Coord.x, epsg3414Coord.z };
            var z = new double[] { epsg3414Coord.y };
            Reproject.ReprojectPoints(xy, z, epsg3414, epsg4326, 0, 1);
            return new Vector3((float)xy[1], (float)z[0], (float)xy[0]);
        }

        /// <summary>
        /// Get EPSG:3414 coordinates from Lat/Long coordinates.
        /// </summary>
        /// <param name="latlng">Coordinates in latidude (x) and longitude (z).</param>
        /// <returns>Coordinates in EPSG:3414 easting (x) and northing (z).</returns>
        public static Vector3 Epsg3414FromLatLong(Vector3 latlng)
        {
            var xy = new double[] { latlng.z, latlng.x };
            var z = new double[] { latlng.y };
            Reproject.ReprojectPoints(xy, z, epsg4326, epsg3414, 0, 1);
            return new Vector3((float)xy[0], (float)z[0], (float)xy[1]);
        }
    }
}