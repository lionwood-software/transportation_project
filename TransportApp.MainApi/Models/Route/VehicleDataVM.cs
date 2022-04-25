using System;
using TransportApp.EntityModel.CacheEntity;

namespace TransportApp.MainApi.Models.Route
{
    public class VehicleDataVM
    {
        private double? distanceToNextVehicle;
        private double? timeToNextVehicle;
        private double timeToNextStop;
        private double? distanceToPreviousVehicle;
        private double? timeToPreviousVehicle;

        public string Number { get; set; }
        public string Direction { get; set; }
        public double? DistanceToNextVehicle
        {
            get { return distanceToNextVehicle; }
            set 
            { 
                if (value == null)
                {
                    distanceToNextVehicle = value;
                }
                else
                {
                    distanceToNextVehicle = Math.Round(value.Value, 8);
                }
            }
        }
        public double? TimeToNextVehicle
        {
            get { return timeToNextVehicle; }
            set
            {
                if (value == null)
                {
                    timeToNextVehicle = value;
                }
                else
                {
                    timeToNextVehicle = Math.Round(value.Value, 8);
                }
            }
        }
        public string NextStop { get; set; }
        public double TimeToNextStop
        {
            get { return timeToNextStop; }
            set
            {
                timeToNextStop = Math.Round(value, 8);
            }
        }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public bool NextVehicleFromOppositeDirection { get; set; }
        public int Azimuth { get; set; }
        public long Timestamp { get; set; }
        public int Speed { get; set; }
        public string NextVehicleNumber { get; set; }
        public double? DistanceToPreviousVehicle
        {
            get { return distanceToPreviousVehicle; }
            set
            {
                if (value == null)
                {
                    distanceToPreviousVehicle = value;
                }
                else
                {
                    distanceToPreviousVehicle = Math.Round(value.Value, 8);
                }
            }
        }
        public double? TimeToPreviousVehicle
        {
            get { return timeToPreviousVehicle; }
            set
            {
                if (value == null)
                {
                    timeToPreviousVehicle = value;
                }
                else
                {
                    timeToPreviousVehicle = Math.Round(value.Value, 8);
                }
            }
        }
        public string PreviousVehicleNumber { get; set; }
    }

    public class VehicleRoute
    {
        public int Index { get; set; }

        public VehicleInfo Vehicle { get; set; }
    }
}
