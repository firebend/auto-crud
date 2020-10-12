using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IEntityMapping<T> where T : class
    {
        void Map(EntityTypeBuilder<T> entityTypeConfiguration);
    }
}