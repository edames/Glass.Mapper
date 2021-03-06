


using System;
using System.Reflection;

namespace Glass.Mapper.Configuration
{
    /// <summary>
    /// Represents the configuration for a property on a .Net type
    /// </summary>
    public abstract class AbstractPropertyConfiguration
    {
		private PropertyInfo _propertyInfo;

        private Action<object, object> _propertySetter;
        private Func<object, object> _propertyGetter;


        /// <summary>
        /// Gets or sets the property info.
        /// </summary>
        /// <value>The property info.</value>
        public PropertyInfo PropertyInfo
		{
			get { return _propertyInfo; }
			set
			{
				_propertyInfo = value;
			    _propertyGetter = Utilities.GetPropertyFunc(PropertyInfo); ;
			    _propertySetter = Utilities.SetPropertyAction(PropertyInfo);
            }
		}



        /// <summary>
        /// Function to get the underlying property value
        /// </summary>
        public Func<object, object> PropertyGetter
        {
            get
            {
                if (_propertyGetter == null)
                {
                    _propertyGetter = Utilities.GetPropertyFunc(PropertyInfo);
                }

                return _propertyGetter;
            }

        }

        /// <summary>
		/// Action to set the underyling property value
		/// </summary>
		public Action<object, object> PropertySetter {
            get
            {
                if (_propertySetter == null)
                {
                    _propertySetter = Utilities.SetPropertyAction(PropertyInfo);
                }

                return _propertySetter;
            }
        }

        /// <summary>
        /// Gets the mapper.
        /// </summary>
        /// <value>The mapper.</value>
        public AbstractDataMapper Mapper  { get; internal set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            if (PropertyInfo == null)
                return "AbstractPropertyConfiguration: Property: Null";

            return "AbstractPropertyConfiguration Property: {0} Type: {1} Assembly: {2}".Formatted(PropertyInfo.Name,
                                                                     PropertyInfo.ReflectedType.FullName,
                                                                     PropertyInfo.ReflectedType.Assembly.FullName);
        }

        protected  abstract AbstractPropertyConfiguration CreateCopy();

        protected virtual void Copy(AbstractPropertyConfiguration copy)
        {
            copy.PropertyInfo = PropertyInfo;

        }
        public  AbstractPropertyConfiguration Copy()
        {
            var configCopy = CreateCopy();
            Copy(configCopy);
            return configCopy;
        }



        public virtual void GetPropertyOptions(GetOptions requestOptions) 
        {
            requestOptions.Type = this.PropertyInfo.PropertyType;
        }
    }
}




