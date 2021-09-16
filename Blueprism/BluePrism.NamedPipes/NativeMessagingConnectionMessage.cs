namespace BluePrism.NamedPipes
{
    public class NamedPipeConnectionMessage
    {
        public NamedPipeConnectionMessage(string bpClientId) => BpClientId = bpClientId;
        public string BpClientId { get; set; }
        public string Name => "Connect";
    }
}
