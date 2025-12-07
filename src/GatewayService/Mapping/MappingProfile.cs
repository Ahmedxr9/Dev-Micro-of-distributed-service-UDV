using AutoMapper;
using Shared.DTOs;
using Shared.Models;

namespace GatewayService.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Notification, NotificationResponseDto>()
            .ForMember(dest => dest.NotificationId, opt => opt.MapFrom(src => src.Id));
    }
}

