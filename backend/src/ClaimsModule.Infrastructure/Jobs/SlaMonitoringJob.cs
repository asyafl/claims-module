using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClaimsModule.Infrastructure.Jobs;

public class SlaMonitoringJob(
    IClaimRepository claimRepository,
    IReserveRepository reserveRepository,
    IAuditLogService auditLogService,
    IConfiguration configuration,
    ILogger<SlaMonitoringJob> logger)
{
    private static readonly Guid OrgId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const int SlaHours = 48;
    private const int DedupeHours = 24;

    public async Task ExecuteAsync()
    {
        logger.LogInformation("SLA monitoring job started");

        var threshold = DateTimeOffset.UtcNow.AddHours(-SlaHours);
        var staleClaims = await claimRepository.GetStaleDraftOrOpenClaimsAsync(threshold, OrgId);

        foreach (var claim in staleClaims)
        {
            try
            {
                // Deduplication: skip if breached within last 24h
                var lastBreach = await reserveRepository.GetLastSlaBreachDateAsync(claim.Id);
                if (lastBreach.HasValue && lastBreach.Value > DateTimeOffset.UtcNow.AddHours(-DedupeHours))
                {
                    logger.LogDebug("Skipping SLA breach for claim {Id} — already logged within 24h", claim.Id);
                    continue;
                }

                await auditLogService.LogAsync(
                    claim.Id,
                    AuditEventTypes.SlaBreachDetected,
                    "Claim has not been updated in 48 hours");

                logger.LogInformation("SLA breach detected for claim {ClaimNumber}", claim.ClaimNumber);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing SLA breach for claim {Id}", claim.Id);
            }
        }

        logger.LogInformation("SLA monitoring job completed. Processed {Count} claims", staleClaims.Count());
    }
}
