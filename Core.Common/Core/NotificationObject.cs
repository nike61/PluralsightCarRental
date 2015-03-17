using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using Core.Common.Utils;

namespace Core.Common.Core
{
    public class NotificationObject : INotifyPropertyChanged
    {
        private event PropertyChangedEventHandler _propertyChangedEvent;

        protected List<PropertyChangedEventHandler> _propertyChangedSubscribers = new List<PropertyChangedEventHandler>();

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (!_propertyChangedSubscribers.Contains(value))
                {
                    _propertyChangedEvent += value;
                    _propertyChangedSubscribers.Add(value);
                }
            }
            remove
            {
                _propertyChangedEvent -= value;
                _propertyChangedSubscribers.Remove(value);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (_propertyChangedEvent != null)
                _propertyChangedEvent(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            string propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
            OnPropertyChanged(propertyName);
        }
    }
}
