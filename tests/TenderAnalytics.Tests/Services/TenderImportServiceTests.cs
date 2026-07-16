using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TenderAnalytics.Application.DTOs.External.Feed;
using TenderAnalytics.Application.DTOs.External.Tender;
using TenderAnalytics.Application.DTOs.Import;
using TenderAnalytics.Application.Interfaces.External;
using TenderAnalytics.Application.Interfaces.Mapping;
using TenderAnalytics.Application.Interfaces.Repositories;
using TenderAnalytics.Application.Services;
using TenderAnalytics.Domain.Entities;

namespace TenderAnalytics.Tests.Services;

public sealed class TenderImportServiceTests
{
    private readonly Mock<ITenderApiClient> _apiClientMock = new();
    private readonly Mock<ITenderMapper> _mapperMock = new();
    private readonly Mock<ITenderRepository> _repositoryMock = new();
    private readonly Mock<ILogger<TenderImportService>> _loggerMock = new();

    private readonly TenderImportService _service;

    public TenderImportServiceTests()
    {
        _service = new TenderImportService(
            _apiClientMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ImportTenderAsync_ShouldSaveTender_WhenTenderMatchesCriteria()
    {
        var source = CreateMatchingTenderDto();
        var mappedTender = CreateMappedTender();

        _apiClientMock
            .Setup(client => client.GetTenderAsync(
                "tender-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenderResponse
            {
                Data = source
            });

        _mapperMock
            .Setup(mapper => mapper.Map(source))
            .Returns(mappedTender);

        var result = await _service.ImportTenderAsync("tender-1");

        result.Should().BeTrue();

        _mapperMock.Verify(
            mapper => mapper.Map(source),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.UpsertAsync(
                mappedTender,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ImportTenderAsync_ShouldReturnFalse_WhenCpvDoesNotMatch()
    {
        var source = CreateMatchingTenderDto(
            cpvCode: "99999999-9");

        _apiClientMock
            .Setup(client => client.GetTenderAsync(
                "tender-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenderResponse
            {
                Data = source
            });

        var result = await _service.ImportTenderAsync("tender-1");

        result.Should().BeFalse();

        _mapperMock.Verify(
            mapper => mapper.Map(
                It.IsAny<TenderDto>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.UpsertAsync(
                It.IsAny<Tender>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ImportTenderAsync_ShouldReturnFalse_WhenStatusDoesNotMatch()
    {
        var source = CreateMatchingTenderDto(
            status: "active");

        _apiClientMock
            .Setup(client => client.GetTenderAsync(
                "tender-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenderResponse
            {
                Data = source
            });

        var result = await _service.ImportTenderAsync("tender-1");

        result.Should().BeFalse();

        _repositoryMock.Verify(
            repository => repository.UpsertAsync(
                It.IsAny<Tender>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ImportTenderAsync_ShouldReturnFalse_WhenDateIsOutsideRange()
    {
        var source = CreateMatchingTenderDto(
            dateCreated: new DateTimeOffset(
                2026,
                1,
                2,
                0,
                0,
                0,
                TimeSpan.Zero));

        _apiClientMock
            .Setup(client => client.GetTenderAsync(
                "tender-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenderResponse
            {
                Data = source
            });

        var result = await _service.ImportTenderAsync("tender-1");

        result.Should().BeFalse();

        _repositoryMock.Verify(
            repository => repository.UpsertAsync(
                It.IsAny<Tender>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ImportFeedAsync_ShouldProcessPagesAndImportMatchingTender()
    {
        var request = new ImportRequest
        {
            DateFrom = new DateTimeOffset(
                2025, 12, 1, 0, 0, 0,
                TimeSpan.Zero),

            DateTo = new DateTimeOffset(
                2026, 1, 1, 0, 0, 0,
                TimeSpan.Zero),

            MaxPages = 1,
            MaxConcurrency = 2
        };

        var source = CreateMatchingTenderDto();
        var mappedTender = CreateMappedTender();

        _apiClientMock
            .Setup(client => client.GetFeedPageAsync(
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FeedResponse
            {
                Data =
                [
                    new FeedTenderDto
                    {
                        Id = "tender-1",
                        Status = "complete",
                        DateCreated = new DateTimeOffset(
                            2025, 12, 15, 0, 0, 0,
                            TimeSpan.Zero)
                    },

                    new FeedTenderDto
                    {
                        Id = "tender-outside-range",
                        Status = "complete",
                        DateCreated = new DateTimeOffset(
                            2026, 1, 15, 0, 0, 0,
                            TimeSpan.Zero)
                    }
                ]
            });

        _apiClientMock
            .Setup(client => client.GetTenderAsync(
                "tender-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenderResponse
            {
                Data = source
            });

        _mapperMock
            .Setup(mapper => mapper.Map(source))
            .Returns(mappedTender);

        var result = await _service.ImportFeedAsync(request);

        result.PagesProcessed.Should().Be(1);
        result.FeedItemsProcessed.Should().Be(2);
        result.CandidatesCount.Should().Be(1);
        result.ImportedCount.Should().Be(1);
        result.SkippedCount.Should().Be(0);
        result.FailedCount.Should().Be(0);
        result.Errors.Should().BeEmpty();

        _repositoryMock.Verify(
            repository => repository.UpsertAsync(
                mappedTender,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ImportFeedAsync_ShouldCountDownloadFailure_AndContinue()
    {
        var request = new ImportRequest
        {
            DateFrom = new DateTimeOffset(
                2025, 12, 1, 0, 0, 0,
                TimeSpan.Zero),

            DateTo = new DateTimeOffset(
                2026, 1, 1, 0, 0, 0,
                TimeSpan.Zero),

            MaxPages = 1,
            MaxConcurrency = 2
        };

        _apiClientMock
            .Setup(client => client.GetFeedPageAsync(
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FeedResponse
            {
                Data =
                [
                    new FeedTenderDto
                    {
                        Id = "broken-tender",
                        Status = "complete",
                        DateCreated = new DateTimeOffset(
                            2025, 12, 15, 0, 0, 0,
                            TimeSpan.Zero)
                    }
                ]
            });

        _apiClientMock
            .Setup(client => client.GetTenderAsync(
                "broken-tender",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(
                "External API is unavailable."));

        var result = await _service.ImportFeedAsync(request);

        result.PagesProcessed.Should().Be(1);
        result.CandidatesCount.Should().Be(1);
        result.ImportedCount.Should().Be(0);
        result.FailedCount.Should().Be(1);
        result.Errors.Should().ContainSingle();
        result.Errors.Single().Should()
            .Contain("broken-tender");

        _repositoryMock.Verify(
            repository => repository.UpsertAsync(
                It.IsAny<Tender>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ImportFeedAsync_ShouldThrow_WhenMaxPagesIsInvalid()
    {
        var request = new ImportRequest
        {
            DateFrom = new DateTimeOffset(
                2025, 12, 1, 0, 0, 0,
                TimeSpan.Zero),

            DateTo = new DateTimeOffset(
                2026, 1, 1, 0, 0, 0,
                TimeSpan.Zero),

            MaxPages = 0,
            MaxConcurrency = 8
        };

        var action = async () =>
            await _service.ImportFeedAsync(request);

        await action.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*MaxPages*");

        _apiClientMock.VerifyNoOtherCalls();
        _repositoryMock.VerifyNoOtherCalls();
    }

    private static TenderDto CreateMatchingTenderDto(
        string status = "complete",
        string cpvCode = "09310000-5",
        DateTimeOffset? dateCreated = null)
    {
        return new TenderDto
        {
            Id = "tender-1",
            Status = status,

            DateCreated = dateCreated ??
                new DateTimeOffset(
                    2025,
                    12,
                    15,
                    0,
                    0,
                    0,
                    TimeSpan.Zero),

            Value = new TenderValueDto
            {
                Amount = 1_000_000m,
                Currency = "UAH"
            },

            ProcuringEntity = new OrganizationDto
            {
                Name = "Test procurer"
            },

            Items =
            [
                new TenderItemDto
                {
                    Classification =
                        new ClassificationDto
                        {
                            Id = cpvCode,
                            Scheme = "ДК021"
                        }
                }
            ]
        };
    }

    private static Tender CreateMappedTender()
    {
        return new Tender
        {
            Id = "tender-1",
            Status = "complete",
            DateCreated = new DateTimeOffset(
                2025,
                12,
                15,
                0,
                0,
                0,
                TimeSpan.Zero),

            ExpectedAmount = 1_000_000m,
            Currency = "UAH",
            CpvCode = "09310000-5",
            ProcuringEntityName = "Test procurer",
            ImportedAt = DateTimeOffset.UtcNow
        };
    }
}