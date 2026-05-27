using AutoMapper;
using EventHub.Api.Controllers;
using EventHub.Api.Data;
using EventHub.Api.DTOs.Event;
using EventHub.Api.Entities;
using EventHub.Api.Mappings;
using EventHub.Api.Services;
using EventHub.Api.Tests.Fixtures;
using EventHub.Api.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EventHub.Api.Tests.Controllers;

public class EventsControllerTests
{
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly FileService _fileService;

    public EventsControllerTests()
    {
        _context = DbContextFixture.CreateInMemoryDbContext();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfile());
        });
        _mapper = mapperConfig.CreateMapper();

        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        _fileService = new FileService(_mockEnv.Object);
    }

    private EventsController CreateController()
    {
        var controller = new EventsController(_context, _mapper, _fileService);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        return controller;
    }

    #region GET GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithEvents()
    {
        // ARRANGE
        var organizer = TestDataBuilder.BuildOrganizer(1);
        var evt1 = TestDataBuilder.BuildEvent(1, 1, "Event 1");
        var evt2 = TestDataBuilder.BuildEvent(2, 1, "Event 2");

        _context.Organizers.Add(organizer);
        _context.Events.AddRange(evt1, evt2);
        await _context.SaveChangesAsync();

        var controller = CreateController();

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var events = okResult.Value.Should().BeAssignableTo<IEnumerable<EventResponseDto>>().Subject;
        events.Count().Should().BeGreaterThanOrEqualTo(2);
    }

    #endregion

    #region GET GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkResult()
    {
        // ARRANGE
        var organizer = TestDataBuilder.BuildOrganizer(2, "Org 2");
        var evt = TestDataBuilder.BuildEvent(3, 2, "Specific Event");
        
        _context.Organizers.Add(organizer);
        _context.Events.Add(evt);
        await _context.SaveChangesAsync();

        var controller = CreateController();

        // ACT
        var result = await controller.GetById(3);

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var eventDto = okResult.Value.Should().BeOfType<EventResponseDto>().Subject;
        eventDto.Id.Should().Be(3);
        eventDto.Title.Should().Be("Specific Event");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // ARRANGE
        var controller = CreateController();

        // ACT
        var result = await controller.GetById(999);

        // ASSERT
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region POST Create Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAtAction()
    {
        // ARRANGE
        var organizer = TestDataBuilder.BuildOrganizer(3, "Org 3");
        _context.Organizers.Add(organizer);
        await _context.SaveChangesAsync();

        var controller = CreateController();
        var dto = new EventCreateDto
        {
            Title = "New Event",
            Description = "New Desc",
            Date = DateTime.UtcNow.AddDays(5),
            Location = "New Location",
            OrganizerId = 3
        };

        // ACT
        var result = await controller.Create(dto);

        // ASSERT
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(EventsController.GetById));
        var eventDto = createdResult.Value.Should().BeOfType<EventResponseDto>().Subject;
        eventDto.Title.Should().Be("New Event");
        eventDto.OrganizerId.Should().Be(3);
    }

    [Fact]
    public async Task Create_WithInvalidOrganizer_ReturnsBadRequest()
    {
        // ARRANGE
        var controller = CreateController();
        var dto = new EventCreateDto
        {
            Title = "New Event",
            Location = "Location",
            OrganizerId = 999 // Non-existent
        };

        // ACT
        var result = await controller.Create(dto);

        // ASSERT
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().BeEquivalentTo(new { error = "Organizer with the specified ID does not exist." });
    }

    #endregion

    #region PUT Update Tests

    [Fact]
    public async Task Update_WithValidData_ReturnsOkResult()
    {
        // ARRANGE
        var organizer = TestDataBuilder.BuildOrganizer(4);
        var evt = TestDataBuilder.BuildEvent(4, 4, "Old Title");
        
        _context.Organizers.Add(organizer);
        _context.Events.Add(evt);
        await _context.SaveChangesAsync();

        var controller = CreateController();
        var updateDto = new EventUpdateDto
        {
            Title = "Updated Title",
            Description = "Updated Desc",
            Date = DateTime.UtcNow.AddDays(15),
            Location = "Updated Location",
            OrganizerId = 4
        };

        // ACT
        var result = await controller.Update(4, updateDto);

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var eventDto = okResult.Value.Should().BeOfType<EventResponseDto>().Subject;
        eventDto.Title.Should().Be("Updated Title");
        eventDto.Location.Should().Be("Updated Location");
    }

    [Fact]
    public async Task Update_WithInvalidEventId_ReturnsNotFound()
    {
        // ARRANGE
        var controller = CreateController();
        var updateDto = new EventUpdateDto
        {
            Title = "Updated Title",
            Location = "Loc",
            OrganizerId = 1
        };

        // ACT
        var result = await controller.Update(999, updateDto);

        // ASSERT
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // ARRANGE
        var evt = TestDataBuilder.BuildEvent(5);
        _context.Events.Add(evt);
        await _context.SaveChangesAsync();

        var controller = CreateController();

        // ACT
        var result = await controller.Delete(5);

        // ASSERT
        result.Should().BeOfType<NoContentResult>();
        
        var deletedEvent = await _context.Events.FindAsync(5);
        deletedEvent.Should().BeNull();
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        // ARRANGE
        var controller = CreateController();

        // ACT
        var result = await controller.Delete(999);

        // ASSERT
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion
}
