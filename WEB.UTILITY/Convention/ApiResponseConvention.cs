using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using WEB.UTILITY.Helper;

namespace WEB.UTILITY.Convention
{
    public class ApiResponseConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            var responseType = typeof(ApiResponse<object>);

            foreach (var controller in application.Controllers)
            {
                foreach (var action in controller.Actions)
                {
                    if (action.Filters.OfType<ProducesResponseTypeAttribute>().Any())
                        continue;

                    action.Filters.Add(new ProducesResponseTypeAttribute(responseType, StatusCodes.Status200OK));
                    action.Filters.Add(new ProducesResponseTypeAttribute(responseType, StatusCodes.Status201Created));
                    action.Filters.Add(new ProducesResponseTypeAttribute(responseType, StatusCodes.Status400BadRequest));
                    action.Filters.Add(new ProducesResponseTypeAttribute(responseType, StatusCodes.Status401Unauthorized));
                    action.Filters.Add(new ProducesResponseTypeAttribute(responseType, StatusCodes.Status403Forbidden));
                    action.Filters.Add(new ProducesResponseTypeAttribute(responseType, StatusCodes.Status404NotFound));
                    action.Filters.Add(new ProducesResponseTypeAttribute(responseType, StatusCodes.Status500InternalServerError));
                }
            }
        }
    }
}
