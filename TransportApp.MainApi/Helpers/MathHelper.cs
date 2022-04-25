using System;

namespace TransportApp.MainApi.Helpers
{
    public static class MathHelper
    {
        public static double GetDistance(double lon1, double lat1, double lon2, double lat2)
        {
            return HaversineDistance(lon1, lat1, lon2, lat2);
        }

        public static bool IsPointInCirlce(double lon1, double lat1, double lon2, double lat2, double radius)
        {
            return GetDistance(lon1, lat1, lon2, lat2) <= radius;
        }

        private static double HaversineDistance(double lon1, double lat1, double lon2, double lat2)
        {
            // distance between latitudes and longitudes 
            double dLat = Math.PI / 180.0 * (lat2 - lat1);
            double dLon = Math.PI / 180.0 * (lon2 - lon1);

            // convert to radians 
            lat1 = Math.PI / 180.0 * lat1;
            lat2 = Math.PI / 180.0 * lat2;

            // apply formula
            double a = Math.Pow(Math.Sin(dLat / 2.0), 2.0) + Math.Pow(Math.Sin(dLon / 2.0), 2.0) * Math.Cos(lat1) * Math.Cos(lat2);
            // Earth radius in km
            double radius = 6371.0;
            double c = 2.0 * Math.Asin(Math.Sqrt(a));

            return radius * c;
        }
    }
}
