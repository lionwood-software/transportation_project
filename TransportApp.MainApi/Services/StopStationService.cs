using AutoMapper;
using Microsoft.AspNetCore.Http;
using TransportApp.MainApi.Factory;
using TransportApp.MainApi.Models.StopStations;
using TransportApp.EntityModel;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;
using System;
using System.Threading.Tasks;
using TransportApp.Cache;
using System.Collections.Generic;
using TransportApp.MainApi.Helpers;
using TransportApp.EntityModel.CacheEntity;
using Microsoft.Extensions.Options;
using TransportApp.MainApi.Exceptions;
using TransportApp.MainApi.ApiClients;

namespace TransportApp.MainApi.Services
{
    public class StopStationService : BaseService<StopStation>
    {
        private readonly ICacheProvider _storage;
        private readonly BusGpsApiClient _busApiClient;
        private readonly TramGpsApiClient _tramApiClient;
        private readonly VehicleTypes _vehicleTypes;
        private const double _averageVehicleSpeed = 17;
        private readonly int _averageSecondsOnStop;

        public StopStationService(IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            RepositoryFactory factory,
            ICacheProvider storage,
            IOptions<ApiConfigurationOptions> option,
            BusGpsApiClient busGpsApiClient,
            TramGpsApiClient tramGpsApiClient)
            : base(mapper, httpContextAccessor, factory)
        {
            _storage = storage;
            _busApiClient = busGpsApiClient;
            _tramApiClient = tramGpsApiClient;
            _vehicleTypes = new VehicleTypes();
            _averageSecondsOnStop = option?.Value.AverageSecondsOnStop ?? 45;
        }

        public async Task<ViewStopWithRoutes> GetRoutesByIdStopAsync(string id)
        {
            var stop = await _repository.SingleOrDefaultAsync<StopStation>(x => x.Id == id);

            if (stop == null)
                throw new NotFoundException("StopStation not found.");

            List<Route> routes = new List<Route>();

            if (stop.Type == StopStationType.Metro)
            {
                routes = _repository.GetCollection<Route>()
                    .Find(x => x.Active && (x.Forward.Stops.Any(y => y.Id == stop.Id) || x.Backward.Stops.Any(y => y.Id == stop.Id)) && x.Type == VehicleType.Metro)
                    .ToList();

                routes = routes.Where(x => !x.TempRoute || (x.TempRoute && DateTime.Now.Date >= x.TempStart?.Date && DateTime.Now.Date < x.TempEnd?.Date)).ToList();
            }
            else
            {
                routes = _repository.GetCollection<Route>()
                    .Find(x => x.Active && (x.Forward.Stops.Any(y => y.Id == stop.Id) || x.Backward.Stops.Any(y => y.Id == stop.Id)) && x.Type != VehicleType.Metro)
                    .ToList();

                routes = routes.Where(x => !x.TempRoute || (x.TempRoute && DateTime.Now.Date >= x.TempStart?.Date && DateTime.Now.Date < x.TempEnd?.Date)).ToList();
            }

            var stopWithRoutes = new ViewStopWithRoutes()
            {
                Id = stop.Id,
                Number = stop.Number,
                NameEN = stop.NameEN,
                NameRU = stop.NameRU,
                NameUA = stop.NameUA,
                Type = stop.Type
            };

            foreach (var route in routes)
            {
                if (route.Type == VehicleType.Metro)
                {
                    if (route.Backward.Stops.FirstOrDefault()?.Id != id)
                    {
                        var times = GetNextETAFromStopsInterval(id, route, DirectionType.Forward);

                        var firstTime = times.FirstOrDefault();
                        var secondTime = times.Skip(1).FirstOrDefault();

                        var lastForwardStop = route.Forward.Stops.LastOrDefault();

                        stopWithRoutes.Routes.Add(new RouteForViewStopWithRoutes
                        {
                            Id = route.Id,
                            Name = lastForwardStop?.NameUA,
                            NextETA = firstTime,
                            SecondETA = secondTime,
                            VehicleType = route.Type,
                            BusType = route.BusType,
                            LastStopStation = _mapper.Map<ViewStopStation>(lastForwardStop)
                        });
                    }

                    if (route.Forward.Stops.FirstOrDefault()?.Id != id)
                    {
                        var times = GetNextETAFromStopsInterval(id, route, DirectionType.Backward);

                        var firstTime = times.FirstOrDefault();
                        var secondTime = times.Skip(1).FirstOrDefault();

                        var lastBackwardStop = route.Backward.Stops.LastOrDefault();

                        stopWithRoutes.Routes.Add(new RouteForViewStopWithRoutes
                        {
                            Id = route.Id,
                            Name = lastBackwardStop?.NameUA,
                            NextETA = firstTime,
                            SecondETA = secondTime,
                            VehicleType = route.Type,
                            BusType = route.BusType,
                            LastStopStation = _mapper.Map<ViewStopStation>(lastBackwardStop)
                        });
                    }
                }
                else
                {
                    DateTime? time = null;

                    var routeInfo = _storage.Get<RouteInfo>(route.Id);

                    if (routeInfo != null)
                    {
                        time = GetNextETAFromStorage(stop, route, routeInfo);
                    }

                    stopWithRoutes.Routes.Add(new RouteForViewStopWithRoutes
                    {
                        Id = route.Id,
                        Name = route.Number,
                        NextETA = time,
                        VehicleType = route.Type,
                        BusType = route.BusType,
                        LastStopStation = GetLastStopStation(stop.Id, route)
                    });
                }
            }

            if (stop.Type != StopStationType.Metro)
            {
                stopWithRoutes.Routes = stopWithRoutes.Routes.OrderByDescending(x => x.NextETA.HasValue).ThenBy(x => x.NextETA).ToList();
            }

            return stopWithRoutes;
        }

