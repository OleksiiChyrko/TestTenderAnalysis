using TenderAnalytics.Infrastructure;
using TenderAnalytics.Application.Interfaces.External;
using TenderAnalytics.Application;
using TenderAnalytics.Application.Interfaces.Mapping;
using TenderAnalytics.Application.Interfaces.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet(
    "/api/debug/tenders/{id}",
    async (
        string id,
        ITenderApiClient client,
        CancellationToken cancellationToken) =>
    {
        var tender = await client.GetTenderAsync(
            id,
            cancellationToken);

        return Results.Ok(tender);
    });

app.MapGet(
    "/api/debug/feed",
    async (
        ITenderApiClient client,
        CancellationToken cancellationToken) =>
    {
        var page = await client.GetFeedPageAsync(
            cancellationToken: cancellationToken);

        return Results.Ok(page);
    });

app.MapGet(
    "/api/debug/tenders/{id}/mapped",
    async (
        string id,
        ITenderApiClient client,
        ITenderMapper mapper,
        CancellationToken cancellationToken) =>
    {
        var response = await client.GetTenderAsync(
            id,
            cancellationToken);

        if (response.Data is null)
        {
            return Results.NotFound();
        }

        var tender = mapper.Map(response.Data);

        return Results.Ok(new
        {
            tender.Id,
            tender.Status,
            tender.DateCreated,
            tender.ExpectedAmount,
            tender.Currency,
            tender.CpvCode,
            tender.ProcuringEntityIdentifier,
            tender.ProcuringEntityName,
            tender.ImportedAt,

            Contracts = tender.Contracts.Select(contract => new
            {
                contract.Id,
                contract.AwardId,
                contract.Status,
                contract.Amount,
                contract.Currency,

                Suppliers = contract.ContractSuppliers.Select(link => new
                {
                    link.Supplier.Identifier,
                    link.Supplier.Name,
                    link.Supplier.NormalizedName
                })
            })
        });
    });

app.MapPost(
    "/api/debug/tenders/{id}/import",
    async (
        string id,
        ITenderImportService importService,
        CancellationToken cancellationToken) =>
    {
        var imported =
            await importService.ImportTenderAsync(
                id,
                cancellationToken);

        return imported
            ? Results.Ok(new
            {
                id,
                imported = true
            })
            : Results.BadRequest(new
            {
                id,
                imported = false,
                reason =
                    "Tender does not match the import criteria."
            });
    });

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
