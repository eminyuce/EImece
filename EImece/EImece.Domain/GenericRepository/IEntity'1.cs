using System;

namespace EImece.Domain.GenericRepository
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    public interface IEntity<TId> where TId : IComparable
    {
        /// <summary>
        /// Id of the Entity
        /// </summary>
        TId Id { get; set; }
    }
}