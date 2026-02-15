using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ordering.API.Application.DTOs;
using Ordering.API.Application.Ports;
using Ordering.API.Domain.Aggregates;
using Shared.Api;

namespace Ordering.API.Presentation.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService) => _customerService = customerService;

    /// <summary>Kayıt sonrası Identity.API veya frontend tarafından müşteri oluşturur (Keycloak sub ile).</summary>
    [HttpPost]
    public async Task<ActionResult<ResultDto<Customer>>> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var (customer, alreadyExisted) = await _customerService.CreateAsync(request, cancellationToken);
        if (customer == null)
            return BadRequest(ResultDto<Customer>.Failure("Geçersiz istek."));
        return Ok(ResultDto<Customer>.Success(customer, alreadyExisted ? "Müşteri zaten mevcut." : "Müşteri oluşturuldu."));
    }

    /// <summary>Giriş yapan kullanıcının müşteri kaydı (sub claim ile).</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpGet("me")]
    public async Task<ActionResult<ResultDto<Customer>>> GetMe(CancellationToken cancellationToken = default)
    {
        var result = await _customerService.GetMeAsync(User, cancellationToken);
        if (result.Unauthorized)
            return NotFound(ResultDto<Customer>.Failure("Kullanıcı bilgisi bulunamadı."));
        if (result.NotFoundMessage != null)
            return NotFound(ResultDto<Customer>.Failure(result.NotFoundMessage));
        return Ok(ResultDto<Customer>.Success(result.Customer!));
    }

    /// <summary>Giriş yapan kullanıcı için müşteri kaydı oluşturur (sub claim ile). Kayıt yoksa token bilgileriyle oluşturulur.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpPost("me")]
    public async Task<ActionResult<ResultDto<Customer>>> CreateMe([FromBody] CreateCustomerMeRequest? request, CancellationToken cancellationToken = default)
    {
        var result = await _customerService.CreateMeAsync(User, request, cancellationToken);
        if (result.Unauthorized)
            return Unauthorized(ResultDto<Customer>.Failure("Kullanıcı bilgisi bulunamadı."));
        if (result.ServerErrorMessage != null)
            return StatusCode(500, ResultDto<Customer>.Failure(result.ServerErrorMessage));
        return Ok(ResultDto<Customer>.Success(result.Customer!, result.AlreadyExisted ? "Müşteri zaten mevcut." : "Müşteri kaydı oluşturuldu."));
    }
}
