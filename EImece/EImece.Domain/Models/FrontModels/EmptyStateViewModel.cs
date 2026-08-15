namespace EImece.Domain.Models.FrontModels
{
    /// <summary>
    /// Model for rendering consistent empty-state UI across storefront lists and carts.
    /// </summary>
    public class EmptyStateViewModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string ActionUrl { get; set; }
        public string ActionText { get; set; }
        public string IconClass { get; set; }

        public EmptyStateViewModel()
        {
        }

        public EmptyStateViewModel(string title, string message = null, string actionUrl = null, string actionText = null, string iconClass = null)
        {
            Title = title;
            Message = message;
            ActionUrl = actionUrl;
            ActionText = actionText;
            IconClass = iconClass;
        }
    }
}
