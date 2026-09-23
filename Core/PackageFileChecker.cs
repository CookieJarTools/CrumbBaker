using CookieJarTools.CrumbBaker.Core.Rules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CookieJarTools.CrumbBaker.Core
{
	public static class PackageFileChecker
	{
		public static PackageCheckResult CheckPackage(string packageRootPath, IReadOnlyList<FileRule> fileRules)
		{
			if (!Directory.Exists(packageRootPath))
			{
				throw new DirectoryNotFoundException($"Package root path does not exist: {packageRootPath}");
			}

			var allFiles = Directory
				.GetFiles(packageRootPath, "*", SearchOption.AllDirectories)
				.Select(path => Path.GetRelativePath(packageRootPath, path).Replace('\\', '/'))
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			var allDirectories = Directory
				.GetDirectories(packageRootPath, "*", SearchOption.AllDirectories)
				.Select(path => Path.GetRelativePath(packageRootPath, path).Replace('\\', '/'))
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			var errors = new List<PackageCheckIssue>();
			var warnings = new List<PackageCheckIssue>();

			foreach (var fileRule in fileRules)
			{
				var matchesFile = allFiles.Contains(fileRule.Pattern);
				var matchesDirectory = allDirectories.Contains(fileRule.Pattern);
				var exists = matchesFile || matchesDirectory;

				if (!exists)
				{
					var ruleRequirementLevel = fileRule.RequirementLevel;
					var message = $"Missing {ruleRequirementLevel} item '{fileRule.Pattern}' ({fileRule.Description}).";
					var issue = new PackageCheckIssue(message, ruleRequirementLevel);

					if (ruleRequirementLevel == FileRequirementLevel.Required)
					{
						errors.Add(issue);
					}
					else
					{
						warnings.Add(issue);
					}

					if (!fileRule.HasHandler)
					{
						continue;
					}

					if (!TryCreateHandler(fileRule, out var _, out var handlerWarningIssue))
					{
						if (handlerWarningIssue != null)
						{
							warnings.Add(handlerWarningIssue);
						}
					}

					continue;
				}

				if (!fileRule.HasHandler)
				{
					continue;
				}

				if (!TryCreateHandler(fileRule, out var fileRuleHandler, out var handlerIssue))
				{
					if (handlerIssue != null)
					{
						warnings.Add(handlerIssue);
					}

					continue;
				}

				if (handlerIssue != null)
				{
					warnings.Add(handlerIssue);
				}

				var context = new FileRuleContext(
					packageRootPath,
					allFiles,
					allDirectories,
					fileRule);

				fileRuleHandler.Evaluate(context, errors, warnings);
			}

			var isSuccess = errors.Count == 0;

			return new PackageCheckResult(isSuccess, errors, warnings);
		}

		private static bool TryCreateHandler(
			FileRule fileRule,
			out IFileRuleHandler fileRuleHandler,
			out PackageCheckIssue handlerWarningIssue)
		{
			fileRuleHandler = null;
			handlerWarningIssue = null;

			if (!fileRule.HasHandler)
			{
				return false;
			}

			var resolvedType = fileRule.HandlerType;

			if (resolvedType == null && !string.IsNullOrWhiteSpace(fileRule.HandlerTypeName))
			{
				resolvedType = Type.GetType(fileRule.HandlerTypeName, false);
				if (resolvedType == null)
				{
					var message = $"File rule handler type '{fileRule.HandlerTypeName}' could not be resolved.";
					handlerWarningIssue = new PackageCheckIssue(message, FileRequirementLevel.Recommended);
					return false;
				}
			}

			if (resolvedType == null)
			{
				return false;
			}

			if (!typeof(IFileRuleHandler).IsAssignableFrom(resolvedType))
			{
				var message = $"File rule handler type '{resolvedType.FullName}' does not implement IFileRuleHandler.";
				handlerWarningIssue = new PackageCheckIssue(message, FileRequirementLevel.Recommended);
				return false;
			}

			try
			{
				var instance = Activator.CreateInstance(resolvedType);
				fileRuleHandler = (IFileRuleHandler)instance;
				return true;
			}
			catch (Exception exception)
			{
				var message = $"Failed to create file rule handler '{resolvedType.FullName}': {exception.Message}";
				handlerWarningIssue = new PackageCheckIssue(message, FileRequirementLevel.Recommended);
				return false;
			}
		}
	}
}