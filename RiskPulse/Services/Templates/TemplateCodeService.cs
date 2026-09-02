using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;

namespace RiskPulse.Services.Templates;

public class TemplateCodeService
{
    private readonly AppDbContext _db;

    public TemplateCodeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> GenerateSaqCodeAsync()
    {
        return await GenerateCodeAsync("SAQ");
    }

    public async Task<string> GenerateKriCodeAsync()
    {
        return await GenerateCodeAsync("KRI");
    }

    private async Task<string> GenerateCodeAsync(string prefix)
    {
        var datePart = DateTime.Now.ToString("yyyyMMdd");
        var prefixPattern = $"{prefix}-{datePart}-";

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var todayCodes = await _db.SaqHeaders
                .AsNoTracking()
                .Where(h => h.SaqCode != null && h.SaqCode.StartsWith(prefixPattern))
                .Select(h => h.SaqCode!)
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

            var exists = await _db.SaqHeaders
                .AsNoTracking()
                .AnyAsync(h => h.SaqCode == candidate);

            if (!exists)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"Unable to generate unique {prefix} code after multiple attempts.");
    }
}