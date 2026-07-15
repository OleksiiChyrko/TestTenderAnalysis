namespace TenderAnalytics.Application.DTOs.Import;

public sealed class ImportResult
{
    public int PagesProcessed { get; set; }

    public int FeedItemsProcessed { get; set; }

    public int CandidatesCount { get; set; }

    public int ImportedCount { get; set; }

    public int SkippedCount { get; set; }

    public int FailedCount { get; set; }

    public List<string> Errors { get; set; } = [];
}