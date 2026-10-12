using CustomsCloud.InfrastructureCore.Interfaces.Redis;
using CustomsCloud.InfrastructureCore.Lock;
using StackExchange.Redis;
using System.Globalization;

namespace CertificateOfOrigins.BL;

// TODO(platform): CustomsCloud.InfrastructureCore.Lock (1.10.51 and 1.10.53, the newest on the feed) takes the lock in
// Redis database 2 (RedisLockUtil.GetDatabase) but SafeReleaseAsync / ReleaseAsync look for it in database 0, find
// nothing and report it released — so a lock is never released and lives until its expiry. Until the package is fixed,
// the lock this request took is released here, in database 2, by the same key and value RedisLockUtil wrote. The call
// is atomic (Redis LockRelease = delete only when the value still matches), so a lock another request holds is never
// freed; once the package releases correctly, the key is already gone and this does nothing.
internal static class LockReleaseInLockDatabase
{
    private const int LockDatabase = 2;

    public static async Task<bool> Release(IRedisConnection redis, string resource, ILockState lockState)
    {
        if (!lockState.IsAcquired)
        {
            return false;
        }

        // RedisLockUtil: key "Infrastructure.LockUtil:{resource.Trim()}", value "{LockId}|{metadata}".
        var key = string.Format(CultureInfo.CurrentCulture, "Infrastructure.LockUtil:{0}", resource.Trim());
        var value = lockState.LockId + "|" + lockState.Metadata;
        var database = await redis.GetDatabase(LockDatabase);
        var result = await database.LockReleaseAsync(key, value, CommandFlags.DemandMaster);
        return result;
    }
}
