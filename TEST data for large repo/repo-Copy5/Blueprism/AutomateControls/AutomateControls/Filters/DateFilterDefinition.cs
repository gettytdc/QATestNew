using AutomateControls.Properties;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.ObjectModel;
using BluePrism.Server.Domain.Models;
using BluePrism.BPCoreLib;

namespace AutomateControls.Filters
{
    public enum DateUnit
    {
        None = 0,
        Millisecond,
        Second,
        Minute,
        Hour,
        Day,
        Week,
        Month,
        Year
    }

    /// <summary>
    /// Class to represent a base date filter definition.
    /// </summary>
    public abstract class DateFilterDefinition : BaseFilterDefinition
    {
        public DateFilterDefinition(string name) : base(name, true) { }

        /// <summary>
        /// Parses the given filter text into a filter item.
        /// </summary>
        /// <param name="filterText">The text to parse</param>
        /// <returns>The filter item derived from the given text.</returns>
        public override FilterItem Parse(string filterText)
        {
            String txt = filterText.ToLower().Trim();
            if (FilterItem.IsNull(txt))
                return new FilterItem();

            // "last n days" / "next n days"
            {
                Regex rxNDays = new Regex(@"((?:last)|(?:next)) (\d+) day(s)?");
                Match m = rxNDays.Match(txt);
                if (m.Success)
                {
                    bool isLast = "last".Equals(m.Groups[1].ToString());
                    int days = int.Parse(m.Groups[2].ToString());
                    // if 'last n days' then range from 'today - (n-1) days' to tonight at midnight
                    // if 'next n days' then range from 'today' to 'today + n days'
                    clsDateRange dr;
                    if (isLast)
                    {
                        dr = new clsDateRange(DateTime.Today.AddDays(-(days - 1)), DateTime.Today.AddDays(1));
                    }
                    else
                    {
                        dr = new clsDateRange(DateTime.Today, DateTime.Today.AddDays(days));
                    }
                    return new FilterItem(filterText, dr);
                }
            }

            // "> n days ago", ">1 month ago, <2 weeks ago", ">1h ago"
            {
                Regex rxAgo = new Regex(@"\s*?((?:<|>)?=?)\s*?(\d+)\s*?(\w*) ago\s*", RegexOptions.None, RegexTimeout.DefaultRegexTimeout);
                Match m = rxAgo.Match(txt);

                DateTime start = DateTime.MinValue;
                DateTime end = DateTime.MaxValue;

                if (m.Success)
                {
                    do
                    {
                        int num = int.Parse(m.Groups[2].Value);
                        DateTime dt = DateTime.Now;
                        switch (m.Groups[3].Value)
                        {
                            case "milliseconds":
                            case "millisecond":
                            case "millis":
                            case "ms":
                                dt = dt.AddMilliseconds(-num);
                                break;

                            case "seconds":
                            case "second":
                            case "secs":
                            case "sec":
                            case "s":
                                dt = dt.AddSeconds(-num);
                                break;

                            case "minutes":
                            case "minute":
                            case "mins":
                            case "min":
                                dt = dt.AddMinutes(-num);
                                break;

                            case "hours":
                            case "hour":
                            case "h":
                                dt = dt.AddHours(-num);
                                break;

                            case "days":
                            case "day":
                            case "d":
                                dt = dt.AddDays(-num);
                                break;

                            case "weeks":
                            case "week":
                            case "w":
                                dt = dt.AddDays(-num * 7);
                                break;

                            case "months":
                            case "month":
                            case "m":
                                dt = dt.AddMonths(-num);
                                break;

                            case "years":
                            case "year":
                            case "y":
                                dt = dt.AddYears(-num);
                                break;

                            default:
                                throw new InvalidFormatException(
                                    Resources.DateFilterDefinition_UnrecognisedUnit0, m.Groups[3].Value);
                        }
                        // If we're going back to before today, filter out the time
                        // component of the datetime
                        if (dt < DateTime.Today)
                            dt = dt.Date;

                        string op = m.Groups[1].Value;
                        switch (op)
                        {
                            case ">":
                                // equivalent to ">= [date+1]"
                                dt = dt.AddDays(1);
                                goto case ">=";

                            case ">=":
                                start = (dt > start ? dt : start);
                                break;

                            case "<=":
                                // equivalent to "< [date+1]"
                                dt = dt.AddDays(1);
                                goto case "<";

                            case "<":
                                // equivalent to "<= [date-1]"
                                end = (dt < end ? dt : end);
                                break;

                            case "=":
                            default: // that date exactly.
                                start = dt;
                                end = start.AddDays(1);
                                break;
                        }

                        m = m.NextMatch();
                    } while (m.Success);

                    return new FilterItem(filterText, new clsDateRange(start, end));

                }
            }

            // "today", "yesterday and today", "today & tomorrow", etc.
            switch (txt)
            {
                case "today":
                    return new FilterItem(filterText,
                        new clsDateRange(0, 1));

                case "tomorrow":
                    return new FilterItem(filterText,
                        new clsDateRange(1, 2));

                case "yesterday":
                    return new FilterItem(filterText,
                        new clsDateRange(-1, 0));

                case "today and tomorrow":
                case "today & tomorrow":
                case "today + tomorrow":
                    return new FilterItem(filterText,
                        new clsDateRange(0, 2));

                case "today and yesterday":
                case "yesterday and today":
                case "today & yesterday":
                case "yesterday & today":
                case "today + yesterday":
                case "yesterday + today":
                    return new FilterItem(filterText,
                        new clsDateRange(-1, 1));
            }

            // Finally, a check for absolute date(s)
            // "> 14/12/1974", ">01/01/2012, <= 31/12/2012" etc.
            {
                DateTime start = DateTime.MinValue;
                DateTime end = DateTime.MaxValue;
                Regex rxDate = new Regex(@"\s*?((?:<|>)?=?)\s*?(\d+[-/. ]\d+[-/. ]\d+)\s*", RegexOptions.None, RegexTimeout.DefaultRegexTimeout);
                Match m = rxDate.Match(txt);
                while (m.Success)
                {
                    if (m.Captures.Count > 0)
                    {
                        string op = m.Groups[1].Value;
                        DateTime parsedDate;
                        if (DateTime.TryParse(m.Groups[2].Value, null, DateTimeStyles.AssumeLocal, out parsedDate))
                        {
                            parsedDate = parsedDate.Date;

                            switch (op)
                            {
                                case ">":
                                    // equivalent to ">= [date+1]"
                                    parsedDate = parsedDate.AddDays(1);
                                    goto case ">=";

                                case ">=":
                                    start = (parsedDate > start ? parsedDate : start);
                                    break;

                                case "<=":
                                    // equivalent to "< [date+1]"
                                    parsedDate = parsedDate.AddDays(1);
                                    goto case "<";

                                case "<":
                                    // equivalent to "<= [date-1]"
                                    end = (parsedDate < end ? parsedDate : end);
                                    break;

                                case "=":
                                default: // that date exactly.
                                    start = parsedDate;
                                    end = start.AddDays(1);
                                    break;
                            }
                        }
                        else
                        {
                            // what? Throw an exception?
                            throw new InvalidFormatException(
                                Resources.DateFilterDefinition_InvalidFilterTerm0, filterText);
                        }
                    }
                    m = m.NextMatch();
                }
                // if we're still at "MinValue - MaxValue", treat as Null
                if (start == DateTime.MinValue && end == DateTime.MaxValue)
                {
                    if (FilterItem.IsNull(txt))
                    {
                        return new FilterItem();
                    }
                    throw new InvalidFormatException(Resources.DateFilterDefinition_InvalidDate0, filterText);
                }
                // otherwise, return our range.
                return new FilterItem(filterText, new clsDateRange(start, end));
            }
        }
    }

