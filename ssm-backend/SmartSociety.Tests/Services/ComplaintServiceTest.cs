using Moq;
using NUnit.Framework;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Services;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class ComplaintServiceTest
{
    private Mock<IUnitOfWork> _mockUow;
    private Mock<IComplaintRepository> _mockComplaintRepo;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<IApartmentRepository> _mockApartmentRepo;
    private Mock<INotificationService> _mockNotif;
    private ComplaintService _service;

    [SetUp]
    public void Setup()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockComplaintRepo = new Mock<IComplaintRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockApartmentRepo = new Mock<IApartmentRepository>();
        _mockNotif = new Mock<INotificationService>();

        _mockUow.Setup(u => u.Complaints).Returns(_mockComplaintRepo.Object);
        _mockUow.Setup(u => u.Users).Returns(_mockUserRepo.Object);
        _mockUow.Setup(u => u.Apartments).Returns(_mockApartmentRepo.Object);

        _service = new ComplaintService(_mockUow.Object, _mockNotif.Object);
    }

    [Test]
    public void CreateAsync_ThrowsNotFound_WhenUserMissing()
    {
        var dto = new CreateComplaintDto { ApartmentId = Guid.NewGuid(), Title = "Leak", Description = "Water leakage" };
        Guid userId = Guid.NewGuid();

        _mockUserRepo.Setup(u => u.GetByIdAsync(userId)).ReturnsAsync((User)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.CreateAsync(dto, userId));
    }

    [Test]
    public void CreateAsync_ThrowsNotFound_WhenApartmentMissing()
    {
        var dto = new CreateComplaintDto { ApartmentId = Guid.NewGuid(), Title = "Leak", Description = "Water leakage" };
        Guid userId = Guid.NewGuid();

        _mockUserRepo.Setup(u => u.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(dto.ApartmentId)).ReturnsAsync((Apartment)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.CreateAsync(dto, userId));
    }

    [Test]
    public async Task CreateAsync_CreatesAndReturnsComplaint()
    {
        var dto = new CreateComplaintDto { ApartmentId = Guid.NewGuid(), Title = "Leak", Description = "Water leakage" };
        Guid userId = Guid.NewGuid();

        _mockUserRepo.Setup(u => u.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId, Name = "Test User" });
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(dto.ApartmentId)).ReturnsAsync(new Apartment { Id = dto.ApartmentId, Block = "A", Number = "101" });

        var result = await _service.CreateAsync(dto, userId);

        Assert.That(result.Title, Is.EqualTo(dto.Title));
        Assert.That(result.Description, Is.EqualTo(dto.Description));
        Assert.That(result.Status, Is.EqualTo(ComplaintStatus.Open));
        
        _mockComplaintRepo.Verify(c => c.AddAsync(It.IsAny<Complaint>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetMyComplaintsAsync_ReturnsComplaints()
    {
        Guid userId = Guid.NewGuid();
        var mockComplaints = new List<Complaint> 
        { 
            new() { Id = Guid.NewGuid(), UserId = userId, Title = "Title 1" } 
        };

        _mockComplaintRepo.Setup(c => c.GetByUserIdAsync(userId)).ReturnsAsync(mockComplaints);

        var result = await _service.GetMyComplaintsAsync(userId);

        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Title, Is.EqualTo("Title 1"));
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllComplaints()
    {
        var mockComplaints = new List<Complaint> 
        { 
            new() { Id = Guid.NewGuid(), Title = "Title 1" },
            new() { Id = Guid.NewGuid(), Title = "Title 2" }
        };

        _mockComplaintRepo.Setup(c => c.GetAllAsync()).ReturnsAsync(mockComplaints);

        var result = await _service.GetAllAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public void ResolveAsync_ThrowsNotFound_WhenComplaintMissing()
    {
        Guid complaintId = Guid.NewGuid();
        var dto = new ResolveComplaintDto { AdminResponse = "Resolved" };

        _mockComplaintRepo.Setup(c => c.GetByIdAsync(complaintId)).ReturnsAsync((Complaint)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.ResolveAsync(complaintId, dto));
    }

    [Test]
    public void ResolveAsync_ThrowsBadRequest_WhenComplaintNotOpen()
    {
        Guid complaintId = Guid.NewGuid();
        var dto = new ResolveComplaintDto { AdminResponse = "Resolved" };
        var complaint = new Complaint { Id = complaintId, Status = ComplaintStatus.Resolved };

        _mockComplaintRepo.Setup(c => c.GetByIdAsync(complaintId)).ReturnsAsync(complaint);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.ResolveAsync(complaintId, dto));
    }

    [Test]
    public async Task ResolveAsync_ResolvesComplaintAndNotifies()
    {
        Guid complaintId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var dto = new ResolveComplaintDto { AdminResponse = "Resolved issue" };
        var complaint = new Complaint { Id = complaintId, UserId = userId, Title = "Leak", Status = ComplaintStatus.Open };

        _mockComplaintRepo.Setup(c => c.GetByIdAsync(complaintId)).ReturnsAsync(complaint);

        var result = await _service.ResolveAsync(complaintId, dto);

        Assert.That(result.Status, Is.EqualTo(ComplaintStatus.Resolved));
        Assert.That(result.AdminResponse, Is.EqualTo(dto.AdminResponse));

        _mockComplaintRepo.Verify(c => c.UpdateAsync(complaint), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockNotif.Verify(n => n.CreateAsync(userId, "Complaint Resolved", It.Is<string>(s => s.Contains("Resolved issue"))), Times.Once);
    }
}
