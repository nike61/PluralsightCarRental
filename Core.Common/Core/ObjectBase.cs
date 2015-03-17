using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Core.Common.Contracts;
using Core.Common.Extensions;
using Core.Common.Utils;
using FluentValidation;
using FluentValidation.Results;

namespace Core.Common.Core
{
    public abstract class ObjectBase : NotificationObject, IDirtyCapable, IExtensibleDataObject, IDataErrorInfo
    {
        public ObjectBase()
        {
            _validator = GetValidator();
            Validate();
        }

        protected bool _isDirty = false;
        protected readonly IValidator _validator = null;

        protected IEnumerable<ValidationFailure> _validationErrors = null;

        #region IExtensibleDataObjects Members
        public ExtensionDataObject ExtensionData { get; set; }
        #endregion

        #region Property change notification
        protected override void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName, true);
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression, bool makeDirty)
        {
            string propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
            OnPropertyChanged(propertyName, makeDirty);
        }

        protected virtual void OnPropertyChanged(string propertyName, bool makeDirty)
        {
            base.OnPropertyChanged(propertyName);

            if (makeDirty)
                _isDirty = true;

            Validate();
        }

        #endregion

        #region Validation

        protected virtual IValidator GetValidator()
        {
            return null;
        }

        [NotNavigable]
        public IEnumerable<ValidationFailure> ValidationErrors
        {
            get { return _validationErrors; }
            set { }
        }

        public void Validate()
        {
            if (_validator != null)
            {
                ValidationResult results = _validator.Validate(this);
                _validationErrors = results.Errors;
            }
        }

        [NotNavigable]
        public virtual bool IsValid
        {
            get
            {
                if (_validationErrors != null && _validationErrors.Any())
                    return false;
                return true;
            }
        }

        #endregion

        #region IDirtyCapable Members
        [NotNavigable]
        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }

        public List<IDirtyCapable> GetDirtyObjects()
        {
            var dirtyObjects = new List<IDirtyCapable>();
            WalkObjectGraph(
                o =>
                {
                    if (o.IsDirty)
                        dirtyObjects.Add(o);

                    return false;
                }, coll => { });

            return dirtyObjects;
        }

        public void CleanAll()
        {
            WalkObjectGraph(
                o =>
                {
                    if (o.IsDirty)
                        o.IsDirty = false;
                    
                    return false;
                }, coll => { });
        }

        public virtual bool IsAnythingDirty()
        {
            bool isDirty = false;
            WalkObjectGraph(
                o =>
                {
                    if (o.IsDirty)
                    {
                        isDirty = true;
                        return true;
                    }
                    return false;
                }, coll => { }
                );

            return isDirty;
        }

        protected void WalkObjectGraph(Func<ObjectBase, bool> snippetForObject,
            Action<IList> snippetForCollection,
            params string[] exemptProperties)
        {
            var visited = new List<ObjectBase>();
            Action<ObjectBase> walk = null;

            var exemptions = new List<string>();
            if (exemptProperties != null)
                exemptions = exemptProperties.ToList();

            walk = o =>
            {
                if (o != null && !visited.Contains(o))
                {
                    visited.Add(o);

                    bool exitWalk = snippetForObject.Invoke(o);

                    if (!exitWalk)
                    {
                        PropertyInfo[] properties = o.GetBrowsableProperties();
                        foreach (var property in properties)
                        {
                            if (!exemptions.Contains(property.Name))
                            {
                                if (property.PropertyType.IsSubclassOf(typeof (ObjectBase)))
                                {
                                    var obj = (ObjectBase) (property.GetValue(o, null));
                                    walk(obj);
                                }
                                else
                                {
                                    var coll = property.GetValue(0, null) as IList;
                                    if (coll != null)
                                    {
                                        snippetForCollection.Invoke(coll);

                                        foreach (var item in coll)
                                        {
                                            if (item is ObjectBase)
                                                walk((ObjectBase) item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            walk(this);
        }

        #endregion

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return string.Empty; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                var errors  = new StringBuilder();
                if (_validationErrors != null && _validationErrors.Any())
                {
                    foreach (ValidationFailure validationError in _validationErrors)
                    {
                        if (validationError.PropertyName == columnName)
                            errors.AppendLine(validationError.ErrorMessage);
                    }
                }

                return errors.ToString();
            }
        }
        #endregion
    }

}
