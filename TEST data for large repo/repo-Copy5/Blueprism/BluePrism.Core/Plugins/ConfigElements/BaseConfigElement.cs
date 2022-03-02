namespace BluePrism.Core.Plugins.ConfigElements
{
    public abstract class BaseConfigElement<T> : IConfigElement
    {
        #region - Member Variables -

        private T _value;
        private bool _valueSet;

        #endregion

        #region - Auto-properties -

        public virtual string Name { get; set; }

        public virtual T DefaultValue { get; set; }

        #endregion

        #region - Constructors -

        protected BaseConfigElement()
        {
            Value = DefaultValue;
        }

        #endregion

        #region - Properties -

        public virtual T Value
        {
            get
            {
                if (!_valueSet) return DefaultValue;
                return _value;
            }
            set
            {
                _value = value;
                _valueSet = true;
            }
        }

        object IConfigElement.Value
        {
            get { return this.Value; }
            set { this.Value = (T)value; }
        }

        string IConfigElement.Name
        {
            get { return this.Name; }
        }

        #endregion

        #region - Methods -

        public void ClearValue()
        {
            Value = default(T);
            _valueSet = false;
        }

        #endregion

    }
}
