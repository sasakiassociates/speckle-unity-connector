using System;
using Speckle.Core.Credentials;

namespace Speckle.ConnectorUnity.Ops
{

	[Serializable]
	public sealed class AccountWrapper : GenericWrapper<Account>
	{

		public string serverUrl, serverName, serverCompany;
		public string userId, userName, userEmail, userAvatar, userCompany;

		public string id, token, refreshToken;
		public bool isDefault;

		public AccountWrapper(Account value) : base(value)
		{
			if (value == null) return;

			id = value.id;
			isDefault = value.isDefault;
			token = value.token;
			refreshToken = value.refreshToken;

			serverUrl = value.serverInfo.url;
			serverName = value.serverInfo.name;
			serverCompany = value.serverInfo.company;

			userName = value.userInfo.name;
			userEmail = value.userInfo.email;
			userId = value.userInfo.id;
			userAvatar = value.userInfo.avatar;
			userCompany = value.userInfo.company;
		}

		protected override Account Get()
		{
			// TODO: handle other ways of looking for accounts
			return SpeckleUnity.GetAccountByUserInfo(userInfo);
		}

		UserInfo userInfo =>
			new UserInfo()
			{
				email = userEmail,
				avatar = userAvatar,
				id = userId,
				name = userName,
				company = userCompany
			};

		ServerInfo serverInfo =>
			new ServerInfo()
			{
				company = serverCompany, name = serverName, url = serverUrl
			};

	}
}