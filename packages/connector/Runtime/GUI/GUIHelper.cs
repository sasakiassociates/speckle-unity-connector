using System.Collections.Generic;
using System.Linq;
using Speckle.ConnectorUnity.Converter;
using Speckle.Core.Api;
using Speckle.Core.Credentials;

namespace Speckle.ConnectorUnity.GUI
{
	public static class GUIHelper
	{

		const char SEP = ':';

		const string DEFAULT = "empty";

		public static string Dir
		{
			get => SpeckleConnector.PackagePath + "Runtime/GUI/";
		}

		#region Converters
		public static IEnumerable<string> Format(this IEnumerable<ScriptableSpeckleConverter> items)
		{
			return items != null ? items.Select(x => x.Name) : new[] { DEFAULT };
		}
		#endregion

		#region Account
		public static string Format(this Account item) => item != null ? item.userInfo.email + SEP + item.serverInfo.name : string.Empty;

		public static string ParseAccountEmail(this string value) => value.Valid() ? value.Split(SEP).FirstOrDefault() : null;

		public static string ParseAccountServer(this string value) => value.Valid() ? value.Split(SEP).Last() : null;

		public static IEnumerable<string> Format(this IEnumerable<Account> items)
		{
			return items != null ? items.Select(x => x.Format()).ToArray() : new[] { DEFAULT };
		}
		#endregion

		#region Stream
		public static IEnumerable<string> Format(this IEnumerable<Stream> items)
		{
			return items != null ? items.Select(x => x.Format()).ToArray() : new[] { DEFAULT };
		}

		public static string Format(this Stream item) => item != null ? item.name + SEP + item.id : string.Empty;

		public static string ParseStreamName(this string value) => value.Valid() ? value.Split(SEP).FirstOrDefault() : null;

		public static string ParseStreamId(this string value) => value.Valid() ? value.Split(SEP).Last() : null;
		#endregion

		#region Branch
		public static string Format(this Branch item) => item != null ? item.name : string.Empty;

		public static IEnumerable<string> Format(this IEnumerable<Branch> items)
		{
			return items != null ? items.Select(x => x.Format()).ToArray() : new[] { DEFAULT };
		}
		#endregion

		#region Commit
		public static IEnumerable<string> Format(this IEnumerable<Commit> items)
		{
			return items != null ? items.Select(x => x.Format()).ToArray() : new[] { DEFAULT };
		}

		public static string Format(this Commit item) => item != null ? item.id + SEP + item.message : string.Empty;

		public static string ParseCommitId(this string value) => value.Valid() ? value.Split(SEP).FirstOrDefault() : null;

		public static string ParseCommitMsg(this string value) => value.Valid() ? value.Split(SEP).Last() : null;
		#endregion

	}
}