
using StackExchange.Redis;

namespace Shared.Services;

public interface IRedisService
{
    Task SetValue(string key, string? value, TimeSpan? expiry = null);
    Task<string?> GetValue(string key);
    Task<bool> DeleteValue(string key);
}

public class RedisService(IConnectionMultiplexer redis): IRedisService
{
    public async Task SetValue(string key, string? value, TimeSpan? expiry = null)
    {
        var db = redis.GetDatabase();
        await db.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetValue(string key)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(key);
        return value;
    }
    public async Task<bool> DeleteValue(string key)
    {
        var db = redis.GetDatabase();
        return await db.KeyDeleteAsync(key);
    }

}
