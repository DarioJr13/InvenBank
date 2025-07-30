namespace InvenBank.API.DTOs
{
    public abstract class ActiveDto : BaseDto
    {
        public bool IsActive { get; set; } = true;
    }

}
