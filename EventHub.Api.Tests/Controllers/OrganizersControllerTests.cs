using AutoMapper;
using EventHub.Api.Controllers;
using EventHub.Api.Data;
using EventHub.Api.DTOs.Organizer;
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

public class OrganizersControllerTests
{
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly FileService _fileService;

    public OrganizersControllerTests()
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

    private OrganizersController CreateController()
    {
        var controller = new OrganizersController(_context, _mapper, _fileService);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("api.eventhub.com");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        return controller;
    }

    #region GET GetAll & GetById Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithOrganizers()
    {
        // ARRANGE
        var org1 = TestDataBuilder.BuildOrganizer(1, "Org 1");
        var org2 = TestDataBuilder.BuildOrganizer(2, "Org 2");
        _context.Organizers.AddRange(org1, org2);
        await _context.SaveChangesAsync();

        var controller = CreateController();

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var organizers = okResult.Value.Should().BeAssignableTo<IEnumerable<OrganizerResponseDto>>().Subject;
        organizers.Count().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkResult()
    {
        // ARRANGE
        var org = TestDataBuilder.BuildOrganizer(3, "Specific Org");
        _context.Organizers.Add(org);
        await _context.SaveChangesAsync();

        var controller = CreateController();

        // ACT
        var result = await controller.GetById(3);

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var orgDto = okResult.Value.Should().BeOfType<OrganizerResponseDto>().Subject;
        orgDto.Id.Should().Be(3);
        orgDto.Name.Should().Be("Specific Org");
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
        var controller = CreateController();
        var dto = new OrganizerCreateDto
        {
            Name = "New Org",
            Email = "neworg@example.com",
            Phone = "123"
        };

        var result = await controller.Create(dto);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(OrganizersController.GetById));
        var orgDto = createdResult.Value.Should().BeOfType<OrganizerResponseDto>().Subject;
        orgDto.Name.Should().Be("New Org");
        orgDto.Email.Should().Be("neworg@example.com");
    }

    #endregion

    #region PUT Update Tests

    [Fact]
    public async Task Update_WithValidData_ReturnsOkResult()
    {
        var org = TestDataBuilder.BuildOrganizer(4, "Old Org");
        _context.Organizers.Add(org);
        await _context.SaveChangesAsync();

        var controller = CreateController();
        var dto = new OrganizerUpdateDto
        {
            Name = "Updated Org",
            Email = "updated@example.com",
            Phone = "456"
        };

        var result = await controller.Update(4, dto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var orgDto = okResult.Value.Should().BeOfType<OrganizerResponseDto>().Subject;
        orgDto.Name.Should().Be("Updated Org");

        var dbOrg = await _context.Organizers.FindAsync(4);
        dbOrg?.Name.Should().Be("Updated Org");
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        var controller = CreateController();
        var dto = new OrganizerUpdateDto { Name = "Updated Org", Email = "updated@example.com" };
        var result = await controller.Update(999, dto);
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        var org = TestDataBuilder.BuildOrganizer(5);
        _context.Organizers.Add(org);
        await _context.SaveChangesAsync();

        var controller = CreateController();
        var result = await controller.Delete(5);

        result.Should().BeOfType<NoContentResult>();
        var dbOrg = await _context.Organizers.FindAsync(5);
        dbOrg.Should().BeNull();
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

