using System;
using System.Collections.Generic;
using System.Dynamic;

namespace EImece.Domain.Helpers.EmailHelper
{
    /// <summary>
    /// Dynamic Razor model so templates can use @Model.Property (and nested @Model.Foo.Bar)
    /// without a strongly-typed class for every template.
    /// </summary>
    public sealed class DynamicMailTemplateModel : DynamicObject
    {
        private readonly Dictionary<string, object> _values =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public void SetValue(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            _values[name] = value ?? string.Empty;
        }

        public DynamicMailTemplateModel GetOrCreateChild(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return this;
            }

            object existing;
            if (_values.TryGetValue(name, out existing) && existing is DynamicMailTemplateModel child)
            {
                return child;
            }

            var created = new DynamicMailTemplateModel();
            _values[name] = created;
            return created;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
            {
                result = string.Empty;
                return true;
            }

            if (_values.TryGetValue(binder.Name, out result))
            {
                return true;
            }

            result = string.Empty;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder == null || string.IsNullOrWhiteSpace(binder.Name))
            {
                return false;
            }

            SetValue(binder.Name, value);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes != null && indexes.Length == 1 && indexes[0] != null)
            {
                var key = indexes[0].ToString();
                if (_values.TryGetValue(key, out result))
                {
                    return true;
                }
            }

            result = string.Empty;
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _values.Keys;
        }
    }
}
