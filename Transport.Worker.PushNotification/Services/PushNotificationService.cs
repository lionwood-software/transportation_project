using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MongoDB.Driver;
using Repository.Core;
using TransportApp.EntityModel;
using MessageType = TransportApp.EntityModel.MessageType;


namespace Transport.Worker.PushNotification.Services
{
    public class PushNotificationService
    {
        private readonly IRepository _repository;
        private readonly FirebaseApp _defaultApp;
        private WorkerConfigurationOptions _configuration { get; set; }

        public PushNotificationService(IRepository repository, WorkerConfigurationOptions configuration)
        {
            _repository = repository;
            _configuration = configuration;
            _defaultApp = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(configuration.JsonConfig),
            });
        }

        public async Task Send()
        {
            var messages = _repository.GetCollection<Message>().Find(x => x.StartDate < DateTime.Now && x.EndDate > DateTime.Now && x.Type == MessageType.General && !x.Sended).ToList();
            var devices = _repository.GetCollection<Device>().AsQueryable().ToList();
            var fcm = FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance;

            var devicesUA = devices.Where(x => x.Language.ToLower() == "ua").Select(x => x.DeviceToken).ToList();
            var devicesEN = devices.Where(x => x.Language.ToLower() == "en").Select(x => x.DeviceToken).ToList();
            var devicesRU = devices.Where(x => x.Language.ToLower() == "ru").Select(x => x.DeviceToken).ToList();

            foreach (var message in messages)
            {
                if (devicesUA.Any())
                {
                    await fcm.SendMulticastAsync(new FirebaseAdmin.Messaging.MulticastMessage()
                    {
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = _configuration.TitleMessage,
                            Body = message.MessageUA
                        },
                        Tokens = devicesUA,
                        Data = new Dictionary<string, string>() { { "message", message.MessageUA } }
                    });
                }

                if (devicesEN.Any())
                {
                    await fcm.SendMulticastAsync(new FirebaseAdmin.Messaging.MulticastMessage()
                    {
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = _configuration.TitleMessage,
                            Body = message.MessageEN
                        },
                        Tokens = devicesEN,
                        Data = new Dictionary<string, string>() { { "message", message.MessageEN } }
                    });
                }

                if (devicesRU.Any())
                {
                    await fcm.SendMulticastAsync(new FirebaseAdmin.Messaging.MulticastMessage()
                    {
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = _configuration.TitleMessage,
                            Body = message.MessageRU
                        },
                        Tokens = devicesRU,
                        Data = new Dictionary<string, string>() { { "message", message.MessageRU } }
                    });
                }
            }

            await _repository.GetCollection<Message>().UpdateManyAsync(Builders<Message>.Filter.In(i => i.Id, messages.Select(x => x.Id)),
               Builders<Message>.Update.Set(i => i.Sended, true));
        }
    }
}