        public async Task<List<ViewStopStation>> GetAllStopsByIdsAsync(string[] ids)
        {
            var stops = await _repository.GetCollection<StopStation>()
                .Find(Builders<StopStation>.Filter.In(x => x.Id, ids))
                .ToListAsync();

            return _mapper.Map<List<ViewStopStation>>(stops);
        }

        private ViewStopStation GetLastStopStation(string stopId, Route route)
        {
            StopStation lastStop = null;
            if (route.Forward.Stops.Any(x => x.Id == stopId))
            {
                lastStop = route.Forward.Stops.LastOrDefault();
            }
            else
            {
                lastStop = route.Backward.Stops.LastOrDefault();
            }

            return _mapper.Map<ViewStopStation>(lastStop);
        }

        private (List<VehicleInfo>, List<Geolocation>, DirectionType) FilterRouteData(StopStation stop, Route route, RouteInfo routeInfo, bool circularRoute)
        {
            var result = new List<VehicleInfo>();
            var vehicles = new List<VehicleInfo>();
            var routeGeolocations = new List<Geolocation>();
            var routeVehicles = new List<VehicleInfo>();
            var direction = DirectionType.Undefined;

            if (circularRoute)
            {
                vehicles = routeInfo.Vehicles.Select(x =>
                {
                    var vehicle = VehicleInfo.Create(x);
                    if (vehicle.Direction == DirectionType.Backward)
                    {
                        vehicle.Direction = DirectionType.Forward;
                    }
                    if (vehicle.ExpectedDirection == DirectionType.Backward)
                    {
                        vehicle.ExpectedDirection = DirectionType.Forward;
                    }
                    return vehicle;
                }).ToList();
            }
            else
            {
                vehicles = routeInfo.Vehicles;
            }

            if (route.Forward.Stops.Any(x => x.Id == stop.Id))
            {
                direction = DirectionType.Forward;
                routeGeolocations = route.Forward.ImprovedGeolocation;
                routeVehicles = vehicles.Where(x => x.Direction == DirectionType.Forward || (x.Direction == DirectionType.Sleep && x.ExpectedDirection == DirectionType.Forward && !x.IsSleep)).ToList();
            }
            else
            {
                direction = DirectionType.Backward;
                routeGeolocations = route.Backward.ImprovedGeolocation;
                routeVehicles = vehicles.Where(x => x.Direction == DirectionType.Backward || (x.Direction == DirectionType.Sleep && x.ExpectedDirection == DirectionType.Backward && !x.IsSleep)).ToList();
            }

            foreach (var vehicle in routeVehicles)
            {
                var vehicleClosestGeolocation = GetClosestGeolocation(routeGeolocations, vehicle.Normal.LastOrDefault().Longitude, vehicle.Normal.LastOrDefault().Latitude);
                var stopClosestGeolocation = GetClosestGeolocation(routeGeolocations, stop.Geolocation.Longitude, stop.Geolocation.Latitude);

                if (vehicleClosestGeolocation.Id > stopClosestGeolocation.Id)
                {
                    continue;
                }

                result.Add(vehicle);
            }

            return (result, routeGeolocations, direction);
        }

