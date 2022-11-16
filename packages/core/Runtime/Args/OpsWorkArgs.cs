namespace Speckle.ConnectorUnity.Args
{
	public abstract class OpsWorkArgs
	{
		public string message { get; set; }

		public bool success { get; set; }

	}

	public class SendWorkArgs : OpsWorkArgs
	{

		public string commitId { get; set; }

		public string url { get; set; }

	}

	public class ReceiveWorkArgs : OpsWorkArgs
	{
		public string referenceObj { get; set; }
	}
}