namespace Speckle.ConnectorUnity.Args
{
	public abstract class SpeckleUnityArgs
	{ }

	public abstract class ClientArgs : SpeckleUnityArgs
	{
		public string message { get; set; }

	}

	public class ClientWorkArgs : ClientArgs
	{
		public bool success { get; set; }

		public object client { get; set; }

	}

	public class ReceiveWorkArgs : ClientWorkArgs
	{
		public string referenceObj { get; set; }
	}

	public class SendWorkArgs : ClientWorkArgs
	{

		public string commitId { get; set; }
		public string url { get; set; }

	}

}