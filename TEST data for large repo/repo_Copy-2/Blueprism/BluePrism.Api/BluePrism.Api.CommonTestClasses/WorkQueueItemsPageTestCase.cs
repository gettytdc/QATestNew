namespace BluePrism.Api.CommonTestClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;

    public class WorkQueueItemsPageTestCase
    {
        public Func<WorkQueueItemNoDataXml, object> SelectPropertyFunc { get; set; }

        public Func<IEnumerable<WorkQueueItemNoDataXml>, IOrderedEnumerable<WorkQueueItemNoDataXml>> OrderByFunc { get; set; }

        public WorkQueueItemSortByProperty WorkQueueItemSortByProperty { get; set; }

        public void Deconstruct(
            out Func<WorkQueueItemNoDataXml, object> selectPropertyFunc,
            out Func<IEnumerable<WorkQueueItemNoDataXml>, IOrderedEnumerable<WorkQueueItemNoDataXml>> orderByFunc,
            out WorkQueueItemSortByProperty workQueueItemSortByProperty)
        {
            selectPropertyFunc = SelectPropertyFunc;
            orderByFunc = OrderByFunc;
            workQueueItemSortByProperty = WorkQueueItemSortByProperty;
        }
    }
}
