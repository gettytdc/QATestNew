namespace BluePrism.Api.CommonTestClasses
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Security;
    using AutomateProcessCore;
    using BpLibAdapters.Extensions;
    using Common.Security;
    using Domain;
    using FluentAssertions;
    using NUnit.Framework;

    public static class DataHelper
    {
        public static void ValidateCollectionsAreEqual(clsCollection bluePrismData, DataCollection domainData)
        {
            bluePrismData.Rows.Count.Should().Be(domainData.Rows.Count);
            foreach (var (bluePrismRow, domainRow) in bluePrismData.Rows.Zip(domainData.Rows, (x, y) => (x, y)))
            {
                ValidateRowsAreEqual(bluePrismRow, domainRow);
            }

        }

        public static void ValidateRowsAreEqual(clsCollectionRow bluePrismRow, IReadOnlyDictionary<string, DataValue> domainRow)
        {
            bluePrismRow.Count.Should().Be(domainRow.Count);
            bluePrismRow.Keys.Should().BeEquivalentTo(domainRow.Keys);
            foreach (var item in bluePrismRow)
            {
                ValidateValuesAreEqual(item.Value, domainRow[item.Key]);
            }
        }

        public static void ValidateValuesAreEqual(clsProcessValue bluePrismValue, DataValue domainValue)
        {
            switch (bluePrismValue.DataType)
            {
                case DataType.binary:
                {
                    Convert.ToBase64String((byte[])domainValue.Value ?? new byte[0]).Should().Be(bluePrismValue.EncodedValue);
                    break;
                }
                case DataType.collection:
                {
                    if (domainValue.Value is DataCollection d)
                        ValidateCollectionsAreEqual(bluePrismValue.Collection, d);
                    else
                        Assert.Fail();
                    break;
                }
                case DataType.date:
                case DataType.datetime:
                {
                    ((DateTimeOffset)domainValue.Value).Should().Be(((DateTime)bluePrismValue).ToDateTimeOffset());
                    break;
                }
                case DataType.flag:
                {
                    ((bool)domainValue.Value).Should().Be((bool)bluePrismValue);
                    break;
                }
                case DataType.image:
                {
                    BitmapsAreEqual((byte[])domainValue.Value, (Bitmap)bluePrismValue).Should().BeTrue();
                    break;
                }
                case DataType.number:
                {
                    (
                        (domainValue.Value is int i && i == (int)bluePrismValue)
                        || (domainValue.Value is decimal m && m == (decimal)bluePrismValue)
                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        || (domainValue.Value is double d && d == (double)bluePrismValue)
                    ).Should().BeTrue();
                    break;
                }
                case DataType.password:
                {
                    (domainValue.Value is SecureString s && s.AsString().Equals(((SafeString)bluePrismValue).AsString())).Should().BeTrue();
                    break;
                }
                case DataType.text:
                {
                    (
                        (domainValue.Value is string s && s.Equals((string)bluePrismValue))
                        || (domainValue.Value is Guid g && g.Equals((Guid)bluePrismValue))
                    ).Should().BeTrue();
                    break;
                }
                case DataType.time:
                {
                    ((DateTimeOffset)domainValue.Value).TimeOfDay.Should().Be(((DateTime)bluePrismValue).TimeOfDay);
                    ((DateTimeOffset)domainValue.Value).Year.Should().Be(1);
                    ((DateTimeOffset)domainValue.Value).Month.Should().Be(1);
                    ((DateTimeOffset)domainValue.Value).Day.Should().Be(1);
                    break;
                }
                case DataType.timespan:
                {
                    (domainValue.Value is TimeSpan t && t.Equals((TimeSpan)bluePrismValue)).Should().BeTrue();
                    break;
                }
                default:
                    Assert.Fail();
                    break;
            }
        }

        public static bool BitmapsAreEqual(byte[] left, Bitmap right)
        {
            if (left == null || right == null)
                return left == null && right == null;

            using (var rightStream = new MemoryStream())
            {
                right.Save(rightStream, ImageFormat.Png);

                var leftString = Convert.ToBase64String(left);
                var rightString = Convert.ToBase64String(rightStream.GetBuffer());

                return leftString.Equals(rightString);
            }
        }

        public static SecureString ToSecureString(string plainString)
        {
            if (string.IsNullOrEmpty(plainString))
            {
                return null;
            }
            var secureString = new SecureString();
            foreach (var c in plainString.ToCharArray())
            {
                secureString.AppendChar(c);
            }
            return secureString;
        }
    }
}
