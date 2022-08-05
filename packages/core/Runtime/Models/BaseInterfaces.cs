using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Speckle.Core.Logging;
using Speckle.Core.Models;

namespace Speckle.ConnectorUnity.Models
{
	public interface IBase : IBaseDynamic
	{

		public UniTask Store(Base @base);

		public string id { get; }

		public string speckle_type { get; }

		public string applicationId { get; }

		public long totalChildCount { get; }

	}
	public interface IBaseDynamic
	{
		public HashSet<string> excluded { get; }

		SpeckleProperties props { get; }

		#region copy pasta from speckle core models

		public object this[string key]
		{
			get
			{
				if (props.Data.ContainsKey(key))
					return props.Data[key];

				var prop = GetType().GetProperty(key);

				return prop == null ? null : prop.GetValue(this);
			}
			set
			{
				if (!this.IsPropNameValid(key, out string reason)) throw new SpeckleException("Invalid prop name: " + reason);

				if (props.Data.ContainsKey(key))
				{
					props.Data[key] = value;
					return;
				}

				// TODO: this probably wont work
				var prop = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(p => p.Name == key);

				if (prop == null)
				{
					props.Data[key] = value;
					return;
				}

				try
				{
					prop.SetValue(this, value);
				}
				catch (Exception ex)
				{
					SpeckleUnity.Console.Error(ex.Message);
				}
			}
		}

		public bool IsPropNameValid(string name, out string reason)
		{
			// Regex rules
			// Rule for multiple leading @.
			var manyLeadingAtChars = new Regex(@"^@{2,}");
			// Rule for invalid chars.
			var invalidChars = new Regex(@"[\.\/]");
			// Existing members
			var members = GetInstanceMembersNames();

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

		/// <summary>
		/// Gets the names of the defined class properties (typed).
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetInstanceMembersNames()
		{
			var names = new List<string>();
			var pinfos = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (var pinfo in pinfos) names.Add(pinfo.Name);

			names.Remove("Item"); // TODO: investigate why we get Item out?
			return names;
		}

		#endregion

	}
}