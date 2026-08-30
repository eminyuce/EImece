using EImece.Domain.Helpers;
using System;

namespace EImece.Domain.Models.FrontModels
{
    public class Filter
    {
        public string FilterLink { get; set; }

        public String FieldName { get; set; }
        private string _valueFirst = "";

        public String ValueFirst
        { get { return _valueFirst; } set { _valueFirst = value; } }

        public String ValueLast { get; set; }

        public int Cnt { get; set; }
        public int Ord { get; set; }

        private string _text = "";

        public string Text
        {
            get
            {
                if (string.IsNullOrEmpty(_text))
                {
                    if (!String.IsNullOrEmpty(ValueLast))
                    {
                        if (ValueFirst == ValueLast)
                        {
                            return ValueFirst;
                        }
                        else
                        {
                            return ValueLast;
                        }
                    }
                    else
                    {
                        return ValueFirst;
                    }
                }
                else
                {
                    return _text;
                }
            }
            set { _text = value; }
        }

        public Filter()
        {
        }

        public string Url
        {
            get
            {
                string url = FieldName.UrlEncode() + "-";
                if (ValueFirst == ValueLast)
                {
                    url += ValueFirst.UrlEncode();
                }
                else
                {
                    url += ValueFirst.UrlEncode() + (!string.IsNullOrEmpty(ValueLast) ? "-" + ValueLast.UrlEncode() : "");
                }

                return url.Trim();
            }
        }

        public Filter(string fieldName, string valueFirst, string valueLast)
        {
            this.FieldName = fieldName;
            this.ValueFirst = valueFirst;
            this.ValueLast = valueLast;
        }

        private ItemType _ownerType;

        public ItemType OwnerType
        { get { return _ownerType; } set { _ownerType = value; } }
    }
}