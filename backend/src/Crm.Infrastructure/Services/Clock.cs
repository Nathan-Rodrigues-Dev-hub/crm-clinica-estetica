using Crm.Application.Common.Interfaces;

namespace Crm.Infrastructure.Services;

public class Clock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
