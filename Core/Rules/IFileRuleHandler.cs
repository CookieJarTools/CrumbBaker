using System.Collections.Generic;

namespace CookieJarTools.CrumbBaker.Core.Rules
{
	public interface IFileRuleHandler
	{
		void Evaluate(FileRuleContext context, List<PackageCheckIssue> errors, List<PackageCheckIssue> warnings);
	}
}