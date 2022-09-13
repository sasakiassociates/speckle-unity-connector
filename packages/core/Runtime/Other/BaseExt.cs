using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Speckle.ConnectorUnity.Models
{
	public static class BaseExt
	{
		
		public static bool IsPropNameValid(this IBase obj, string name, out string reason)
		{ 
			// Regex rules
			// Rule for multiple leading @.
			var manyLeadingAtChars = new Regex(@"^@{2,}");
			// Rule for invalid chars.
			var invalidChars = new Regex(@"[\.\/]");

			// TODO: Check for detached/non-detached duplicate names? i.e: '@something' vs 'something'
			// TODO: Instance members will not be overwritten, this may cause issues.
			var checks = new List<(bool, string)>
			{
				(!(string.IsNullOrEmpty(name) || name == "@"), "Found empty prop name"),
				// Checks for multiple leading @
				(!manyLeadingAtChars.IsMatch(name), "Only one leading '@' char is allowed. This signals the property value should be detached."),
				// Checks for invalid chars
				(!invalidChars.IsMatch(name), $"Prop with name '{name}' contains invalid characters. The following characters are not allowed: ./"),
				// Checks if you are trying to change a member property
				//(!members.Contains(name), "Modifying the value of instance member properties is not allowed.")
			};

			var r = "";
			// Prop name is valid if none of the checks are true
			var isValid = checks.TrueForAll(v =>
			{
				if (!v.Item1) r = v.Item2;
				return v.Item1;
			});

			reason = r;
			return isValid;
		}


	}
}