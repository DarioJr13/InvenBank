using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenBank.API.Controllers.Admin;

[ApiController]
[Route("api/admin/products/{productId:int}/suppliers")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class ProductSuppliersController : ControllerBase
{
    private readonly IProductSupplierService _service;
    private readonly ILogger<ProductSuppliersController> _logger;

    public ProductSuppliersController(IProductSupplierService service, ILogger<ProductSuppliersController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int productId)
    {
        var result = await _service.GetByProductIdAsync(productId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int productId, [FromBody] CreateProductSupplierRequest request)
    {
        request.ProductId = productId;
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetAll), new { productId }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int productId, int id, [FromBody] UpdateProductSupplierRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int productId, int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
