using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Repository.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransportApp.EntityModel;
using TransportApp.MainApi.Factory;

namespace TransportApp.MainApi.Services
{
    public class BaseService<T> where T : BaseEntity, new()
    {
        protected readonly IRepository _repository;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;

        public BaseService(IMapper mapper, IHttpContextAccessor httpContextAccessor, RepositoryFactory factory)
        {
            _mapper = mapper;
            _logger = Log.Logger;
            _repository = GetRepository(httpContextAccessor, factory);
        }

        public virtual List<Y> GetAll<Y>() where Y : class
        {
            var entites = _repository.GetCollection<T>().AsQueryable();

            return _mapper.Map<List<Y>>(entites);
        }

        public virtual Y GetById<Y>(string id) where Y : class
        {
            var entity = GetByIdAsync<Y>(id).Result;

            return _mapper.Map<Y>(entity);
        }

        public virtual async Task<Y> GetByIdAsync<Y>(string id) where Y : class
        {
            var entity = await _repository.GetCollection<T>()
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();

            return _mapper.Map<Y>(entity);
        }

        private IRepository GetRepository(IHttpContextAccessor httpContextAccessor, RepositoryFactory factory)
        {
            try
            {
                var codeResult = httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("cityCode", out object cityCode);

                if (!codeResult)
                {
                    throw new Exception("City not found!");
                }

                return factory.GetRepository(cityCode.ToString());
            }
            catch
            {
                throw new Exception("City not found!");
            }
        }
    }
}
