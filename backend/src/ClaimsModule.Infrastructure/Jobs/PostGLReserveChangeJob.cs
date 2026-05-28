using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace ClaimsModule.Infrastructure.Jobs;

public class PostGLReserveChangeJob(
    IReserveRepository reserveRepository,
    IAuditLogService auditLogService,
    IUnitOfWork unitOfWork,
    ILogger<PostGLReserveChangeJob> logger)
{
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(Guid reserveHistoryId, Guid claimId, string idempotencyKey)
    {
        logger.LogInformation("GL posting job started. IdempotencyKey: {Key}", idempotencyKey);

        var history = await reserveRepository.GetHistoryByIdAsync(reserveHistoryId);
        if (history is null)
        {
            logger.LogWarning("ReserveHistory {Id} not found — skipping GL posting", reserveHistoryId);
            return;
        }

        // Idempotency check — re-entrant safe
        if (history.PostingStatus == PostingStatus.Posted)
        {
            logger.LogInformation("GL posting already done for {Key} — skipping", idempotencyKey);
            return;
        }

        try
        {
            var jobId = Hangfire.JobStorage.Current?.ToString() ?? "unknown";

            history.PostingStatus = PostingStatus.Posted;
            history.PostingJobId = jobId;
            reserveRepository.UpdateHistory(history);
            await unitOfWork.SaveChangesAsync();

            var journalEntry = $"DR Change in Outstanding Reserves / CR Outstanding Loss Reserves, Amount = {history.Amount:C}";

            await auditLogService.LogAsync(
                claimId,
                AuditEventTypes.GlPostingSimulated,
                $"GL posting simulated: {journalEntry}",
                relatedEntityId: reserveHistoryId,
                relatedEntityType: "ReserveHistory",
                newValue: journalEntry);

            logger.LogInformation("GL posting completed for {Key}", idempotencyKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GL posting failed for {Key}", idempotencyKey);

            history.PostingStatus = PostingStatus.Failed;
            reserveRepository.UpdateHistory(history);
            await unitOfWork.SaveChangesAsync();

            await auditLogService.LogAsync(
                claimId,
                AuditEventTypes.GlPostingFailed,
                $"GL posting failed: {ex.Message}",
                relatedEntityId: reserveHistoryId,
                relatedEntityType: "ReserveHistory",
                newValue: ex.Message);

            throw;
        }
    }
}
