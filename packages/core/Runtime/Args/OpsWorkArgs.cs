namespace Speckle.ConnectorUnity.Args
{
	public abstract class OpsWorkArgs
	{
		public string Message { get; set; }

		public bool Success { get; set; }

	}

	public class SendWorkArgs : OpsWorkArgs
	{

		public string CommitId { get; set; }

		public string URL { get; set; }

	}

	public class ReceiveWorkArgs : OpsWorkArgs
	{
		public string ReferenceObj { get; set; }
	}
}