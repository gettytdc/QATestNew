using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using NUnit.Framework;

namespace AutomateUI.UnitTests.Classes
{
    public class NullDataGridViewImageCellTests
    {
        [Test]
        public void NullDataGridViewImageCell_DefaultNewRowValueGetCalled_ShouldReturnNull()
        {
            var nullDataGridViewImageCell = new NullDataGridViewImageCell();
            var result = nullDataGridViewImageCell.DefaultNewRowValue;
            result.Should().BeNull();
        }

        
        [Test]
        public void NullDataGridViewImageCell_AdjustCellBorderStyle_ShouldBePopulatedCorrectly()
        {
            var dataGridViewAdvancedBorderStyleInput = new DataGridViewAdvancedBorderStyle();
            var dataGridViewAdvancedBorderStylePlaceholder = new DataGridViewAdvancedBorderStyle();

            var nullDataGridViewImageCell = new NullDataGridViewImageCell();
            var result = nullDataGridViewImageCell.AdjustCellBorderStyle(dataGridViewAdvancedBorderStyleInput,
                                                                         dataGridViewAdvancedBorderStylePlaceholder,
                                                                         false,
                                                                         false,
                                                                         false,
                                                                         false);


            result.Right.Should().Be(DataGridViewAdvancedCellBorderStyle.None);
            result.Bottom.Should().Be(DataGridViewAdvancedCellBorderStyle.Single);
            dataGridViewAdvancedBorderStyleInput.Top.Should().Be(DataGridViewAdvancedCellBorderStyle.Single);
            dataGridViewAdvancedBorderStyleInput.Left.Should().Be(DataGridViewAdvancedCellBorderStyle.Single);
        }
    }
}
