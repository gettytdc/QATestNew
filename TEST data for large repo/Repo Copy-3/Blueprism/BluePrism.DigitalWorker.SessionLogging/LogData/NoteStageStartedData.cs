using ProcessStages = BluePrism.DigitalWorker.Messages.Events.ProcessStages;

namespace BluePrism.DigitalWorker.SessionLogging.LogData
{
    public class NoteStageStartedData : ProcessStages.LogData.NoteStageStartedData
    {
        public NoteStageStartedData(string narrative) 
        {
            Narrative = narrative;
        }

        public string Narrative { get; }
    }
}
