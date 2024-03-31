using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuroraRgb.Settings;

public interface IInitializableProfile : IDisposable
{
    bool Initialized { get; }

    Task<bool> Initialize(CancellationToken cancellationToken);
}