using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WEB.UTILITY.Convention
{
    public class ApiResponseConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                foreach (var action in controller.Actions)
                {
                    // Add default response types if not already defined
                    if (!action.Filters.OfType<ProducesResponseTypeAttribute>().Any())
                    {
                        action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status200OK));
                        action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));
                        action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status201Created));
                        action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status403Forbidden));
                        action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status404NotFound));
                        action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status401Unauthorized));
                        action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
                    }
                }
            }
        }
    }
}
