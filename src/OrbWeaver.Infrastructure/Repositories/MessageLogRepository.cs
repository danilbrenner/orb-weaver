using Microsoft.EntityFrameworkCore;
using OrbWeaver.Application.Abstractions;
using OrbWeaver.Domain;

namespace OrbWeaver.Infrastructure.Repositories;

public class MessageLogRepository(IDbContextFactory<OrbWeaverDbContext> contextFactory) : IMessageLogRepository
{
    public async Task<int> Log(Message message, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Database
            .ExecuteSqlInterpolatedAsync
            ($"""
              insert into messages_log(hash, payload, timestamp) 
              values ({message.Hash}, {message.Payload}, {message.Timestamp})
              on conflict (hash) do nothing;
            """, cancellationToken: cancellationToken);
    }
}