    /// <summary>
    /// Filter definition which represents a past date.
    /// </summary>
    public class PastDateFilterDefinition : DateFilterDefinition
    {
        public static class Terms
        {
            public const string Today = "Today";
            public const string Last3Days = "Last 3 Days";
            public const string Last7Days = "Last 7 Days";
            public const string Last31Days = "Last 31 Days";
        }

        /// <summary>
        /// The default collection of items to display for a past date filter
        /// </summary>
        private ICollection<FilterItem> _items = new Collection<FilterItem>(
            new FilterItem[] {
                new FilterItem(),
                new FilterItem(Terms.Today, new clsDateRange(0, 1), Resources.PastDateFilterDefinition_Today),
                new FilterItem(Terms.Last3Days, new clsDateRange(-2, 1), Resources.PastDateFilterDefinition_Last3Days), 
                new FilterItem(Terms.Last7Days, new clsDateRange(-6, 1), Resources.PastDateFilterDefinition_Last7Days),
                new FilterItem(Terms.Last31Days, new clsDateRange(-30, 1), Resources.PastDateFilterDefinition_Last31Days)
            });

        /// <summary>
        /// Creates a new filter definition representing past dates with
        /// the given name.
        /// </summary>
        /// <param name="name">The name of the filter that this definition
        /// describes.</param>
        public PastDateFilterDefinition(string name) : base(name) { }

        /// <summary>
        /// The items to display for this filter
        /// </summary>
        public override ICollection<FilterItem> Items
        {
            get { return _items; }
        }
    }

    // public class PotentialDateFilterDefinition : PastDateFilterDefinition { }

    /// <summary>
    /// Represents a filter definition for dates in the future.
    /// </summary>
    public class FutureDateFilterDefinition : DateFilterDefinition
    {
        /// <summary>
        /// The default collection of items to display for this definition.
        /// </summary>
        private static ICollection<FilterItem> _items = new Collection<FilterItem>(
            new FilterItem[] {
                new FilterItem(),
                new FilterItem("Today", new clsDateRange(0, 1), Resources.FutureDateFilterDefinition_Today),
                new FilterItem("Next 3 Days", new clsDateRange(0,3), Resources.FutureDateFilterDefinition_Next3Days), 
                new FilterItem("Next 7 Days", new clsDateRange(0,7), Resources.FutureDateFilterDefinition_Next7Days),
                new FilterItem("Next 14 Days", new clsDateRange(0,14), Resources.FutureDateFilterDefinition_Next14Days),
                new FilterItem("Next 21 Days", new clsDateRange(0,21), Resources.FutureDateFilterDefinition_Next21Days),
                new FilterItem("Next 31 Days", new clsDateRange(0,31), Resources.FutureDateFilterDefinition_Next31Days)
        });

        /// <summary>
        /// Creates a new filter definition for future dates known by the
        /// given name.
        /// </summary>
        /// <param name="name">The name of the filter that this definition
        /// describes.</param>
        public FutureDateFilterDefinition(string name) : base(name) { }

        /// <summary>
        /// The items to display for filters defined by this object.
        /// </summary>
        public override ICollection<FilterItem> Items
        {
            get { return _items; }
        }
    }
}