        private double CalculateDistanceFromVehicleToStop(StopStation stop, List<Geolocation> routeGeolocations, Geolocation vehicleGeolocation)
        {
            var vehicleClosestGeolocation = GetClosestGeolocation(routeGeolocations, vehicleGeolocation.Longitude, vehicleGeolocation.Latitude);
            var stopClosestGeolocation = GetClosestGeolocation(routeGeolocations, stop.Geolocation.Longitude, stop.Geolocation.Latitude);

            var sum = 0.0;
            if (stopClosestGeolocation.Id == vehicleClosestGeolocation.Id)
            {
                sum = MathHelper.GetDistance(stop.Geolocation.Longitude, stop.Geolocation.Latitude, vehicleGeolocation.Longitude, vehicleGeolocation.Latitude);
            }
            else
            {
                foreach (var geolocation in routeGeolocations.Where(x => x.Id >= vehicleClosestGeolocation.Id && x.Id < stopClosestGeolocation.Id))
                {
                    if (routeGeolocations.Last().Id == geolocation.Id)
                    {
                        break;
                    }

                    var nextGeolocation = routeGeolocations[geolocation.Id + 1];

                    sum += MathHelper.GetDistance(geolocation.Longitude, geolocation.Latitude, nextGeolocation.Longitude, nextGeolocation.Latitude);
                }
            }

            return sum;
        }

        private Geolocation GetClosestGeolocation(List<Geolocation> routeGeolocations, double lon, double lat)
        {
            return routeGeolocations.OrderBy(x => MathHelper.GetDistance(lon, lat, x.Longitude, x.Latitude))
                  .FirstOrDefault();
        }

        private DateTime? GetNextETAFromStorage(StopStation stop, Route route, RouteInfo routeInfo)
        {
            try
            {
                bool circularRoute = false;
                if (!route.Forward.Stops.Any())
                {
                    route.Forward.Stops.AddRange(route.Backward.Stops);
                    route.Forward.ImprovedGeolocation.AddRange(route.Backward.ImprovedGeolocation);
                    circularRoute = true;
                }
                else if (!route.Backward.Stops.Any())
                {
                    route.Backward.Stops.AddRange(route.Forward.Stops);
                    route.Backward.ImprovedGeolocation.AddRange(route.Forward.ImprovedGeolocation);
                    circularRoute = true;
                }

                var (vehicleList, routeGeolocations, direction) = FilterRouteData(stop, route, routeInfo, circularRoute);
                var routeStop = direction == DirectionType.Forward
                    ? route.Forward.Stops.First(x => x.Id == stop.Id)
                    : route.Backward.Stops.First(x => x.Id == stop.Id);

                var time = 0.0;
                if (!vehicleList.Any())
                {
                    if (direction == DirectionType.Forward)
                    {
                        var vehicles = routeInfo.Vehicles.Where(x => x.Direction == DirectionType.Backward || (x.Direction == DirectionType.Sleep && x.ExpectedDirection == DirectionType.Backward && !x.IsSleep)).ToList();

                        if (!vehicles.Any())
                        {
                            return null;
                        }

                        var lastBackwardStop = route.Backward.Stops.Last();
                        var firstForwardStop = route.Forward.Stops.First();

                        var closestVehicleToLastStop = vehicles.OrderBy(x => CalculateDistanceFromVehicleToStop(lastBackwardStop, route.Backward.ImprovedGeolocation, x.Normal.LastOrDefault())).FirstOrDefault();
                        var distance = CalculateDistanceFromVehicleToStop(lastBackwardStop, route.Backward.ImprovedGeolocation, closestVehicleToLastStop.Normal.LastOrDefault());
                        distance += route.Forward.StopDistances[firstForwardStop.Index, routeStop.Index]
                                        + MathHelper.GetDistance(lastBackwardStop.Geolocation.Longitude, lastBackwardStop.Geolocation.Latitude, firstForwardStop.Geolocation.Longitude, firstForwardStop.Geolocation.Latitude);

                        var stopsBettwenOpposite = GetStopsBeetwenVehicleAndStop(closestVehicleToLastStop.Normal.LastOrDefault(), route, lastBackwardStop, DirectionType.Backward);
                        var stopsBettwen = route.Forward.Stops.Where(x => x.Index <= routeStop.Index).ToList();

                        time = (distance / _averageVehicleSpeed) + (stopsBettwen.Count + stopsBettwenOpposite.Count) * (_averageSecondsOnStop / 3600);
                    }
                    else
                    {
                        var vehicles = routeInfo.Vehicles.Where(x => x.Direction == DirectionType.Forward || (x.Direction == DirectionType.Sleep && x.ExpectedDirection == DirectionType.Forward && !x.IsSleep)).ToList();

                        if (!vehicles.Any())
                        {
                            return null;
                        }

                        var lastForwardStop = route.Forward.Stops.Last();
                        var firstBackwardStop = route.Backward.Stops.First();

                        var closestVehicleToLastStop = vehicles.OrderBy(x => CalculateDistanceFromVehicleToStop(lastForwardStop, route.Forward.ImprovedGeolocation, x.Normal.LastOrDefault())).FirstOrDefault();
                        var distance = CalculateDistanceFromVehicleToStop(lastForwardStop, route.Forward.ImprovedGeolocation, closestVehicleToLastStop.Normal.LastOrDefault());
                        distance += route.Backward.StopDistances[firstBackwardStop.Index, routeStop.Index]
                                        + MathHelper.GetDistance(lastForwardStop.Geolocation.Longitude, lastForwardStop.Geolocation.Latitude, firstBackwardStop.Geolocation.Longitude, firstBackwardStop.Geolocation.Latitude);

                        var stopsBettwenOpposite = GetStopsBeetwenVehicleAndStop(closestVehicleToLastStop.Normal.LastOrDefault(), route, lastForwardStop, DirectionType.Forward);
                        var stopsBettwen = route.Backward.Stops.Where(x => x.Index <= routeStop.Index).ToList();

                        time = (distance / _averageVehicleSpeed) + (stopsBettwen.Count + stopsBettwenOpposite.Count) * (_averageSecondsOnStop / 3600);
                    }
                }
                else
                {
                    var closestVehicle = vehicleList.OrderBy(x => CalculateDistanceFromVehicleToStop(stop, routeGeolocations, x.Normal.LastOrDefault())).FirstOrDefault();
                    var distance = CalculateDistanceFromVehicleToStop(stop, routeGeolocations, closestVehicle.Normal.LastOrDefault());

                    var stopsBeetwen = GetStopsBeetwenVehicleAndStop(closestVehicle.Normal.LastOrDefault(), route, routeStop, direction);

                    time = (distance / _averageVehicleSpeed) + stopsBeetwen.Count * (_averageSecondsOnStop / 3600);
                }

                return DateTime.Now.AddHours(time);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }

            return null;
        }

