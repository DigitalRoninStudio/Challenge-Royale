using Unity.Networking.Transport;

public struct NetworkPipelineService
{
    public NetworkPipeline Unreliable { get; private set; }
    public NetworkPipeline Reliable { get; private set; }
    public NetworkPipeline Fragmentation { get; private set; }

    public NetworkPipelineService(NetworkDriver driver)
    {
        Unreliable = driver.CreatePipeline(typeof(UnreliableSequencedPipelineStage));
        Reliable = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        Fragmentation = driver.CreatePipeline(typeof(FragmentationPipelineStage), typeof(ReliableSequencedPipelineStage));
    }
}

