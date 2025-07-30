namespace InvenBank.API.DTOs.Requests
{
    public class UpdateProductRequest : CreateProductRequest
    {
        public bool IsActive { get; set; } = true;
    }
}
