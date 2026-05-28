using ClaimsModule.Application.Interfaces;
using Hangfire;
using System.Linq.Expressions;

namespace ClaimsModule.Infrastructure.Services;

public class HangfireEnqueuer : IHangfireEnqueuer
{
    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
        => BackgroundJob.Enqueue(methodCall);
}
