namespace Speckle.ConnectorUnity.Args
{
	public abstract class ClientWorkArgs
	{
		public string message { get; set; }

		public bool success { get; set; }

		public object client { get; set; }

	}

	public class SendWorkArgs : ClientWorkArgs
	{

		public string commitId { get; set; }

		public string url { get; set; }

	}

	public class ReceiveWorkArgs : ClientWorkArgs
	{
		public string referenceObj { get; set; }
	}
}