using System;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;

namespace EImece.Domain.Helpers.EmailHelper
{
    /// <summary>
    /// Leaf value used when rendering test emails. Razor templates often call
    /// <c>ToString(format)</c>, <c>CurrencySign()</c> or <c>ToDecimal()</c> on model
    /// properties; those are instance methods here so they work through <c>dynamic</c>.
    /// </summary>
    public sealed class DummyMailValue : DynamicObject, IComparable
    {
        private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");
        private readonly object _value;

        public DummyMailValue(object value)
        {
            _value = value ?? string.Empty;
        }

        public object RawValue
        {
            get { return _value; }
        }

        public override string ToString()
        {
            if (_value == null)
            {
                return string.Empty;
            }

            var formattable = _value as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(null, Turkish);
            }

            return System.Convert.ToString(_value, Turkish) ?? string.Empty;
        }

        public string ToString(string format)
        {
            var formattable = _value as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(format, Turkish);
            }

            DateTime date;
            if (DateTime.TryParse(ToString(), Turkish, DateTimeStyles.None, out date))
            {
                return date.ToString(format, Turkish);
            }

            return ToString();
        }

        public string CurrencySign()
        {
            return CurrencyHelper.CurrencySign(AsDecimal());
        }

        public DummyMailValue ToDecimal()
        {
            return new DummyMailValue(AsDecimal());
        }

        public bool Equals(string other)
        {
            return string.Equals(ToString(), other ?? string.Empty, StringComparison.Ordinal);
        }

        public int CompareTo(object obj)
        {
            return AsDecimal().CompareTo(ConvertToDecimal(obj));
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder == null)
            {
                result = null;
                return false;
            }

            var target = Nullable.GetUnderlyingType(binder.Type) ?? binder.Type;
            if (target == typeof(string))
            {
                result = ToString();
                return true;
            }

            if (target == typeof(decimal))
            {
                result = AsDecimal();
                return true;
            }

            if (target == typeof(double))
            {
                result = (double)AsDecimal();
                return true;
            }

            if (target == typeof(int))
            {
                result = (int)AsDecimal();
                return true;
            }

            if (target == typeof(bool))
            {
                bool parsed;
                result = bool.TryParse(ToString(), out parsed) && parsed;
                return true;
            }

            if (target == typeof(DateTime))
            {
                result = AsDateTime();
                return true;
            }

            try
            {
                result = System.Convert.ChangeType(_value, target, Turkish);
                return true;
            }
            catch (InvalidCastException)
            {
                result = null;
                return false;
            }
            catch (FormatException)
            {
                result = null;
                return false;
            }
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            result = null;
            if (binder == null)
            {
                return false;
            }

            var left = AsDecimal();
            decimal right;
            if (!TryConvertToDecimal(arg, out right))
            {
                return false;
            }

            switch (binder.Operation)
            {
                case ExpressionType.GreaterThan:
                    result = left > right;
                    return true;
                case ExpressionType.GreaterThanOrEqual:
                    result = left >= right;
                    return true;
                case ExpressionType.LessThan:
                    result = left < right;
                    return true;
                case ExpressionType.LessThanOrEqual:
                    result = left <= right;
                    return true;
                case ExpressionType.Equal:
                    result = left == right;
                    return true;
                case ExpressionType.NotEqual:
                    result = left != right;
                    return true;
                case ExpressionType.Add:
                    result = new DummyMailValue(left + right);
                    return true;
                case ExpressionType.Subtract:
                    result = new DummyMailValue(left - right);
                    return true;
                case ExpressionType.Multiply:
                    result = new DummyMailValue(left * right);
                    return true;
                case ExpressionType.Divide:
                    result = new DummyMailValue(right == 0 ? 0 : left / right);
                    return true;
                default:
                    return false;
            }
        }

        private decimal AsDecimal()
        {
            decimal parsed;
            TryConvertToDecimal(_value, out parsed);
            return parsed;
        }

        private DateTime AsDateTime()
        {
            if (_value is DateTime)
            {
                return (DateTime)_value;
            }

            DateTime parsed;
            if (DateTime.TryParse(ToString(), Turkish, DateTimeStyles.None, out parsed))
            {
                return parsed;
            }

            return DateTime.Now;
        }

        private static bool TryConvertToDecimal(object value, out decimal result)
        {
            var dummy = value as DummyMailValue;
            if (dummy != null)
            {
                return TryConvertToDecimal(dummy._value, out result);
            }

            if (value is decimal)
            {
                result = (decimal)value;
                return true;
            }

            if (value is int)
            {
                result = (int)value;
                return true;
            }

            if (value is double)
            {
                result = System.Convert.ToDecimal((double)value);
                return true;
            }

            if (value is float)
            {
                result = System.Convert.ToDecimal((float)value);
                return true;
            }

            if (value is long)
            {
                result = (long)value;
                return true;
            }

            return decimal.TryParse(
                System.Convert.ToString(value, CultureInfo.InvariantCulture),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out result)
                || decimal.TryParse(
                    System.Convert.ToString(value, Turkish),
                    NumberStyles.Any,
                    Turkish,
                    out result);
        }

        private static decimal ConvertToDecimal(object value)
        {
            decimal parsed;
            TryConvertToDecimal(value, out parsed);
            return parsed;
        }
    }
}
