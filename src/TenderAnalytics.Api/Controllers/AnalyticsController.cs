using Microsoft.AspNetCore.Mvc;
using TenderAnalytics.Application.DTOs.Analytics;
using TenderAnalytics.Application.Interfaces.Services;

namespace TenderAnalytics.Api.Controllers;

/// <summary>
/// Provides analytical information based on imported procurement data.
/// </summary>
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

    /// <summary>
    /// Calculates the total expected procurement budget,
    /// the total value of signed contracts,
    /// and the resulting budget savings.
    /// </summary>
    /// <returns>Aggregated budget savings statistics.</returns>
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

    /// <summary>
    /// Returns the procuring organizations
    /// with the highest total value of active contracts.
    /// </summary>
    /// <param name="limit">
    /// Number of organizations to return (1-100).
    /// </param>
    /// <returns>Top procuring organizations.</returns>
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

    /// <summary>
    /// Returns suppliers ranked by the total value
    /// of active contracts.
    /// </summary>
    /// <param name="limit">
    /// Number of suppliers to return (1-100).
    /// </param>
    /// <returns>Top suppliers.</returns>
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