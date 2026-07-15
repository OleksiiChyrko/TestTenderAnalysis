using Microsoft.AspNetCore.Mvc;
using TenderAnalytics.Application.DTOs.Import;
using TenderAnalytics.Application.Interfaces.Services;

namespace TenderAnalytics.Api.Controllers;

[ApiController]
[Route("api/import")]
public sealed class ImportController : ControllerBase
{
    private readonly ITenderImportService _importService;

    public ImportController(
        ITenderImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("tender/{id}")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportTender(
        string id,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new
            {
                message = "Tender id is required."
            });
        }

        var imported =
            await _importService.ImportTenderAsync(
                id,
                cancellationToken);

        if (!imported)
        {
            return BadRequest(new
            {
                id,
                imported = false,
                message =
                    "Tender does not match the import criteria."
            });
        }

        return Ok(new
        {
            id,
            imported = true
        });
    }

    [HttpPost("feed")]
    [ProducesResponseType(
        typeof(ImportResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportResult>>
        ImportFeed(
            [FromBody] ImportRequest request,
            CancellationToken cancellationToken)
    {
        if (request.DateFrom >= request.DateTo)
        {
            return BadRequest(new
            {
                message =
                    "DateFrom must be earlier than DateTo."
            });
        }

        if (request.MaxPages <= 0)
        {
            return BadRequest(new
            {
                message =
                    "MaxPages must be greater than zero."
            });
        }

        if (request.MaxConcurrency <= 0)
        {
            return BadRequest(new
            {
                message =
                    "MaxConcurrency must be greater than zero."
            });
        }

        var result =
            await _importService.ImportFeedAsync(
                request,
                cancellationToken);

        return Ok(result);
    }
}