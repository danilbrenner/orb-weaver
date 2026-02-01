using Microsoft.EntityFrameworkCore;
using OrbWeaver.Domain;
using OrbWeaver.Handler.Abstractions;

namespace OrbWeaver.Data.Repositories;

public class MessageLogRepository(IDbContextFactory<OrbWeaverDbContext> contextFactory) : IMessageLogRepository
{
    public async Task<int> Log(Message message, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Database
            .ExecuteSqlInterpolatedAsync
            ($"""
              insert into messages_log(hash, payload) 
              values ({message.Hash}, {message.Payload})
              on conflict (hash) do nothing;
            """, cancellationToken: cancellationToken);
    }
}