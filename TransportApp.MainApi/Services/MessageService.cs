using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransportApp.EntityModel;
using TransportApp.MainApi.Factory;
using TransportApp.MainApi.Models.Device;
using TransportApp.MainApi.Models.Message;

namespace TransportApp.MainApi.Services
{
    public class MessageService : BaseService<Message>
    {
        public MessageService(IMapper mapper, IHttpContextAccessor httpContextAccessor, RepositoryFactory factory)
            : base(mapper, httpContextAccessor, factory)
        { }

        public async Task<List<ViewMessage>> GetAllGeneralAsync(string deviceId)
        {
            var messages = _repository.GetCollection<Message>()
                .Find(x => x.Type == MessageType.General && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now)
                .ToList();

            var mappedEntities = _mapper.Map<List<ViewMessage>>(messages);

            var user = await _repository.GetCollection<Device>().Find(x => x.DeviceToken == deviceId).SingleOrDefaultAsync();
            if (user != null)
            {
                mappedEntities.ForEach(x => x.IsReaden = user.ReadedMessagesId.Contains(x.Id));

                await MarkAsReadedAsync(deviceId, mappedEntities.Select(x => x.Id).ToList());
            }

            return mappedEntities;
        }

        public async Task<DeviceMessage> GetCountAsync(string deviceId)
        {
            var messages = (await _repository.GetCollection<Message>()
                .Find(x => x.Type == MessageType.General && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now)
                .ToListAsync())
                .Select(x => x.Id);

            var device = _repository.GetCollection<Device>().Find(x => x.DeviceToken == deviceId).FirstOrDefault();

            if (device == null)
            {
                return new DeviceMessage() { MessageCount = messages.Count() };
            }
            else
            {
                return new DeviceMessage() { MessageCount = messages.Except(device.ReadedMessagesId).Count() };
            }
        }

        private Task MarkAsReadedAsync(string deviceId, List<string> messageIds)
        {
            return _repository.GetCollection<Device>().UpdateOneAsync(x => x.DeviceToken == deviceId,
                Builders<Device>.Update.Set(x => x.ReadedMessagesId, messageIds)
                    .Set(x => x.UpdatedAt, DateTime.Now));
        }
    }
}
