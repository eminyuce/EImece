using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    public class ShortUrl : BaseEntity
    {
        public string UrlKey { get; set; }
        public string Url { get; set; }
        public int RequestCount { get; set; }

        /// <summary>
        /// ShortUrls.Lang is nvarchar in the existing database; <see cref="BaseEntity.Lang"/> is int
        /// and is ignored for this entity in the EF model.
        /// </summary>
        [Column("Lang")]
        public string LangValue { get; set; }
    }
}