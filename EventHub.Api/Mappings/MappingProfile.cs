using AutoMapper;
using EventHub.Api.DTOs.Event;
using EventHub.Api.DTOs.Organizer;
using EventHub.Api.DTOs.Ticket;
using EventHub.Api.DTOs.User;
using EventHub.Api.Entities;

namespace EventHub.Api.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Event, EventResponseDto>()
            .ForMember(dest => dest.Organizer, opt => opt.MapFrom(src => src.Organizer));
        CreateMap<EventCreateDto, Event>();
        CreateMap<EventUpdateDto, Event>();
        CreateMap<OrganizerUpdateDto, Organizer>();
        CreateMap<TicketUpdateDto, Ticket>();

        CreateMap<Organizer, OrganizerResponseDto>();
        CreateMap<OrganizerCreateDto, Organizer>();

        CreateMap<Ticket, TicketResponseDto>();
        CreateMap<TicketCreateDto, Ticket>();

        CreateMap<User, UserResponseDto>();
    }
}