using System.Collections.Generic;

namespace AuroraRgb.Modules.Razer.RazerApi;

public class RazerEndpoints(List<RazerEndpoint> endpoints)
{
    public List<RazerEndpoint> Endpoints => endpoints;
}