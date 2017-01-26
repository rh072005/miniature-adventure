using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
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
            var versionAttributes = controllers.Select(x => x.GetTypeInfo().GetCustomAttributes<ApiVersionAttribute>());
            versionList = versionAttributes.SelectMany(x => x.Select(y => y.Versions.FirstOrDefault().ToString())).ToList();
            versionList = versionList.Distinct().OrderByDescending(x => x).ToList();
            return versionList;
        }

        public static List<string> GetApiVersionsForController(Type controllerType)
        {
            var versionList = new List<string>();
            var versionAttributes = controllerType.GetTypeInfo().GetCustomAttributes<ApiVersionAttribute>();
            versionList = versionAttributes.Select(x => x.Versions.FirstOrDefault().ToString()).ToList();
            return versionList;
        }

        private static IEnumerable<Type> GetSubClasses<T>()
        {
            return Assembly.GetCallingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(T))).ToList();
        }
    }
}
