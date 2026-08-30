namespace EImece.Domain.Models.DTOs
{
    public class SpecificationOptionItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }

        public SpecificationOptionItem()
        {
        }

        public SpecificationOptionItem(string text, string value, bool selected = false)
        {
            Text = text;
            Value = value;
            Selected = selected;
        }
    }
}
