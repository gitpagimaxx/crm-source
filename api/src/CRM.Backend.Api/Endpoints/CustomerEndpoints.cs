using CRM.Backend.Application.Commands.CreateCustomer;
using CRM.Backend.Application.Commands.UpdateCustomer;
using CRM.Backend.Application.Common;
using CRM.Backend.Application.Interfaces;
using CRM.Backend.Application.Queries.CheckDocumentUniqueness;
using CRM.Backend.Application.Queries.GetCustomer;
using CRM.Backend.Application.Queries.GetCustomerEvents;
using CRM.Backend.Application.Queries.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Backend.Api.Endpoints;

public static class CustomerEndpoints
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public static void MapCustomerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/customers").RequireAuthorization();

        group.MapPost("/", async (CreateCustomerCommand command, IMediator mediator, HttpContext ctx, UserContext userCtx) =>
        {
            SetUserContext(ctx, userCtx);
            var id = await mediator.Send(command);
            return Results.Created($"/customers/{id}", new { id });
        })
        .WithName("CreateCustomer")
        .WithTags("Customers");

        group.MapPut("/{id:guid}", async (Guid id, UpdateCustomerRequest request, IMediator mediator, HttpContext ctx, UserContext userCtx) =>
        {
            SetUserContext(ctx, userCtx);
            var command = new UpdateCustomerCommand(id, request.Name, request.Email, request.CompanyName, request.StateRegistration, request.ZipCode, request.AddressNumber, request.AddressComplement);
            await mediator.Send(command);
            return Results.NoContent();
        })
        .WithName("UpdateCustomer")
        .WithTags("Customers");

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCustomerQuery(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetCustomer")
        .WithTags("Customers");

        group.MapGet("/", async ([FromQuery] int page, [FromQuery] int pageSize, IMediator mediator) =>
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > MaxPageSize) pageSize = DefaultPageSize;
            var result = await mediator.Send(new GetCustomersQuery(page, pageSize));
            return Results.Ok(result);
        })
        .WithName("GetCustomers")
        .WithTags("Customers");

        group.MapGet("/{id:guid}/events", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCustomerEventsQuery(id));
            return Results.Ok(result);
        })
        .WithName("GetCustomerEvents")
        .WithTags("Customers");

        group.MapGet("/check-document/{document}", async (string document, IMediator mediator) =>
        {
            var result = await mediator.Send(new CheckDocumentUniquenessQuery(document));
            return Results.Ok(result);
        })
        .WithName("CheckDocumentUniqueness")
        .WithTags("Customers");

        group.MapGet("/address/{zipCode}", async (string zipCode, IViaCepService viaCepService) =>
        {
            var result = await viaCepService.GetAddress(zipCode);
            return result is null ? Results.NotFound(new { message = "CEP não encontrado" }) : Results.Ok(result);
        })
        .WithName("GetAddressByZipCode")
        .WithTags("Customers");
    }

    private static void SetUserContext(HttpContext ctx, UserContext userCtx)
    {
        var user = ctx.User;
        userCtx.UserId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;
        userCtx.Email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? user.FindFirst("email")?.Value;
        userCtx.Name = user.FindFirst("name")?.Value;
        userCtx.CorrelationId = ctx.TraceIdentifier;
    }
}

public record UpdateCustomerRequest(string Name, string Email, string? CompanyName, string? StateRegistration, string? ZipCode, string? AddressNumber, string? AddressComplement);