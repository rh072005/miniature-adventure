using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;

namespace SwashbuckleAspNetApiVersioningExample
{
    public class ApiExplorerGroupPerVersionConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var versions = SwaggerVersioning.GetApiVersionsForController(controller.ControllerType);
            if (versions.Any())
            {
                controller.ApiExplorer.GroupName = string.Join("#", versions);
            }
        }
    }
}
