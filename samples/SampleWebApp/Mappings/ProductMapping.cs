using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using SampleWebApp.Entities;

namespace SampleWebApp.Mappings;

public class ProductMapping : ClassMapping<Product>
{
    public ProductMapping()
    {
        Table("Products");

        Id(x => x.Id, map =>
        {
            map.Generator(Generators.Identity);
        });

        Property(x => x.Name, map =>
        {
            map.NotNullable(true);
            map.Length(200);
        });

        Property(x => x.Price, map =>
        {
            map.NotNullable(true);
        });

        Property(x => x.Description, map =>
        {
            map.Length(1000);
        });
    }
}
