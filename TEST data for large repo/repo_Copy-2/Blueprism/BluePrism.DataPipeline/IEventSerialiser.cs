namespace BluePrism.DataPipeline
{
    /// <summary>
    /// Serialises a data pipeline event to a string to be published to the data pipeline.
    /// </summary>
    public interface IEventSerialiser
    {
        string SerialiseEvent(DataPipelineEvent dataPipelineEvent);


    }
}
