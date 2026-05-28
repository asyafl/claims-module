namespace ClaimsModule.Application.Interfaces;

public interface IHangfireEnqueuer
{
    string Enqueue<T>(System.Linq.Expressions.Expression<Func<T, Task>> methodCall);
}
