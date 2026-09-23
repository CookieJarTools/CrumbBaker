using CookieJarTools.CrumbBaker.Core.Rules;

namespace CookieJarTools.CrumbBaker.Core
{
	public class PackageCheckIssue
	{
		public string Message { get; }
		public FileRequirementLevel RequirementLevel { get; }

		public PackageCheckIssue(string message, FileRequirementLevel requirementLevel)
		{
			Message = message;
			RequirementLevel = requirementLevel;
		}
		
		public override string ToString()
		{
			return $"{RequirementLevel}: {Message}";
		}
	}
}