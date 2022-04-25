using AutoMapper;
using TransportApp.MainApi.Models.Message;
using EntityMessage = TransportApp.EntityModel.Message;

namespace TransportApp.MainApi.Mapper.Profiles
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {           
            CreateMap<EntityMessage, ViewMessage>();
        }        
    }
}
