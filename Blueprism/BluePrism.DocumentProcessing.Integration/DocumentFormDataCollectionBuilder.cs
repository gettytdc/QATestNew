namespace BluePrism.DocumentProcessing.Integration
{
    using AutomateProcessCore;
    using Domain;
    using Newtonsoft.Json;
    using BluePrism.DocumentProcessing.Api.Models;

    public class DocumentFormDataCollectionBuilder : IDocumentFormDataCollectionBuilder
    {
        private const string Fields = nameof(Fields);
        private const string Rows = nameof(Rows);
        private const string Tables = nameof(Tables);
        private const string Name = nameof(Name);
        private const string OriginalValue = nameof(OriginalValue);
        private const string Text = nameof(Text);
        private const string Value = nameof(Value);

        public clsCollection CreateCollection(string formDataAsString)
        {
            var formData = JsonConvert.DeserializeObject<DocumentFormDocument>(formDataAsString);

            // Top level
            var collection = new clsCollection();
            collection.AddField(Fields, DataType.collection);
            collection.AddField(Tables, DataType.collection);

            // Add top level
            var parentRow = new clsCollectionRow();
            parentRow.Add(Fields, new clsProcessValue(new clsCollection()));
            parentRow.Add(Tables, new clsProcessValue(new clsCollection()));

            // Fields definition
            var fieldCollection = collection.GetFieldDefinition(Fields).Children;
            fieldCollection.AddField(Name, DataType.text);
            fieldCollection.AddField(OriginalValue, DataType.text);
            fieldCollection.AddField(Text, DataType.text);

            // Tables definition
            var tableCollection = collection.GetFieldDefinition(Tables).Children;
            tableCollection.AddField(Rows, DataType.collection);

            tableCollection.GetField(Rows).Children.AddField(Fields, DataType.collection);

            tableCollection.GetField(Rows).Children.GetField(Fields).Children.AddField(Name, DataType.text);
            tableCollection.GetField(Rows).Children.GetField(Fields).Children.AddField(Value, DataType.text);

            BuildStandardFields(formData, parentRow);
            BuildTables(formData, parentRow);

            collection.Add(parentRow);

            return collection;
        }

        private static void BuildTables(DocumentFormDocument formData, clsCollectionRow parentRow)
        {
            formData.Tables.ForEach(table =>
            {
                BuildTable(parentRow, table);
            });
        }

        private static void BuildTable(clsCollectionRow parentRow, DocumentFormDataTable table)
        {
            var tableRow = new clsCollectionRow();
            tableRow.Add(Rows, new clsProcessValue(new clsCollection()));

            table.Rows.ForEach(row =>
            {
                BuildRow(row, tableRow);
            });

            parentRow[Tables].Collection.Add(tableRow);
        }

        private static void BuildRow(DocumentFormDataRow row, clsCollectionRow tableRow)
        {
            var rowRow = new clsCollectionRow();
            rowRow.Add(Fields, new clsProcessValue(new clsCollection()));

            row.Fields.ForEach(f =>
            {
                BuildField(f, rowRow);
            });

            tableRow[Rows].Collection.Add(rowRow);
        }

        private static void BuildField(DocumentFormDataField field, clsCollectionRow rowRow)
        {
            var fieldRow = new clsCollectionRow();
            fieldRow.Add(Name, new clsProcessValue(field.Name));
            fieldRow.Add(Value, new clsProcessValue(field.Text));

            rowRow[Fields].Collection.Add(fieldRow);
        }

        private static void BuildStandardFields(DocumentFormDocument formData, clsCollectionRow parentRow)
        {
            formData.Fields.ForEach(field =>
            {
                var collectionRow = new clsCollectionRow();
                collectionRow.Add(Name, new clsProcessValue(field.Name));
                collectionRow.Add(OriginalValue, new clsProcessValue(field.OriginalValue));
                collectionRow.Add(Text, new clsProcessValue(field.Text));

                parentRow[Fields].Collection.Add(collectionRow);
            });
        }
    }
}
