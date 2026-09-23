using System.Collections.Generic;

namespace CookieJarTools.CrumbBaker.Core
{
	public class PackageCheckResult
	{
		public bool IsSuccess { get; }
		public IReadOnlyList<PackageCheckIssue> Errors { get; }
		public IReadOnlyList<PackageCheckIssue> Warnings { get; }

		public PackageCheckResult(bool isSuccess, List<PackageCheckIssue> errors, List<PackageCheckIssue> warnings)
		{
			IsSuccess = isSuccess;
			Errors = errors;
			Warnings = warnings;
		}
	}
}