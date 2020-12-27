using System;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class ShoppingCart : BaseEntity
    {
        public string OrderGuid { get; set; }
        public string ShoppingCartJson { get; set; }
    }
}