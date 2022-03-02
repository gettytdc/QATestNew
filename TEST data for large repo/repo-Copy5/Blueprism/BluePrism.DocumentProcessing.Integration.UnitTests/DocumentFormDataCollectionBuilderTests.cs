namespace BluePrism.DocumentProcessing.Integration.UnitTests
{
    using Domain;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using System.Collections.Generic;
    using BluePrism.Utilities.Testing;

    [TestFixture]
    public class DocumentFormDataCollectionBuilderTests : UnitTestBase<DocumentApi>
    {
        [Test]
        public void CreateCollection_GivenARequest_WhenCalled_ThenMatchingCollectionIsReturned()
        {
            var expectedResponse =
                "<collection><row><field name=\"Fields\" type=\"collection\"><row><field name=\"Name\" type=\"text\" value=\"name1\" /><field name=\"OriginalValue\" type=\"text\" value=\"ov1\" /><field name=\"Text\" type=\"text\" value=\"text1\" /></row><row><field name=\"Name\" type=\"text\" value=\"name2\" /><field name=\"OriginalValue\" type=\"text\" value=\"ov2\" /><field name=\"Text\" type=\"text\" value=\"text2\" /></row></field><field name=\"Tables\" type=\"collection\"><row><field name=\"Rows\" type=\"collection\"><row><field name=\"Fields\" type=\"collection\"><row><field name=\"Name\" type=\"text\" value=\"name1\" /><field name=\"Value\" type=\"text\" value=\"text1\" /></row><row><field name=\"Name\" type=\"text\" value=\"name2\" /><field name=\"Value\" type=\"text\" value=\"text2\" /></row></field></row><row><field name=\"Fields\" type=\"collection\"><row><field name=\"Name\" type=\"text\" value=\"name1\" /><field name=\"Value\" type=\"text\" value=\"text1\" /></row><row><field name=\"Name\" type=\"text\" value=\"name2\" /><field name=\"Value\" type=\"text\" value=\"text2\" /></row></field></row></field></row><row><field name=\"Rows\" type=\"collection\"><row><field name=\"Fields\" type=\"collection\"><row><field name=\"Name\" type=\"text\" value=\"name1\" /><field name=\"Value\" type=\"text\" value=\"text1\" /></row><row><field name=\"Name\" type=\"text\" value=\"name2\" /><field name=\"Value\" type=\"text\" value=\"text2\" /></row></field></row><row><field name=\"Fields\" type=\"collection\"><row><field name=\"Name\" type=\"text\" value=\"name1\" /><field name=\"Value\" type=\"text\" value=\"text1\" /></row><row><field name=\"Name\" type=\"text\" value=\"name2\" /><field name=\"Value\" type=\"text\" value=\"text2\" /></row></field></row></field></row></field></row></collection>";
            var classUnderTest = new DocumentFormDataCollectionBuilder();
            var request = CreateDocumentFormDataRequest();

            classUnderTest.CreateCollection(JsonConvert.SerializeObject(request)).GenerateXML().Should().Be(expectedResponse);
        }

        private static DocumentFormDocument CreateDocumentFormDataRequest()
        {
            return new DocumentFormDocument()
            {

                Fields = new List<DocumentFormDataField>
                    {
                        new DocumentFormDataField
                        {
                            Name = "name1", OriginalValue = "ov1",
                            Text = "text1"
                        },
                        new DocumentFormDataField
                        {
                            Name = "name2", OriginalValue = "ov2",
                            Text = "text2"
                        }

                    },


                Tables = new List<DocumentFormDataTable>
                    {
                        CreateTable(),
                        CreateTable()
                    }

            };
        }

        private static DocumentFormDataTable CreateTable()
        {
            return new DocumentFormDataTable
            {
                Rows = new List<DocumentFormDataRow>
                {
                    CreateRow(),
                    CreateRow()
                }
            };
        }

        private static DocumentFormDataRow CreateRow()
        {
            return new DocumentFormDataRow
            {
                Fields = new List<DocumentFormDataField>
                {
                    new DocumentFormDataField
                    {
                        Name = "name1", OriginalValue = "ov1",
                        Text = "text1"
                    },
                    new DocumentFormDataField
                    {
                        Name = "name2", OriginalValue = "ov2",
                        Text = "text2"
                    }
                }
            };
        }
    }
}