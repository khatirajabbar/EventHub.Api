using AutoMapper;
using EventHub.Api.DTOs.Event;
using EventHub.Api.DTOs.Organizer;
using EventHub.Api.DTOs.Ticket;
using EventHub.Api.Entities;

namespace EventHub.Api.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Event, EventResponseDto>();
        CreateMap<EventCreateDto, Event>();
        CreateMap<EventUpdateDto, Event>();

        CreateMap<Organizer, OrganizerResponseDto>();
        CreateMap<OrganizerCreateDto, Organizer>();

        CreateMap<Ticket, TicketResponseDto>();
        CreateMap<TicketCreateDto, Ticket>();
    }
}