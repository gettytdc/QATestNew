using System;
using System.Collections.Generic;

namespace BluePrism.NativeMessaging.Browser
{
    public class Tab
    {
        public Tab(int id, string title)
        {
            Id = id;
            Title = title;
            Pages = new List<Guid>();
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public List<Guid> Pages { get; set; }
    }
}
