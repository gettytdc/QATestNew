using BluePrism.Scheduling.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;
using System.Linq;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Simple way to hold a set of days
    /// </summary>
    [Serializable, DataContract(Namespace = "bp"), KnownType(typeof(ReadOnlyDaySet))]
    public class DaySet : IEnumerable<DayOfWeek>, IEnumerable, ICollection<DayOfWeek>
    {
        #region ReadOnlyDaySet

        /// <summary>
        /// Read-only extension of a dayset which throws a NotSupportedException
        /// if any of the mutator methods / properties are called.
        /// </summary>
        private class ReadOnlyDaySet : DaySet
        {
            private static int combine(params DayOfWeek[] days)
            {
                int week = 0;
                foreach (DayOfWeek day in days)
                    week |= (1 << (int)day);
                return week;
            }

            internal ReadOnlyDaySet(params DayOfWeek[] days) : base(combine(days)) { }

            public override void Add(DayOfWeek item)
            {
                throw new NotSupportedException();
            }

            public override void Clear()
            {
                throw new NotSupportedException();
            }

            public override bool IsReadOnly
            {
                get { return true; }
            }

            public override bool Remove(DayOfWeek item)
            {
                throw new NotSupportedException();
            }

            public override void Set(DayOfWeek day)
            {
                throw new NotSupportedException();
            }

            public override void SetTo(int flags)
            {
                throw new NotSupportedException();
            }

            public override bool this[DayOfWeek day]
            {
                get { return base[day]; }
                set { throw new NotSupportedException(); }
            }

            public override void SetTo(DaySet daySet)
            {
                throw new NotSupportedException();
            }

            public override void Unset(DayOfWeek day)
            {
                throw new NotSupportedException();
            }

        }

        #endregion

        /// <summary>
        /// Read only DaySet containing Monday - Friday inclusive
        /// </summary>
        public static readonly DaySet FiveDayWeek = new ReadOnlyDaySet(
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday);

        /// <summary>
        /// Read only DaySet containing all days of the week.
        /// </summary>
        public static readonly DaySet FullWeek = new ReadOnlyDaySet(
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday);

        /// <summary>
        /// Gets the first day of the week as far as this system is concerned.
        /// </summary>
        public static DayOfWeek FirstDayOfWeek
        {
            get { return DayOfWeek.Monday; }
        }

        /// <summary>
        /// Bitmask containing all days set on.
        /// </summary>
        public const int ALL_DAYS =
            (1 << ((int)DayOfWeek.Monday)) |
            (1 << ((int)DayOfWeek.Tuesday)) |
            (1 << ((int)DayOfWeek.Wednesday)) |
            (1 << ((int)DayOfWeek.Thursday)) |
            (1 << ((int)DayOfWeek.Friday)) |
            (1 << ((int)DayOfWeek.Saturday)) |
            (1 << ((int)DayOfWeek.Sunday));

        /// <summary>
        /// The bitset which contains the week on which days can be set
        /// // on or off
        /// </summary>
        [DataMember]      
        private int _week;

        #region Constructors

        /// <summary>
        /// Creates a new day set using the days specified in the given value.
        /// </summary>
        /// <param name="value">The int containing the bit flags indicating
        /// which days are to be set on in the resultant day set. Any bits
        /// other than those representing days are discarded when the set
        /// is created.</param>
        public DaySet(int value)
        {
            _week = (value & ALL_DAYS); // mask out any unhandled bits
        }

        /// <summary>
        /// Creates a new day set using the provided days.
        /// </summary>
        /// <param name="days">The days which should be set in the resultant
        /// DaySet.</param>
        public DaySet(params DayOfWeek[] days)
        {
            _week = 0;
            foreach (DayOfWeek day in days)
                Set(day);
        }

        /// <summary>
        /// Creates a new day set with the same value as the given day set
        /// </summary>
        /// <param name="from">The day set whose value should be inherited.
        /// </param>
        public DaySet(DaySet from)
        {
            _week = 0;
            SetTo(from);
        }

        #endregion

        #region Methods and properties

        /// <summary>
        /// Flags / Checks the given day within this day set.
        /// </summary>
        /// <param name="day">The day to check or flag.</param>
        /// <returns>true if the day is flagged within this set;
        /// false otherwise.</returns>
        public virtual bool this[DayOfWeek day]
        {
            get { return Contains(day); }
            set { if (value) Set(day); else Unset(day); }
        }

        /// <summary>
        /// Gets the configuration of this day set - this is very similar to that
        /// which is returned by <see cref="ToString"/>, but is a more succinct
        /// string representation of the day set.
        /// </summary>
        public string Configuration
        {
            get
            {
                StringBuilder sb = new StringBuilder("[", 20);
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    if (Contains(day))
                    {
                        if (sb.Length > 1)
                            sb.Append(',');
                        sb.Append(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(day));
                    }
                }
                sb.Append(']');

                return sb.ToString();
            }
        }

        /// <summary>
        /// Sets the flag for the given day
        /// </summary>
        /// <param name="day">The day which should be set <em>on</em> in this set.</param>
        public virtual void Set(DayOfWeek day)
        {
            _week |= (1 << (int)day);
        }

        /// <summary>
        /// Clears the flag for the given day
        /// </summary>
        /// <param name="day">The day which should be set <em>off</em> in this set.</param>
        public virtual void Unset(DayOfWeek day)
        {
            _week &= ~(1 << (int)day);
        }

        /// <summary>
        /// Checks if the given day is set on in this set or not.
        /// </summary>
        /// <param name="day">The day to check</param>
        /// <returns>true if the flag representing the given day is set within 
        /// this set; false otherwise.</returns>
        public bool Contains(DayOfWeek day)
        {
            return (_week & (1 << (int)day)) != 0;
        }

        /// <summary>
        /// Checks if this day set has at least one day set into it, and sets it into
        /// the output variable <paramref name="day"/> if it has.
        /// If it has not, then the variable is set to -1, making it an invalid day
        /// of the week (ie. it should not be used by calling code after this method
        /// has returned false)
        /// </summary>
        /// <param name="day">The output parameter which will contain the first day
        /// encountered in this day set if it has one, or the integer value -1 if it
        /// is empty.</param>
        /// <returns>true if at least one day was found in this dayset and returned
        /// in the output parameter; false otherwise.</returns>
        public bool HasAtLeastOneDay(out DayOfWeek day)
        {
            if (this.Any())
            {
                day = this.First();
                return true;
            }

            day = (DayOfWeek)(-1);
            return false;
        }

        /// <summary>
        /// Clears this set, ie. sets all day flags <em>off</em>.
        /// </summary>
        public virtual void Clear()
        {
            _week = 0;
        }

        /// <summary>
        /// Checks if this set is empty, ie. whether all days are currently
        /// set <em>off</em> in this set.
        /// </summary>
        /// <returns>true if no days are set on this day set; false otherwise.
        /// </returns>
        public bool IsEmpty()
        {
            return (_week & ALL_DAYS) == 0;
        }

        /// <summary>
        /// Checks if this set is full, ie. whether all days are currently
        /// set <em>on</em> in this set.
        /// </summary>
        /// <returns>true if all days are enabled within this day set; false
        /// otherwise.</returns>
        public bool IsFull()
        {
            return (_week & ALL_DAYS) == ALL_DAYS;
        }

        /// <summary>
        /// Sets this day set to the same value as the given day set.
        /// </summary>
        /// <param name="daySet">The day set whose value this object should
        /// copy.</param>
        public virtual void SetTo(DaySet daySet)
        {
            _week = daySet._week;
        }

        /// <summary>
        /// Sets the day flags in this set to the given bit set.
        /// </summary>
        /// <param name="flags">The integer representation of the day set
        /// that this object should be set to.</param>
        public virtual void SetTo(int flags)
        {
            _week = (flags & ALL_DAYS);
        }

        /// <summary>
        /// Gets the integer representation of this day set.
        /// </summary>
        /// <returns>The int which represents this dayset.</returns>
        public int ToInt()
        {
            return _week;
        }

        #endregion

        #region Operator overloads - only for equality checks

        /// <summary>
        /// Handle checking if 2 daysets are equal
        /// </summary>
        /// <param name="one">The first dayset to check</param>
        /// <param name="two">The second dayset to check</param>
        /// <returns>true if the given sets are equal; false otherwise.
        /// </returns>
        public static bool operator ==(DaySet one, DaySet two)
        {
            // downcast to object to avoid infinite loop calling strongly
            // typed "!=" operator
            if (((object)one) != null)
                return one.Equals(two);
            return ((object)two) == null; // likewise for the "==" operator

        }

        /// <summary>
        /// Handle checking if 2 daysets are not equal
        /// </summary>
        /// <param name="one">The first dayset to check</param>
        /// <param name="two">The second dayset to check</param>
        /// <returns>true if the given sets are <em>inequal</em>;
        /// false otherwise. </returns>
        public static bool operator !=(DaySet one, DaySet two)
        {
            // downcast to object to avoid infinite loop calling strongly
            // typed "!=" operator
            if (((object)one) != null)
                return !one.Equals(two);
            return ((object)two) != null;
        }

        #endregion

        #region object overloads

        /// <summary>
        /// Checks if the given object is equal to this set.
        /// It is considered equal if it is a DayFlags with the same days set
        /// on and off as this set.
        /// </summary>
        /// <param name="obj">The object to check for equality</param>
        /// <returns>true if the given object is a DayFlags instance which
        /// has the same flags raised as this object.</returns>
        public override bool Equals(object obj)
        {
            return (obj is DaySet) && ((DaySet)obj)._week == this._week;
        }

        /// <summary>
        /// Gets an integer hash of this object.
        /// </summary>
        /// <returns>A hash representing this object.</returns>
        public override int GetHashCode()
        {
            return _week.GetHashCode();
        }

        /// <summary>
        /// Gets a string representation of this object.
        /// </summary>
        /// <returns>A comma-separated list of all the days which are set
        /// on in this flag set; or "[No Days Set]" if it is currently
        /// empty.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                if (Contains(day))
                    sb.Append(day.ToString()).Append(',');
            }
            if (sb.Length == 0)
                return Resources.NoDaysSet;
            sb.Length--;
            return sb.ToString();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets an enumerator over the days of the week set in this
        /// flag set.
        /// </summary>
        /// <returns>An enumerator for the days of the week flagged on within
        /// this set.</returns>
        public IEnumerator<DayOfWeek> GetEnumerator()
        {
            return new DaySetEnumerator(this);
        }

        /// <summary>
        /// Gets an enumerator over the days of the week set in this
        /// flag set.
        /// </summary>
        /// <returns>An enumerator for the days of the week flagged on within
        /// this set.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Enumerator inner class

        /// <summary>
        /// An enumerator over a day set.
        /// </summary>
        private class DaySetEnumerator : IEnumerator<DayOfWeek>, IEnumerator
        {
            /// <summary>
            /// The flags containing the days to return.
            /// </summary>
            private DaySet flags;

            /// <summary>
            /// The current index within the day array that we have reached.
            /// </summary>
            private int day;

            /// <summary>
            /// Creates a new enumerator over the given flag set.
            /// </summary>
            /// <param name="flagSet">The flags to enumerate over.</param>
            public DaySetEnumerator(DaySet flagSet)
            {
                this.flags = flagSet;
                this.day = -1;
            }

            /// <summary>
            /// Gets the current day of the week within this enumerator
            /// </summary>
            public DayOfWeek Current
            {
                get
                {
                    if (day == -1)
                    {
                        throw new InvalidOperationException(Resources.EnumeratorHasNotBeenStarted);
                    }
                    else if (day == int.MaxValue)
                    {
                        throw new InvalidOperationException(Resources.EnumeratorHasFinished);
                    }
                    return (DayOfWeek)day;
                    //return DAYARRAY[day];
                }
            }

            /// <summary>
            /// Disposes of this enumerator.
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// Gets the current day of the week within this enumerator.
            /// </summary>
            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            /// <summary>
            /// Attempts to move to the next DayOfWeek marked within the day set
            /// </summary>
            /// <returns>true if the enumerator successfully moved to the next
            /// day within the set; false if there were no more days set.</returns>
            public bool MoveNext()
            {
                if (day == int.MaxValue)
                    return false;

                if (day == -1)
                {
                    day = (int)DaySet.FirstDayOfWeek;
                    // Check this outside the loop
                    if (flags.Contains(DaySet.FirstDayOfWeek))
                        return true;
                }

                // Now we add 1 and modulus down to 0..6 until we hit
                // FirstDayOfWeek again... if we do that then we've
                // run out of days to check.

                while ((day = (day + 1) % 7) != (int)DaySet.FirstDayOfWeek)
                {
                    if (flags.Contains((DayOfWeek)day))
                        return true;
                }
                // reached the first day of the week again... it's all over now.
                day = int.MaxValue;

                return false;
            }

            /// <summary>
            /// Resets the enumerator to the state it was in when it was created.
            /// </summary>
            public void Reset()
            {
                day = -1;
            }
        }
        #endregion

        #region ICollection Members

        /// <summary>
        /// Adds the given day of the week to this day set.
        /// </summary>
        /// <param name="item">The day of the week to add.</param>
        public virtual void Add(DayOfWeek item)
        {
            Set(item);
        }

        /// <summary>
        /// Copies the days held in this set to the given array, in natural
        /// order, starting from the <see cref="FirstDayOfWeek"/>.
        /// </summary>
        /// <param name="array">The array into which the days in this set 
        /// should be inserted</param>
        /// <param name="index">The index at which to start copying
        /// the days in this set.</param>
        /// <exception cref="ArgumentOutOfRangeException">arrayIndex is negative
        /// -or- arrayIndex points beyond the end of the given array
        /// -or- there is too little space in the array from arrayIndex to the
        /// end of the array to contain all the elements in this day set.
        /// </exception>
        /// <exception cref="ArgumentNullException">if array is null</exception>
        public void CopyTo(DayOfWeek[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (index < 0 || Count + index > array.Length)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", index,
                       Resources.StartIndexIsOutsideTheArrayBoundsOrLeavesTooLittleRoomForTheDaysInThisSet);
            }
            foreach (DayOfWeek day in this)
                array[index++] = day;
        }

        /// <summary>
        /// The number of days of the week set in this collection
        /// </summary>
        public int Count
        {
            get
            {
                int count = 0;
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    if (Contains(day))
                        count++;
                }
                return count;
            }
        }

        /// <summary>
        /// Indicates that this day set is not read only. Always false.
        /// </summary>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the given day of the week from this set.
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>true if the given item was found and removed from this set,
        /// false if this set did not contain it in the first place.</returns>
        public virtual bool Remove(DayOfWeek item)
        {
            if (Contains(item))
            {
                Unset(item);
                return true;
            }
            return false;
        }

        #endregion
    }
}
