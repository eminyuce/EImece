namespace EImece.Domain.Core.Entities;

public interface IEntity<TId> where TId : IComparable
{
    TId Id { get; set; }
}
