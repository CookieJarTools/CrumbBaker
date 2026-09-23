using CookieJarTools.CrumbBaker.Core.Rules;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace CookieJarTools.CrumbBaker.Core.RuleHandlers
{
	public sealed class PackageJsonRuleHandler : IFileRuleHandler
	{
		public void Evaluate(
			FileRuleContext context,
			List<PackageCheckIssue> errors,
			List<PackageCheckIssue> warnings)
		{
			var packageJsonPath = Path.Combine(context.PackageRootPath, context.Rule.Pattern);
			if (!File.Exists(packageJsonPath))
			{
				return;
			}

			var jsonText = File.ReadAllText(packageJsonPath);
			var jsonObject = JObject.Parse(jsonText);

			if (jsonObject["name"] == null)
			{
				errors.Add(new PackageCheckIssue(
					"package.json is missing required 'name' field.",
					FileRequirementLevel.Required));
			}

			if (jsonObject["version"] == null)
			{
				errors.Add(new PackageCheckIssue(
					"package.json is missing required 'version' field.",
					FileRequirementLevel.Required));
			}
		}
	}
}