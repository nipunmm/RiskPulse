using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;

namespace RiskPulse.Services.Utilities;

public class CodeGeneratorService
{
    private readonly AppDbContext _db;

    public CodeGeneratorService(AppDbContext db)
    {
        _db = db;
    }

    public Task<string> GenerateSaqCodeAsync()
    {
        return GenerateCodeAsync("SAQ", _db.SaqHeaders
            .Where(h => h.SaqCode != null)
            .Select(h => h.SaqCode!));
    }

    public Task<string> GenerateKriCodeAsync()
    {
        return GenerateCodeAsync("KRI", _db.KriHeaders
            .Where(h => h.KriCode != null)
            .Select(h => h.KriCode!));
    }

    public Task<string> GenerateScheduleCodeAsync()
    {
        return GenerateCodeAsync("SCH", _db.Schedules
            .Select(s => s.ScheduleCode));
    }

    public Task<string> GenerateAssessmentCodeAsync()
    {
        return GenerateCodeAsync("ASM", _db.AssessmentHeaders
            .Select(h => h.AssessmentCode));
    }

    private async Task<string> GenerateCodeAsync(string prefix, IQueryable<string> allCodes)
    {
        var datePart = DateTime.Now.ToString("yyyyMMdd");
        var prefixPattern = $"{prefix}-{datePart}-";

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var todayCodes = await allCodes
                .Where(c => c.StartsWith(prefixPattern))
                .ToListAsync();

            var maxSeq = todayCodes
                .Select(c =>
                {
                    var suffix = c[prefixPattern.Length..];
                    return int.TryParse(suffix, out var n) ? n : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            var candidate = $"{prefixPattern}{(maxSeq + 1):0000}";

            var exists = await allCodes
                .AnyAsync(c => c == candidate);

            if (!exists)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"Unable to generate unique {prefix} code after multiple attempts.");
    }
}