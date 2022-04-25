using AutoMapper;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Type = TransportApp.EntityModel.VehicleType;
using TransportApp.MainApi.Factory;
using System.Threading.Tasks;
using MongoDB.Driver;
using TransportApp.MainApi.Models.Route;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using TransportApp.MainApi.Models;
using TransportApp.MainApi.Models.CompactRoute;
using TransportApp.EntityModel;
using TransportApp.MainApi.Exceptions;

namespace TransportApp.MainApi.Services
{
    public partial class RouteService : BaseService<Route>
    {
        public RouteService(IMapper mapper, IHttpContextAccessor httpContextAccessor, RepositoryFactory factory) : base(mapper, httpContextAccessor, factory)
        {
        }

        public async Task<ModelList<ViewRoute>> GetAllAsync(Type? type, int pageNumber, int pageSize, string routeNumber)
        {
            var filters = GetRouteBaseFilterDefinitions();

            if (pageNumber <= 0 || pageSize < 0)
            {
                pageNumber = 1;
                pageSize = 0;
            }

            if (pageSize > 50)
            {
                pageSize = 50;
            }

            if (type != null)
            {
                filters.Add(Builders<Route>.Filter.Eq(x => x.Type, type));
            }

            if (!string.IsNullOrEmpty(routeNumber))
            {
                pageSize = 10;
                pageNumber = 1;
                filters.Add(Builders<Route>.Filter.Regex(x => x.Number, new MongoDB.Bson.BsonRegularExpression($"^{routeNumber}", "i")));
            }

            var routeQuery = _repository.GetCollection<Route>()
               .Find(Builders<Route>.Filter.And(filters));

            var count = await routeQuery.CountDocumentsAsync();

            var routes = routeQuery
                .Sort(Builders<Route>.Sort.Combine(Builders<Route>.Sort.Ascending(x => x.NumberInt), Builders<Route>.Sort.Ascending(x => x.BusType)))
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToList();

            var res = MappToRouteModels(routes);
            return new ModelList<ViewRoute>
            {
                Data = res,
                PageCount = (int)Math.Ceiling((double)count / pageSize),
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ModelList<CompactRoute>> GetCompactAsync(int pageNumber, int pageSize, Type? type = null)
        {
            var filters = GetRouteBaseFilterDefinitions();

            if (type.HasValue)
            {
                filters.Add(Builders<Route>.Filter.Eq(x => x.Type, type.Value));
            }

            if (pageNumber <= 0 || pageSize < 0)
            {
                pageNumber = 1;
                pageSize = 0;
            }

            var routeQuery = _repository.GetCollection<Route>()
               .Find(Builders<Route>.Filter.And(filters));

            var count = await routeQuery.CountDocumentsAsync();

            var routes = routeQuery
                .SortBy(x => x.NumberInt)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToList();

            var res = MappToCompactRouteModels(routes);
            return new ModelList<CompactRoute>
            {
                Data = res,
                PageCount = (int)Math.Ceiling((double)count / pageSize),
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<List<ViewRouteNumbersWithBusType>> GetRouteNumbersByTypeAsync(Type type)
        {
            var filters = GetRouteBaseFilterDefinitions();
            filters.Add(Builders<Route>.Filter.Eq(x => x.Type, type));

            var routes = await _repository.GetCollection<Route>()
                .Find(Builders<Route>.Filter.And(filters))
                .SortBy(x => x.NumberInt)
                .ToListAsync();

            return routes.Select(x => new ViewRouteNumbersWithBusType { Number = x.Number, Type = x.BusType }).ToList();
        }

        public async Task<string[]> GetRouteNumbersByTypeAndBusTypeAsync(Type type, BusType busType)
        {
            var filters = GetRouteBaseFilterDefinitions();
            filters.Add(Builders<Route>.Filter.Eq(x => x.Type, type));
            filters.Add(Builders<Route>.Filter.Eq(x => x.BusType, busType));

            var routes = await _repository.GetCollection<Route>()
                .Find(Builders<Route>.Filter.And(filters))
                .SortBy(x => x.NumberInt)
                .ToListAsync();

            return routes.Select(x => x.Number).ToArray();
        }

        public async Task<List<ViewRoute>> GetAllRoutesByIdsAsync(string[] ids)
        {
            var filters = GetRouteBaseFilterDefinitions();

            filters.Add(Builders<Route>.Filter.In(x => x.Id, ids));

            var routes = await _repository.GetCollection<Route>()
                .Find(Builders<Route>.Filter.And(filters))
                .ToListAsync();

            return MappToRouteModels(routes);
        }

        public async Task<ViewRoute> GetByTypeAndNumberAsync(Type type, string number, BusType? busType)
        {
            var filters = GetRouteBaseFilterDefinitions();
            filters.Add(Builders<Route>.Filter.Eq(x => x.Type, type));
            filters.Add(Builders<Route>.Filter.Eq(x => x.Number, number));

            if (busType.HasValue)
            {
                filters.Add(Builders<Route>.Filter.Eq(x => x.BusType, busType.Value));
            }

            var entity = await _repository.GetCollection<Route>().Find(Builders<Route>.Filter.And(filters)).FirstOrDefaultAsync();

            if (entity == null)
                throw new NotFoundException("Route not found!");

            return MappToRouteModels(new List<Route> { entity }).FirstOrDefault();
        }

        public override async Task<Y> GetByIdAsync<Y>(string id)
        {
            var route = await _repository.FindOneAsync<Route>(x => x.Id == id);
            var message = await _repository.FindOneAsync<Message>(x => x.RouteId == id && x.EndDate >= DateTime.Now && x.StartDate <= DateTime.Now);

            var mapped = _mapper.Map<ViewRoute>(route);
            mapped.Message = _mapper.Map<MessageForViewRoute>(message);

            var forwardLength = route.Forward.Stops.Count > 0 && route.Forward.StopDistances != null ? route.Forward.StopDistances[0, route.Forward.Stops.Count - 1] : 0.0;
            var backwardLength = route.Backward.Stops.Count > 0 && route.Backward.StopDistances != null ? route.Backward.StopDistances[0, route.Backward.Stops.Count - 1] : 0.0;
            mapped.Length = forwardLength > backwardLength ? forwardLength : backwardLength;
            mapped.Length = Math.Round(mapped.Length, 2);

            return mapped as Y;
        }

        public override Y GetById<Y>(string id)
        {
            return GetByIdAsync<Y>(id).GetAwaiter().GetResult();
        }

        private List<ViewRoute> MappToRouteModels(List<Route> routes)
        {
            var mappedEntities = new List<ViewRoute>();
            var messages = _repository.GetCollection<Message>()
                .Find(Builders<Message>.Filter.And(
                    Builders<Message>.Filter.In(x => x.RouteId, routes.Select(x => x.Id)),
                    Builders<Message>.Filter.Gte(x => x.EndDate, DateTime.Now),
                    Builders<Message>.Filter.Lte(x => x.StartDate, DateTime.Now)
                ))
                .ToList();

            foreach (var item in routes)
            {
                var mapped = _mapper.Map<ViewRoute>(item);
                var message = messages.FirstOrDefault(x => x.RouteId == item.Id);

                var forwardLength = item.Forward.Stops.Count > 0 && item.Forward.StopDistances != null ? item.Forward.StopDistances[0, item.Forward.Stops.Count - 1] : 0.0;
                var backwardLength = item.Backward.Stops.Count > 0 && item.Backward.StopDistances != null ? item.Backward.StopDistances[0, item.Backward.Stops.Count - 1] : 0.0;

                mapped.Length = forwardLength > backwardLength ? forwardLength : backwardLength;
                mapped.Length = Math.Round(mapped.Length, 2);
                mapped.Message = _mapper.Map<MessageForViewRoute>(message);

                mappedEntities.Add(mapped);
            }

            return mappedEntities;
        }

        private List<CompactRoute> MappToCompactRouteModels(List<Route> routes)
        {
            var mappedEntities = new List<CompactRoute>();
            var messages = _repository.GetCollection<Message>()
                .Find(Builders<Message>.Filter.And(
                    Builders<Message>.Filter.In(x => x.RouteId, routes.Select(x => x.Id)),
                    Builders<Message>.Filter.Gte(x => x.EndDate, DateTime.Now),
                    Builders<Message>.Filter.Lte(x => x.StartDate, DateTime.Now)
                ))
                .ToList();

            foreach (var item in routes)
            {
                var mapped = _mapper.Map<CompactRoute>(item);
                var message = messages.FirstOrDefault(x => x.RouteId == item.Id);
                mapped.Message = _mapper.Map<MessageForCompactRoute>(message);

                mappedEntities.Add(mapped);
            }

            return mappedEntities;
        }

        private static List<FilterDefinition<Route>> GetRouteBaseFilterDefinitions()
        {
            var filters = new List<FilterDefinition<Route>> { Builders<Route>.Filter.Eq(x => x.Active, true) };

            filters.Add(Builders<Route>.Filter.Or(
                Builders<Route>.Filter.Eq(x => x.TempRoute, false),
                Builders<Route>.Filter.And(
                    Builders<Route>.Filter.Eq(x => x.TempRoute, true),
                    Builders<Route>.Filter.Lte(x => x.TempStart, DateTime.Now),
                    Builders<Route>.Filter.Gt(x => x.TempEnd, DateTime.Now)
            )));

            return filters;
        }
    }
}