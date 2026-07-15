using Microsoft.AspNetCore.Mvc;
using TenderAnalytics.Application.DTOs.Analytics;
using TenderAnalytics.Application.Interfaces.Services;

namespace TenderAnalytics.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(
        IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("savings")]
    [ProducesResponseType(
        typeof(BudgetSavingsDto),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<BudgetSavingsDto>>
        GetSavings(
            CancellationToken cancellationToken)
    {
        var result =
            await _analyticsService.GetBudgetSavingsAsync(
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("top-procurers")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<TopProcurerDto>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<IReadOnlyCollection<TopProcurerDto>>>
        GetTopProcurers(
            [FromQuery] int limit = 5,
            CancellationToken cancellationToken = default)
    {
        var result =
            await _analyticsService.GetTopProcurersAsync(
                limit,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("top-suppliers")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<TopSupplierDto>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<IReadOnlyCollection<TopSupplierDto>>>
        GetTopSuppliers(
            [FromQuery] int limit = 5,
            CancellationToken cancellationToken = default)
    {
        var result =
            await _analyticsService.GetTopSuppliersAsync(
                limit,
                cancellationToken);

        return Ok(result);
    }
}