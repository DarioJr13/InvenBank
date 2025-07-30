namespace InvenBank.API.Entities
{
    public abstract class ActiveEntity : BaseEntity
    {
        public bool IsActive { get; set; } = true;
    }
}
