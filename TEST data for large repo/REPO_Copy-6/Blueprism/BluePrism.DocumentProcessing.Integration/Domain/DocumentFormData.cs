using System.Collections.Generic;

namespace BluePrism.DocumentProcessing.Integration.Domain
{
    public class DocumentFormDataResponse
    {
        public DocumentFormDocument Document { get; set; }
    }

    public class DocumentFormDocument
    {
        public List<DocumentFormDataField> Fields { get; set; }
        public List<DocumentFormDataTable> Tables { get; set; }
    }

    public class DocumentFormDataTable
    {
        public List<DocumentFormDataRow> Rows { get; set; }
        public string Name { get; set; }
    }

    public class DocumentFormDataRow
    {
        public List<DocumentFormDataField> Fields { get; set; }
    }

    public class DocumentFormDataField
    {
        public List<string> Confidence { get; set; }
        public string Validity { get; set; }
        public string RegionIndex { get; set; }
        public string PageId { get; set; }
        public string Name { get; set; }
        public string Reason { get; set; }
        public string OriginalValue { get; set; }
        public string Text { get; set; }
    }
}