        private List<DateTime?> GetNextETAFromStopsInterval(string stopId, Route route, DirectionType direction)
        {
            WeekInterval stopIntervals = null;

            if (direction == DirectionType.Forward)
            {
                stopIntervals = route.StopsInterval?.Forward?.FirstOrDefault(x => x.StopId == stopId);
            }
            else
            {
                stopIntervals = route.StopsInterval?.Backward?.FirstOrDefault(x => x.StopId == stopId);
            }

            if (stopIntervals != null)
            {
                var now = DateTime.Now;
                var nowFromStart = new DateTime(1970, 1, 1, now.Hour, now.Minute, now.Second);
                var intervals = now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday ? stopIntervals.Weekend : stopIntervals.Weekday;
                return intervals.Where(x => x > nowFromStart).Take(2).Select(x => (DateTime?)new DateTime(now.Year, now.Month, now.Day, x.Hour, x.Minute, x.Second, DateTimeKind.Utc)).ToList();
            }

            return new List<DateTime?>();
        }

        private List<StopStation> GetStopsBeetwenVehicleAndStop(Geolocation vehicleGeo, Route route, StopStation stop, DirectionType direction)
        {
            var res = new List<StopStation>();
            if (direction == DirectionType.Forward)
            {
                var closestStop = route.Forward.Stops.OrderBy(x => MathHelper.GetDistance(x.Geolocation.Longitude, x.Geolocation.Latitude, vehicleGeo.Longitude, vehicleGeo.Latitude)).First();
                res.AddRange(route.Forward.Stops.Where(x => x.Index >= closestStop.Index && x.Index < stop.Index));
            }
            else
            {
                var closestStop = route.Backward.Stops.OrderBy(x => MathHelper.GetDistance(x.Geolocation.Longitude, x.Geolocation.Latitude, vehicleGeo.Longitude, vehicleGeo.Latitude)).First();
                res.AddRange(route.Forward.Stops.Where(x => x.Index >= closestStop.Index && x.Index < stop.Index));
            }

            return res;
        }
    }
}