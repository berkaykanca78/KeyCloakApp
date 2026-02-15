using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.DTOs;
using OrderApi.Domain.Aggregates;
using OrderApi.Domain.Repositories;
using Shared.Api;

namespace OrderApi.Presentation.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;

    public CustomersController(ICustomerRepository customerRepository) => _customerRepository = customerRepository;

    /// <summary>Kayıt sonrası AuthApi veya frontend tarafından müşteri oluşturur (Keycloak sub ile).</summary>
    [HttpPost]
    public async Task<ActionResult<ResultDto<Customer>>> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _customerRepository.GetByKeycloakSubAsync(request.KeycloakSub, cancellationToken);
            if (existing != null)
                return Ok(ResultDto<Customer>.Success(existing, "Müşteri zaten mevcut."));

            var customer = Customer.Create(
                request.KeycloakSub,
                request.FirstName,
                request.LastName,
                request.Address,
                request.CityId,
                request.DistrictId,
                request.CardLast4);
            _customerRepository.Add(customer);
            await _customerRepository.SaveChangesAsync(cancellationToken);
            return Ok(ResultDto<Customer>.Success(customer, "Müşteri oluşturuldu."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ResultDto<Customer>.Failure(ex.Message));
        }
    }

    /// <summary>Giriş yapan kullanıcının müşteri kaydı (sub claim ile).</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpGet("me")]
    public async Task<ActionResult<ResultDto<Customer>>> GetMe(CancellationToken cancellationToken = default)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sub))
            return NotFound(ResultDto<Customer>.Failure("Kullanıcı bilgisi bulunamadı."));
        var customer = await _customerRepository.GetByKeycloakSubAsync(sub, cancellationToken);
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

        var existing = await _customerRepository.GetByKeycloakSubAsync(sub, cancellationToken);
        if (existing != null)
            return Ok(ResultDto<Customer>.Success(existing, "Müşteri zaten mevcut."));

        var firstName = request?.FirstName?.Trim();
        var lastName = request?.LastName?.Trim();
        if (string.IsNullOrEmpty(firstName))
            firstName = User.FindFirst("given_name")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "";
        if (string.IsNullOrEmpty(lastName))
            lastName = User.FindFirst("family_name")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? "";
        if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
        {
            var name = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "";
            var parts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            firstName = parts.Length > 0 ? parts[0] : "Kullanıcı";
            lastName = parts.Length > 1 ? parts[1] : "";
        }
        if (string.IsNullOrEmpty(firstName))
            firstName = "Kullanıcı";

        var customer = Customer.Create(
            sub,
            firstName,
            lastName,
            request?.Address,
            request?.CityId,
            request?.DistrictId,
            request?.CardLast4);
        _customerRepository.Add(customer);
        await _customerRepository.SaveChangesAsync(cancellationToken);
        return Ok(ResultDto<Customer>.Success(customer, "Müşteri kaydı oluşturuldu."));
    }
}
