using System;
using System.Windows.Forms;

using BluePrism.Core.Data;
using BluePrism.Server.Domain.Models;

namespace AutomateControlTester.Configurators
{
    public partial class DataGridViewConfigurator : BaseConfigurator
    {
        public DataGridViewConfigurator()
        {
            InitializeComponent();
            DataGridViewRowCollection rows = gridTest.Rows;
            rows.Add("calendar", ItemStatus.Queried,
                new BasicItemHeader()
                {
                    Title = "First",
                    SubTitle = "And the subtitle goes to",
                    ImageKey = "OpenBox"
                },
                0,
                DateTime.MinValue, DateTime.MaxValue,
                DateTime.Now, "calendar, Queried, minvalue, maxvalue, now");
            rows.Add("box", ItemStatus.Unknown,
                new BasicItemHeader()
                {
                    Title = "Second",
                    SubTitle = "And the subtitle goes to",
                    ImageKey = "Calendar"
                },
                50,
                new DateTime(1974, 12, 14, 4, 19, 52), DateTime.MaxValue,
                null, "box, Unknown, 1974-12-14 04:19:52, maxvalue, null");
            rows.Add(7, ItemStatus.Locked,
                new BasicItemHeader()
                {
                    Title = "Third",
                    SubTitle = "And the subtitle goes to",
                    ImageKey = "Class"
                },
                12.5m,
                new DateTime(2001, 1, 1), new DateTime(2020, 12, 19),
                DateTime.MinValue, "7, Locked, 2001-01-01, 2020-12-19, minvalue");
            rows.Add(7, ItemStatus.Locked,
                new BasicItemHeader()
                {
                    Title = "Fourth",
                    SubTitle = "And the subtitle goes to"
                },
                100,
                new DateTime(2001, 1, 1), new DateTime(2020, 12, 19),
                DateTime.MaxValue, "7, Locked, 2001-01-01, 2020-12-19, maxvalue");
            rows.Add(7, ItemStatus.Locked,
                new BasicItemHeader()
                {
                    Title = "The longest one of the group so far",
                    SubTitle = "And the subtitle goes to",
                    ImageKey = "OpenBox"
                },
                -1,
                new DateTime(2001, 1, 1), new DateTime(2020, 12, 19),
                DateTime.MinValue, "7, Locked, -1, 2001-01-01, 2020-12-19, minvalue");
            rows.Add(7, ItemStatus.Locked,
                new BasicItemHeader()
                {
                    Title = "First",
                    SubTitle = "",
                    ImageKey = "Project"
                },
                100,
                new DateTime(2001, 1, 1), new DateTime(2020, 12, 19),
                DateTime.MaxValue, "7, Locked, 2001-01-01, 2020-12-19, maxvalue");
            rows.Add(1, ItemStatus.Completed,
                new BasicItemHeader() { Title = "Last" },
                50,
                DateTime.MinValue, DateTime.MaxValue, DateTime.MinValue,
                "1, Completed, 50, minvalue, maxvalue, maxvalue");
        }

        public override string ConfigName
        {
            get { return "Data Grid View"; }
        }

    }
}
