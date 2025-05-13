using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Cheapp.Services;

public class EbayTokenWarmup : IHostedService
{
    private readonly IEbayOAuthService _svc;
    public EbayTokenWarmup(IEbayOAuthService svc) => _svc = svc;

    public Task StartAsync(CancellationToken ct) => _svc.GetTokenAsync(ct);
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
