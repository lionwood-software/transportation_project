using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using TransportApp.MainApi.Factory;
using StopStationEntity = TransportApp.EntityModel.StopStation;
using StreetEntity = TransportApp.EntityModel.Street;
using TransportApp.NominatimClient;
using System.Threading.Tasks;
using TransportApp.MainApi.Helpers;
using TransportApp.MainApi.Models.Search;

namespace TransportApp.MainApi.Services
{
    public class SearchService : BaseService<StopStationEntity>
    {
        private string _cityCode;
        private readonly NominatimApiClient _nominatimApiClient;

        private readonly Dictionary<string, List<string>> _allowedClassesTypes = new Dictionary<string, List<string>> {
            { "aeroway", new List<string> { "aerodrome", "hangar" } },
            { "highway", new List<string> {
                "tertiary",
                "residential",
                "service"
            } },
            { "amenity", new List<string> {
                "arts_centre",
                "cinema",
                "college",
                "community_centre",
                "courthouse",
                "embassy",
                "ferry_terminal",
                "fountain",
                "grave_yard",
                "hospital",
                "kindergarten",
                "library",
                "nightclub",
                "nursing_home",
                "police",
                "post_office",
                "prison",
                "school",
                "theatre",
                "university",
                "townhall",
                "fast_food",
                "pharmacy"
            } },
            { "building", new List<string> {
                "church",
                "city_hall",
                "dormitory",
                "faculty",
                "hospital",
                "hotel",
                "industrial",
                "office",
                "school",
                "stadium",
                "university",
                "yes",
                "apartments",
                "house",
                "kindergarten"
            } },
            { "historic", new List<string> { "memorial", "monument" } },
            { "landuse", new List<string> {
                "cemetery",
                "commercial",
                "conservation",
                "forest",
                "wood",
                "retail",
                "recreation_ground",

            } },
            { "leisure", new List<string> {
                "beach_resort",
                "nature_reserve",
                "park",
                "sports_centre",
                "water_park",
                "swimming_pool",
                "stadium",
                "ice_rink",
                "marina",
            } },
            { "natural", new List<string> { "beach" } },
            { "shop", new List<string> {
                "department_store",
                "farm",
                "general",
                "mall",
                "shopping_centre",
                "supermarket"
            } },
            { "tourism", new List<string> { "attraction", "museum", "zoo" } },

        };

        public SearchService(IMapper mapper, IHttpContextAccessor httpContextAccessor, RepositoryFactory factory, NominatimApiClient nominatimApiClient)
            : base(mapper, httpContextAccessor, factory)
        {
            if (!httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("cityCode", out object cityObject))
            {
                _cityCode = "dnipro";
            }
            else
            {
                _cityCode = cityObject.ToString();
            }

            _nominatimApiClient = nominatimApiClient;
        }

        public async Task<SearchStopsResult> FindStopsAsync(string searchString, string language = null)
        {
            if (string.IsNullOrEmpty(searchString) || searchString.Length < 3)
            {
                return new SearchStopsResult()
                {
                    Places = new List<Stop>(),
                    Stops = new List<Stop>()
                };
            }

            if (string.IsNullOrEmpty(language))
            {
                language = SearchHelper.DetectLanguage(searchString);
            }
            language = language.ToLower();

            List<StreetEntity> streets;
            List<StopStationEntity> stops;

            switch (language)
            {
                case "ua":
                    streets = _repository.GetCollection<StreetEntity>()
                        .Find(x => x.NameUA.ToLower().Contains(searchString.ToLower()) || x.AltNameUA.ToLower().Contains(searchString.ToLower()))
                        .ToList();
                    stops = _repository.GetCollection<StopStationEntity>()
                       .Find(x => x.NameUA.ToLower().Contains(searchString.ToLower()))
                       .ToList();
                    break;
                case "ru":
                    streets = _repository.GetCollection<StreetEntity>()
                        .Find(x => x.NameRU.ToLower().Contains(searchString.ToLower()) || x.AltNameRU.ToLower().Contains(searchString.ToLower()))
                        .ToList();
                    stops = _repository.GetCollection<StopStationEntity>()
                       .Find(x => x.NameRU.ToLower().Contains(searchString.ToLower()))
                       .ToList();
                    break;
                case "en":
                    streets = _repository.GetCollection<StreetEntity>()
                        .Find(x => x.NameEN.ToLower().Contains(searchString.ToLower()) || x.AltNameEN.ToLower().Contains(searchString.ToLower()))
                        .ToList();
                    stops = _repository.GetCollection<StopStationEntity>()
                       .Find(x => x.NameEN.ToLower().Contains(searchString.ToLower()))
                       .ToList();
                    break;
                default:
                    streets = new List<StreetEntity>();
                    stops = new List<StopStationEntity>();
                    break;
            }

            List<NominatimResponce> nominatimResult;

            if (streets.Any())
            {
                var tasks = streets.Select(x => RequestNominatimSearchAsync(x.NameUA, language, _cityCode)).ToList();
                var results = await Task.WhenAll(tasks);
                nominatimResult = results.SelectMany(x => x).ToList();
            }
            else
            {
                nominatimResult = await RequestNominatimSearchAsync(searchString, language, _cityCode);
            }

            nominatimResult = nominatimResult.Where(x => (_allowedClassesTypes.ContainsKey(x.@class) && _allowedClassesTypes[x.@class].Contains(x.type))
                || (x.address.ContainsKey("road") && x.address["road"].Contains(searchString, System.StringComparison.InvariantCultureIgnoreCase))).ToList();

            return new SearchStopsResult
            {
                Places = nominatimResult.Select(x =>
                {
                    string name = string.Empty;
                    if (x.address.ContainsKey(x.type))
                    {
                        name += $"\"{x.address[x.type]}\" ";
                    }
                    if (x.address.ContainsKey("road"))
                    {
                        name += $"{x.address["road"]} ";
                    }
                    if (x.address.ContainsKey("house_number"))
                    {
                        name += $"{x.address["house_number"]} ";
                    }

                    return new Stop
                    {
                        Name = name.Trim(),
                        Lat = x.lat,
                        Lon = x.lon
                    };
                }).GroupBy(x => x.Name).Select(x => x.FirstOrDefault()).ToList(),
                Stops = stops.Select(x => new Stop
                {
                    Name = language == "ru" ? x.NameRU : language == "en" ? x.NameEN : x.NameUA,
                    Lat = x.Geolocation.Latitude,
                    Lon = x.Geolocation.Longitude
                }).ToList()
            };
        }

        public async Task<SearchAddressResult> FindAddressByLocationAsync(double lat, double lon, string language = "ua")
        {
            var result = await _nominatimApiClient.GetAsync<NominatimResponce>($"/reverse.php?format=json&lat={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lon={lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}&addressdetails=1&accept-language={language}&limit=1");

            string name = string.Empty;
            if (result.address.ContainsKey("road"))
            {
                name += $"{result.address["road"]} ";
            }
            else if (result.address.ContainsKey("pedestrian"))
            {
                name += $"{result.address["pedestrian"]} ";
            }

            if (result.address.ContainsKey("house_number"))
            {
                name += $"{result.address["house_number"]} ";
            }

            return new SearchAddressResult { Address = name.Trim() };
        }

        private Task<List<NominatimResponce>> RequestNominatimSearchAsync(string searchString, string language, string cityCode)
        {
            var query = SearchHelper.BuildSearchQuery(searchString, language, cityCode);
            return _nominatimApiClient.GetAsync<List<NominatimResponce>>($"/search.php?q={query}&format=json&addressdetails=1&limit=50&accept-language={language}");
        }
    }
}
