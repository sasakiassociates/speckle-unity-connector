using System;
using Speckle.Core.Credentials;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public sealed class AccountAdapter : GenericAdapter<Account>
	{
		[SerializeField] string _serverUrl;
		[SerializeField] string _serverName;
		[SerializeField] string _serverCompany;
		[SerializeField] string _userId;
		[SerializeField] string _userName;
		[SerializeField] string _userEmail;
		[SerializeField] string _userAvatar;
		[SerializeField] string _userCompany;

		string _id;
		string _token;
		string _refreshToken;
		bool _isDefault;

		public string id => _id;

		public string token => _token;

		public string refreshToken => _refreshToken;

		public bool isDefault => _isDefault;

		public AccountAdapter(Account value) : base(value)
		{
			if (value == null) return;

			_id = value.id;
			_isDefault = value.isDefault;
			_token = value.token;
			_refreshToken = value.refreshToken;

			_serverUrl = value.serverInfo.url;
			_serverName = value.serverInfo.name;
			_serverCompany = value.serverInfo.company;

			_userName = value.userInfo.name;
			_userEmail = value.userInfo.email;
			_userId = value.userInfo.id;
			_userAvatar = value.userInfo.avatar;
			_userCompany = value.userInfo.company;
		}

		protected override Account Get()
		{
			// NOTE: don't load the account directly from the serialized data. 
			// NOTE: use the manager to load in the account properly 

			return SpeckleUnity.GetAccountByUserInfo(userInfo);
			
			// TODO: handle other ways of looking for accounts
		}

		public UserInfo userInfo =>
			new UserInfo()
			{
				email = _userEmail,
				avatar = _userAvatar,
				id = _userId,
				name = _userName,
				company = _userCompany
			};

		public ServerInfo serverInfo =>
			new ServerInfo()
			{
				company = _serverCompany,
				name = _serverName,
				url = _serverUrl
			};

	}
}