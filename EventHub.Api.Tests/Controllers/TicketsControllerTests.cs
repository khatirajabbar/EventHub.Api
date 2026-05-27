using AutoMapper;
using EventHub.Api.Controllers;
using EventHub.Api.Data;
using EventHub.Api.DTOs.Ticket;
using EventHub.Api.Entities;
using EventHub.Api.Mappings;
using EventHub.Api.Tests.Fixtures;
using EventHub.Api.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EventHub.Api.Tests.Controllers;

public class TicketsControllerTests
{
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public TicketsControllerTests()
    {
        _context = DbContextFixture.CreateInMemoryDbContext();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfile());
        });
        _mapper = mapperConfig.CreateMapper();
    }

    private TicketsController CreateController()
    {
        return new TicketsController(_context, _mapper);
    }

    #region GET Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithTickets()
    {
        var evt = TestDataBuilder.BuildEvent(1);
        _context.Events.Add(evt);
        var t1 = TestDataBuilder.BuildTicket(1, 1);
        var t2 = TestDataBuilder.BuildTicket(2, 1);
        _context.Tickets.AddRange(t1, t2);
        await _context.SaveChangesAsync();

        var controller = CreateController();

        var result = await controller.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var tickets = okResult.Value.Should().BeAssignableTo<IEnumerable<TicketResponseDto>>().Subject;
        tickets.Count().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkResult()
    {
        var evt = TestDataBuilder.BuildEvent(2);
        var t = TestDataBuilder.BuildTicket(3, 2);
        _context.Events.Add(evt);
        _context.Tickets.Add(t);
        await _context.SaveChangesAsync();

        var controller = CreateController();

        var result = await controller.GetById(3);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<TicketResponseDto>().Subject;
        dto.Id.Should().Be(3);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        var controller = CreateController();
        var result = await controller.GetById(999);
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region POST Create Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAtAction()
    {
        var evt = TestDataBuilder.BuildEvent(3);
        _context.Events.Add(evt);
        await _context.SaveChangesAsync();

        var controller = CreateController();
        var dto = new TicketCreateDto
        {
            EventId = 3,
            Type = EventHub.Api.Enums.TicketType.Standard,
            Price = 100m,
            QuantityAvailable = 50
        };

        var result = await controller.Create(dto);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var responseDto = createdResult.Value.Should().BeOfType<TicketResponseDto>().Subject;
        responseDto.EventId.Should().Be(3);
        responseDto.Price.Should().Be(100m);
    }

    [Fact]
    public async Task Create_WithInvalidEvent_ReturnsBadRequest()
    {
        var controller = CreateController();
        var dto = new TicketCreateDto { EventId = 999 };
        
        var result = await controller.Create(dto);
        
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().BeEquivalentTo(new { error = "Event with the specified ID does not exist." });
    }

    #endregion

    #region PUT Update Tests

    [Fact]
    public async Task Update_WithValidData_ReturnsOkResult()
    {
        var evt = TestDataBuilder.BuildEvent(4);
        var t = TestDataBuilder.BuildTicket(4, 4);
        t.Price = 50m;
        _context.Events.Add(evt);
        _context.Tickets.Add(t);
        await _context.SaveChangesAsync();

        var controller = CreateController();
        var dto = new TicketUpdateDto
        {
            EventId = 4,
            Type = EventHub.Api.Enums.TicketType.VIP,
            Price = 150m,
            QuantityAvailable = 20
        };

        var result = await controller.Update(4, dto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseDto = okResult.Value.Should().BeOfType<TicketResponseDto>().Subject;
        responseDto.Price.Should().Be(150m);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        var controller = CreateController();
        var result = await controller.Update(999, new TicketUpdateDto());
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        var evt = TestDataBuilder.BuildEvent(5);
        var t = TestDataBuilder.BuildTicket(5, 5);
        _context.Events.Add(evt);
        _context.Tickets.Add(t);
        await _context.SaveChangesAsync();

        var controller = CreateController();
        var result = await controller.Delete(5);

        result.Should().BeOfType<NoContentResult>();
        var dbTicket = await _context.Tickets.FindAsync(5);
        dbTicket.Should().BeNull();
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        var controller = CreateController();
        var result = await controller.Delete(999);
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion
}

