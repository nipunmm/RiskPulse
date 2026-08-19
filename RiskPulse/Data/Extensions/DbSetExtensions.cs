using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RiskPulse.Models.ViewModel;

namespace RiskPulse.Data.Extensions;

public static class DbSetExtensions
{
    public static async Task EnsureUniqueAsync<T>(
        this DbSet<T> set,
        Expression<Func<T, bool>> match,
        string fieldName,
        string value) where T : class
    {
        var exists = await set.AnyAsync(match);
        if (exists)
            throw new InvalidOperationException($"{fieldName} '{value}' already exists.");
    }

    public static async Task<List<OptionViewModel>> ToOptionListAsync<T>(
        this IQueryable<T> query,
        Func<T, int> valueSelector,
        Func<T, string> labelSelector)
    {
        return await query
            .Select(x => new OptionViewModel
            {
                Value = valueSelector(x),
                Label = labelSelector(x)
            })
            .ToListAsync();
    }
}
