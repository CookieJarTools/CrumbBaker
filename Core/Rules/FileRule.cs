using System;

namespace CookieJarTools.CrumbBaker.Core.Rules
{
	public sealed class FileRule
	{
		public string Pattern { get; }
		public FileRequirementLevel RequirementLevel { get; }
		public string Description { get; }

		public Type HandlerType { get; }
		public string HandlerTypeName { get; }

		public bool HasHandler => HandlerType != null || !string.IsNullOrWhiteSpace(HandlerTypeName);

		public FileRule(string pattern, FileRequirementLevel requirementLevel, string description)
			: this(pattern, requirementLevel, description, null, null)
		{
		}

		public FileRule(string pattern, FileRequirementLevel requirementLevel, string description, Type handlerType)
			: this(pattern, requirementLevel, description, handlerType, null)
		{
		}

		public FileRule(string pattern, FileRequirementLevel requirementLevel, string description, string handlerTypeName)
			: this(pattern, requirementLevel, description, null, handlerTypeName)
		{
		}

		public FileRule(
			string pattern,
			FileRequirementLevel requirementLevel,
			string description,
			Type handlerType,
			string handlerTypeName)
		{
			Pattern = pattern;
			RequirementLevel = requirementLevel;
			Description = description;
			HandlerType = handlerType;
			HandlerTypeName = handlerTypeName;
		}
	}
}