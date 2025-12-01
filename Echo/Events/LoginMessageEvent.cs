using Prism.Events;

namespace Echo.Events
{
	public enum LoginMessageType
	{
		Login,
		Register
	}

	public class LoginMessage
	{
		public LoginMessageType MessageType { get; set; }
		public bool Status { get; set; }
		public string Msg { get; set; }
	}

	public class LoginMessageEvent : PubSubEvent<LoginMessage>
	{
	}
}
