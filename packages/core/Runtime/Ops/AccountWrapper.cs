using System;
using Speckle.Core.Credentials;

namespace Speckle.ConnectorUnity.Ops
{
	public abstract class GenericWrapper<TObj>
	{
		protected TObj _source;

		public virtual TObj source
		{
			get => _source ?? Get();
			set
			{
				if (value != null) Set(value);
			}
		}

		public abstract void Set(TObj value);

		public abstract TObj Get();

	}

	[Serializable]
	public sealed class AccountWrapper : GenericWrapper<Account>
	{

		public string server_url, server_name, server_company;
		public string user_id, user_name, user_email, user_avatar, user_company;

		public string id, token, refreshToken;
		public bool isDefault;

		public override void Set(Account value)
		{
			id = value.id;
			isDefault = value.isDefault;
			token = value.token;
			refreshToken = value.refreshToken;

			server_url = value.serverInfo.url;
			server_name = value.serverInfo.name;
			server_company = value.serverInfo.company;

			user_name = value.userInfo.name;
			user_email = value.userInfo.email;
			user_id = value.userInfo.id;
			user_avatar = value.userInfo.avatar;
			user_company = value.userInfo.company;
		}

		public override Account Get()
		{
			_source = SpeckleUnity.GetAccountByUserInfo(userInfo);
			if (source != null)
				return _source;

			return null;
		}

		UserInfo userInfo =>
			new UserInfo()
			{
				email = user_email,
				avatar = user_avatar,
				id = user_id,
				name = user_name,
				company = user_company
			};

		void Clear()
		{
			user_id = string.Empty;
			user_name = string.Empty;
			user_email = string.Empty;

			server_url = string.Empty;
			server_company = string.Empty;
			server_name = string.Empty;
		}

	}
}