using Unity.Networking.Transport;

public interface INetworkService
{
    void Update();
    void Dispose();
    NetworkPipeline GetPipeline(Pipeline pipeline);
    NetworkDriver GetDriver();
}