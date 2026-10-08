using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value.Errors.First().ErrorMessage
                );

            context.Result = new BadRequestObjectResult(new
            {
                Status = false,
                Errors = errors
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}