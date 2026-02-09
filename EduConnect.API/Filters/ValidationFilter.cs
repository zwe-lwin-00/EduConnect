using EduConnect.API.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EduConnect.API.Filters;

/// <summary>
/// Validates request body using FluentValidation and returns standard ApiErrorResponse when validation fails.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
#pragma warning disable CS0618 // IValidatorFactory is obsolete but still supported
    private readonly IValidatorFactory _validatorFactory;

    public ValidationFilter(IValidatorFactory validatorFactory)
#pragma warning restore CS0618
    {
        _validatorFactory = validatorFactory;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var (key, value) in context.ActionArguments)
        {
            if (value == null) continue;
            var validator = _validatorFactory.GetValidator(value.GetType());
            if (validator == null) continue;

            var contextType = typeof(ValidationContext<>).MakeGenericType(value.GetType());
            var validationContext = (IValidationContext)Activator.CreateInstance(contextType, value)!;
            var result = await validator.ValidateAsync(validationContext);
            if (result.IsValid) continue;

            var errors = result.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }).ToList();
            var response = new ApiErrorResponse
            {
                Error = "One or more validation errors occurred.",
                Code = "VALIDATION_ERROR",
                Details = errors,
                RequestId = context.HttpContext.TraceIdentifier
            };
            context.Result = new BadRequestObjectResult(response);
            return;
        }
        await next();
    }
}
