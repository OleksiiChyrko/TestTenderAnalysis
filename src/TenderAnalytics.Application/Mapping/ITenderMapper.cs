using TenderAnalytics.Application.DTOs.External.Tender;
using TenderAnalytics.Domain.Entities;

namespace TenderAnalytics.Application.Interfaces.Mapping;

public interface ITenderMapper
{
    Tender Map(TenderDto source);
}