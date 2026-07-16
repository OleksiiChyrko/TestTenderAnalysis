using FluentAssertions;
using TenderAnalytics.Application.DTOs.External.Tender;
using TenderAnalytics.Application.Mapping;

namespace TenderAnalytics.Tests.Mapping;

public sealed class TenderMapperTests
{
    private readonly TenderMapper _mapper = new();

    [Fact]
    public void Map_ShouldMapTenderAndOnlyActiveContracts()
    {
        var source = CreateTenderDto();

        var result = _mapper.Map(source);

        result.Id.Should().Be("tender-1");
        result.Status.Should().Be("complete");
        result.CpvCode.Should().Be("09310000-5");
        result.ExpectedAmount.Should().Be(1_000_000m);
        result.Currency.Should().Be("UAH");
        result.ProcuringEntityIdentifier.Should().Be("12345678");
        result.ProcuringEntityName.Should().Be("Test procurer");

        result.Contracts.Should().ContainSingle();

        var contract = result.Contracts.Single();

        contract.Id.Should().Be("contract-active");
        contract.Status.Should().Be("active");
        contract.Amount.Should().Be(900_000m);
        contract.AwardId.Should().Be("award-active");

        contract.ContractSuppliers.Should().ContainSingle();

        var supplier = contract.ContractSuppliers
            .Single()
            .Supplier;

        supplier.Identifier.Should().Be("87654321");
        supplier.Name.Should().Be("Test   Supplier");
        supplier.NormalizedName.Should().Be("TEST SUPPLIER");
    }

    [Fact]
    public void Map_ShouldIgnoreCancelledContracts()
    {
        var source = CreateTenderDto();

        var result = _mapper.Map(source);

        result.Contracts.Should()
            .NotContain(contract =>
                contract.Status == "cancelled");
    }

    [Fact]
    public void Map_ShouldConvertDateCreatedToUtc()
    {
        var source = CreateTenderDto();

        var result = _mapper.Map(source);

        result.DateCreated.Offset.Should().Be(TimeSpan.Zero);

        result.DateCreated.Should().Be(
            new DateTimeOffset(
                2025,
                12,
                17,
                13,
                58,
                46,
                TimeSpan.Zero));
    }

    [Fact]
    public void Map_ShouldThrow_WhenTenderIdIsMissing()
    {
        var source = new TenderDto
        {
            Id = null,
            Status = "complete",

            DateCreated = new DateTimeOffset(
                2025,
                12,
                17,
                15,
                58,
                46,
                TimeSpan.FromHours(2)),

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
                    Classification = new ClassificationDto
                    {
                        Id = "09310000-5",
                        Scheme = "ДК021"
                    }
                }
            ]
        };

        var action = () => _mapper.Map(source);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*id is missing*");
    }

    [Fact]
    public void Map_ShouldCreateContractWithoutSupplier_WhenAwardIsMissing()
    {
        var original = CreateTenderDto();

        var source = new TenderDto
        {
            Id = original.Id,
            Status = original.Status,
            DateCreated = original.DateCreated,
            DateModified = original.DateModified,
            Value = original.Value,
            ProcuringEntity = original.ProcuringEntity,
            Items = original.Items,
            Contracts = original.Contracts,
            Awards = []
        };

        var result = _mapper.Map(source);

        result.Contracts.Should().ContainSingle();

        result.Contracts
            .Single()
            .ContractSuppliers
            .Should()
            .BeEmpty();
    }

    private static TenderDto CreateTenderDto()
    {
        return new TenderDto
        {
            Id = "tender-1",
            Status = "complete",

            DateCreated = new DateTimeOffset(
                2025,
                12,
                17,
                15,
                58,
                46,
                TimeSpan.FromHours(2)),

            Value = new TenderValueDto
            {
                Amount = 1_000_000m,
                Currency = "UAH"
            },

            ProcuringEntity = new OrganizationDto
            {
                Name = "Test procurer",

                Identifier = new OrganizationIdentifierDto
                {
                    Id = "12345678",
                    Scheme = "UA-EDR"
                }
            },

            Items =
            [
                new TenderItemDto
                {
                    Classification = new ClassificationDto
                    {
                        Id = "09310000-5",
                        Scheme = "ДК021"
                    }
                }
            ],

            Contracts =
            [
                new TenderContractDto
                {
                    Id = "contract-cancelled",
                    AwardId = "award-cancelled",
                    Status = "cancelled",

                    Value = new TenderValueDto
                    {
                        Amount = 950_000m,
                        Currency = "UAH"
                    }
                },

                new TenderContractDto
                {
                    Id = "contract-active",
                    AwardId = "award-active",
                    Status = "active",

                    Value = new TenderValueDto
                    {
                        Amount = 900_000m,
                        Currency = "UAH"
                    }
                }
            ],

            Awards =
            [
                new TenderAwardDto
                {
                    Id = "award-cancelled",
                    Status = "cancelled",

                    Suppliers =
                    [
                        new SupplierDto
                        {
                            Name = "Cancelled Supplier",

                            Identifier =
                                new OrganizationIdentifierDto
                                {
                                    Id = "11111111"
                                }
                        }
                    ]
                },

                new TenderAwardDto
                {
                    Id = "award-active",
                    Status = "active",

                    Suppliers =
                    [
                        new SupplierDto
                        {
                            Name = "  Test   Supplier  ",

                            Identifier =
                                new OrganizationIdentifierDto
                                {
                                    Id = "87654321"
                                }
                        }
                    ]
                }
            ]
        };
    }
}