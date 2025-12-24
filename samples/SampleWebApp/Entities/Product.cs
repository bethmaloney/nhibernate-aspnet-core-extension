namespace SampleWebApp.Entities;

public class Product
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; } = string.Empty;
    public virtual decimal Price { get; set; }
    public virtual string? Description { get; set; }
}
