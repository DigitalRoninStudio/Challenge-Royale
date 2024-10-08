using System;
using Unity.Networking.Transport;

public interface INetworkService : IDisposable
{
    void Update();
    void Dispose();
    NetworkPipeline GetPipeline(Pipeline pipeline);
    NetworkDriver GetDriver();
}