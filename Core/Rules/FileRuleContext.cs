using System.Collections.Generic;

namespace CookieJarTools.CrumbBaker.Core.Rules
{
	public sealed class FileRuleContext
	{
		public string PackageRootPath { get; }
		public IReadOnlyCollection<string> AllFiles { get; }
		public IReadOnlyCollection<string> AllDirectories { get; }
		public FileRule Rule { get; }

		public FileRuleContext(
			string packageRootPath,
			IReadOnlyCollection<string> allFiles,
			IReadOnlyCollection<string> allDirectories,
			FileRule rule)
		{
			PackageRootPath = packageRootPath;
			AllFiles = allFiles;
			AllDirectories = allDirectories;
			Rule = rule;
		}
	}
}