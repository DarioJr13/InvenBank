namespace InvenBank.API.DTOs.Requests
{
    public class UpdateSupplierRequest : CreateSupplierRequest
    {
        public bool IsActive { get; set; } = true;
    }
}
