using CookieJarTools.CrumbBaker.Core.Rules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CookieJarTools.CrumbBaker.Core.Config
{
	public sealed class FileRuleConfig
	{
		public string Pattern { get; set; }
		public string Requirement { get; set; }
		public string Description { get; set; }
		public string HandlerTypeName { get; set; }
	}

	public sealed class CrumbBakerConfig
	{
		public List<FileRuleConfig> Rules { get; set; } = new List<FileRuleConfig>();
	}

	public static class RuleConfigLoader
	{
		public static IReadOnlyList<FileRule> LoadRulesFromConfig(string configPath)
		{
			if (!File.Exists(configPath))
			{
				throw new FileNotFoundException("CrumbBaker config file not found.", configPath);
			}

			var jsonText = File.ReadAllText(configPath);

			var config = JsonConvert.DeserializeObject<CrumbBakerConfig>(jsonText);
			if (config == null || config.Rules == null || config.Rules.Count == 0)
			{
				throw new InvalidOperationException("CrumbBaker config contains no rules.");
			}

			var fileRules = new List<FileRule>();

			foreach (var ruleConfig in config.Rules)
			{
				if (string.IsNullOrWhiteSpace(ruleConfig.Pattern))
				{
					continue;
				}

				if (!Enum.TryParse<FileRequirementLevel>(ruleConfig.Requirement, true, out var requirementLevel))
				{
					requirementLevel = FileRequirementLevel.Required;
				}

				var description = ruleConfig.Description ?? string.Empty;

				var fileRule = new FileRule(
					ruleConfig.Pattern,
					requirementLevel,
					description,
					ruleConfig.HandlerTypeName);

				fileRules.Add(fileRule);
			}

			return fileRules;
		}
	}
}