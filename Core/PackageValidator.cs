using CookieJarTools.CrumbBaker.Core.Config;
using CookieJarTools.CrumbBaker.Core.Rules;
using System;
using System.Collections.Generic;

namespace CookieJarTools.CrumbBaker.Core
{
	public static class PackageValidator
	{
		private static readonly FileRule[] defaultFileRules = new[]
		{
			new FileRule("package.json", FileRequirementLevel.Required, "Package manifest"),
			new FileRule("README.md", FileRequirementLevel.Recommended, "Documentation")
		};
		
		public static PackageCheckResult ValidatePackage(string packagePath, FileRule[] fileRules)
		{
			var fileRulesToCheck = new List<FileRule>(
				fileRules.Length +
				defaultFileRules.Length);

			if (fileRules.Length > 0)
			{
				fileRulesToCheck.AddRange(fileRules);
			}
			fileRulesToCheck.AddRange(defaultFileRules);
			
			var result = PackageFileChecker.CheckPackage(packagePath, fileRulesToCheck);

			return result;
		}
		
		public static PackageCheckResult ValidatePackage(string packagePath, string packageConfigPath)
		{
			var rules = RuleConfigLoader.LoadRulesFromConfig(packageConfigPath);
			
			var fileRulesToCheck = new List<FileRule>(
				rules.Count +
				defaultFileRules.Length);

			if (rules.Count > 0)
			{
				fileRulesToCheck.AddRange(rules);
			}
			fileRulesToCheck.AddRange(defaultFileRules);
			
			var result = PackageFileChecker.CheckPackage(packagePath, fileRulesToCheck);

			return result;
		}
	}
}