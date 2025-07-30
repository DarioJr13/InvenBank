namespace InvenBank.API.DTOs.Requests
{
    public class UpdateCategoryRequest : CreateCategoryRequest
    {
        public bool IsActive { get; set; } = true;
    }
}
