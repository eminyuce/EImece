using EImece.Domain.Entities;

namespace EImece.Domain.Factories.IFactories
{
    public interface IEntityFactory
    {
        T GetBaseEntityInstance<T>() where T : BaseEntity, new();

        T GetBaseContentInstance<T>() where T : BaseContent, new();
    }
}