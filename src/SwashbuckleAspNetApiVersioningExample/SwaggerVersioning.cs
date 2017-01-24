using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SwashbuckleAspNetApiVersioningExample
{
    public static class SwaggerVersioning
    {
        public static bool SetDocInclusions(string version, ApiDescription apiDescription)
        {
            var versions = apiDescription.GroupName?.Split('#');
            if(versions == null)
            {
                return false;
            }
            if (!versions.Contains(version.Replace("v", "")))
            {
                return false;
            }

            //This is the line that ends up with the wrong version
            var versionRegex = new Regex("^(v\\d|v{version})$");
            var values = apiDescription.RelativePath.Split('/').Select(v => versionRegex.Replace(v, version));
            apiDescription.RelativePath = string.Join("/", values);
            var versionParameter = apiDescription.ParameterDescriptions.SingleOrDefault(p => p.Name == "version");

            if (versionParameter != null)
            {
                apiDescription.ParameterDescriptions.Remove(versionParameter);
            }

            foreach (var parameter in apiDescription.ParameterDescriptions)
            {
                parameter.Name = char.ToLowerInvariant(parameter.Name[0]) + parameter.Name.Substring(1);
            }

            return true;
        }

        public static List<string> GetAllApiVersions()
        {
            var controllers = GetSubClasses<Controller>();
            var versionList = new List<string>();
            var versionedApiControllers = controllers.Select(x => x.CustomAttributes?.Where(y => y.AttributeType == typeof(ApiVersionAttribute))
                .Select(z => z.ConstructorArguments)).ToList();

            foreach(var versionedApiController in versionedApiControllers)
            {
                var versions = versionedApiController.Select(x => x.FirstOrDefault().Value?.ToString()).ToList();
                versionList.AddRange(versions);
            }

            versionList = versionList.Distinct().OrderByDescending(x => x).ToList();
            return versionList;
        }

        public static List<string> GetApiVersionsForController(Type controllerType)
        {            
            var versionList = new List<string>();            
            var versionedApiController = controllerType.CustomAttributes?.Where(x => x.AttributeType == typeof(ApiVersionAttribute))
                .Select(z => z.ConstructorArguments).ToList();
            var versions = versionedApiController.Select(x => x.FirstOrDefault().Value?.ToString()).ToList();
            versionList.AddRange(versions);
            return versionList;
        }

        private static IEnumerable<Type> GetSubClasses<T>()
        {
            return Assembly.GetCallingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(T))).ToList();
        }
    }
}
