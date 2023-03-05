namespace TelegramChatGPT
{
	internal class AppSettings
	{
		public int AppId { get; set; }
		public string ApiHash { get; set; }
		public string PhoneNumber { get; set; }
		public TimeSpan MessagesDelay { get; set; }
		public string ChatCPTApiUrl { get; set; }
	}
}
