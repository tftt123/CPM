using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Modules.Approval.Contracts;
using CpmServer.Modules.PM.DTOs;
using CpmServer.Modules.PM.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CpmServer.Tests.Services;

public class PmProjectTraceServiceTests
{
    private static CpmDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CpmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(b => b.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new CpmDbContext(options);
    }

    [Fact]
    public async Task SubmitCycleTimeChangeRequestAsync_Should_Create_Request_With_Status_Zero()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var mockApproval = new Mock<IApprovalService>();
        mockApproval
            .Setup(x => x.StartApprovalAsync(
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<long>()))
            .ReturnsAsync(new SysApprovalInstance { Id = 99 });

        var service = new PmProjectTraceService(db, mockApproval.Object);

        var step = new PmProjectTraceStep
        {
            ProjectTraceId = 1,
            StepOrder = 1,
            ProcessName = "TestProcess"
        };
        db.ProjectTraceSteps.Add(step);
        await db.SaveChangesAsync();

        var changes = new List<PmStepCycleTimeChangeDetailDto>
        {
            new() { ChangeType = 0, RecordDate = DateTime.UtcNow, ActualCycleTime = 120 }
        };

        // Act
        var requestId = await service.SubmitCycleTimeChangeRequestAsync(
            step.Id, 1, "1", "TestUser", changes);

        // Assert
        var request = await db.StepCycleTimeChangeRequests.FindAsync(requestId);
        request.Should().NotBeNull();
        request!.ApprovalStatus.Should().Be(0);
        request.StepId.Should().Be(step.Id);
        request.Details.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllStepsWithLatestCycleTimeAsync_Should_Return_Only_Active_Status()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var mockApproval = new Mock<IApprovalService>();
        var service = new PmProjectTraceService(db, mockApproval.Object);

        var trace = new PmProjectTrace
        {
            CustomerName = "CustomerA",
            ProductCode = "P001",
            Status = 0
        };
        db.ProjectTraces.Add(trace);
        await db.SaveChangesAsync();

        var step = new PmProjectTraceStep
        {
            ProjectTraceId = trace.Id,
            StepOrder = 1,
            ProcessName = "Cutting"
        };
        db.ProjectTraceSteps.Add(step);
        await db.SaveChangesAsync();

        // Add one expired and one active actual cycle time record
        step.ActualCycleTimes.Add(new PmProjectTraceStepActualCycleTime
        {
            ProjectTraceStepId = step.Id,
            RecordDate = DateTime.UtcNow.AddDays(-2),
            ActualCycleTime = 100,
            Status = 1 // expired
        });
        step.ActualCycleTimes.Add(new PmProjectTraceStepActualCycleTime
        {
            ProjectTraceStepId = step.Id,
            RecordDate = DateTime.UtcNow.AddDays(-1),
            ActualCycleTime = 150,
            Status = 0 // active
        });
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetAllStepsWithLatestCycleTimeAsync(null);

        // Assert
        result.Should().HaveCount(1);
        var item = result.First();
        item.LatestActualCycleTime.Should().Be(150);
    }

    [Fact]
    public async Task ExecuteApprovedChangeRequestAsync_Should_Mark_Old_Records_Expired()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var mockApproval = new Mock<IApprovalService>();
        var service = new PmProjectTraceService(db, mockApproval.Object);

        var trace = new PmProjectTrace
        {
            CustomerName = "CustomerB",
            ProductCode = "P002",
            Status = 0
        };
        db.ProjectTraces.Add(trace);
        await db.SaveChangesAsync();

        var step = new PmProjectTraceStep
        {
            ProjectTraceId = trace.Id,
            StepOrder = 1,
            ProcessName = "Milling"
        };
        db.ProjectTraceSteps.Add(step);
        await db.SaveChangesAsync();

        // Add an old active record
        var oldRecord = new PmProjectTraceStepActualCycleTime
        {
            ProjectTraceStepId = step.Id,
            RecordDate = DateTime.UtcNow.AddDays(-5),
            ActualCycleTime = 200,
            Status = 0
        };
        step.ActualCycleTimes.Add(oldRecord);
        await db.SaveChangesAsync();

        var request = new PmStepCycleTimeChangeRequest
        {
            StepId = step.Id,
            TraceId = trace.Id,
            ApprovalStatus = 0,
            Details = new List<PmStepCycleTimeChangeDetail>
            {
                new()
                {
                    ChangeType = 0, // 新增
                    RecordDate = DateTime.UtcNow,
                    ActualCycleTime = 250,
                    CreatedAt = DateTime.UtcNow
                }
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.StepCycleTimeChangeRequests.Add(request);
        await db.SaveChangesAsync();

        // Act
        await service.ExecuteApprovedChangeRequestAsync(request.Id);

        // Assert
        var refreshedOld = await db.StepActualCycleTimes.FindAsync(oldRecord.Id);
        refreshedOld.Should().NotBeNull();
        refreshedOld!.Status.Should().Be(1); // marked expired

        var newRecords = db.StepActualCycleTimes.Where(a => a.Status == 0).ToList();
        newRecords.Should().HaveCount(1);
        newRecords.First().ActualCycleTime.Should().Be(250);
    }
}
