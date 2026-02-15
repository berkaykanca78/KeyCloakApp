using System.Security.Claims;
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
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sub))
            return NotFound(ResultDto<Customer>.Failure("Kullanıcı bilgisi bulunamadı."));
        var customer = await _customerService.GetByKeycloakSubAsync(sub, cancellationToken);
        if (customer == null)
            return NotFound(ResultDto<Customer>.Failure("Müşteri kaydı bulunamadı. Önce kayıt olun."));
        return Ok(ResultDto<Customer>.Success(customer));
    }

    /// <summary>Giriş yapan kullanıcı için müşteri kaydı oluşturur (sub claim ile). Kayıt yoksa token bilgileriyle oluşturulur.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpPost("me")]
    public async Task<ActionResult<ResultDto<Customer>>> CreateMe([FromBody] CreateCustomerMeRequest? request, CancellationToken cancellationToken = default)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sub))
            return Unauthorized(ResultDto<Customer>.Failure("Kullanıcı bilgisi bulunamadı."));

        var firstName = request?.FirstName?.Trim();
        var lastName = request?.LastName?.Trim();
        if (string.IsNullOrEmpty(firstName))
            firstName = User.FindFirst("given_name")?.Value ?? User.FindFirst(ClaimTypes.GivenName)?.Value ?? "";
        if (string.IsNullOrEmpty(lastName))
            lastName = User.FindFirst("family_name")?.Value ?? User.FindFirst(ClaimTypes.Surname)?.Value ?? "";
        if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
        {
            var name = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "";
            var parts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            firstName = parts.Length > 0 ? parts[0] : "Kullanıcı";
            lastName = parts.Length > 1 ? parts[1] : "";
        }
        if (string.IsNullOrEmpty(firstName))
            firstName = "Kullanıcı";

        var (customer, alreadyExisted) = await _customerService.CreateMeAsync(sub, firstName, lastName, request?.Address, request?.CityId, request?.DistrictId, request?.CardLast4, cancellationToken);
        if (customer == null)
            return StatusCode(500, ResultDto<Customer>.Failure("Müşteri kaydı oluşturulamadı."));
        return Ok(ResultDto<Customer>.Success(customer, alreadyExisted ? "Müşteri zaten mevcut." : "Müşteri kaydı oluşturuldu."));
    }
}
