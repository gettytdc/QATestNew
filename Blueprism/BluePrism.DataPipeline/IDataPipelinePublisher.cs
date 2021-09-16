using System.Collections.Generic;
using BluePrism.Data;

namespace BluePrism.DataPipeline
{
    public interface IDataPipelinePublisher
    {
        /// <summary>
        /// Publishes supplied pipeline events to the db
        /// </summary>
        /// <param name="connection">The db connection to be used</param>
        /// <param name="pipelineEvent">The pipeline events to be published</param>
        void PublishToDataPipeline(IDatabaseConnection connection, IList<DataPipelineEvent> pipelineEvent);
    }
}
