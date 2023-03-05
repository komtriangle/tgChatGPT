using Microsoft.Extensions.Configuration;
using TelegramChatGPT;
using TeleSharp.TL.Messages;
using TLSharp.Core;

namespace TelegramBotExample
{
	class Program
	{

		private static HttpClient _httpClient { get; set; }
		static async Task Main(string[] args)
		{

			AppSettings appSettings = GetConfig();

			_httpClient = new HttpClient();

			TelegramClient client = new TelegramClient(appSettings.AppId, appSettings.ApiHash);

			await AuthAsync(appSettings, client);

			var contacts = await client.GetContactsAsync();

			long targetUserAccessHash = -6362048485537347209;
			int targetUserId = 745762860;

			var inputUser = new TLSchema.TLInputUser { UserId = targetUserId, AccessHash = targetUserAccessHash };
			var peer = new TeleSharp.TL.TLInputPeerUser { UserId = targetUserId, AccessHash = targetUserAccessHash };

			while(true)
			{
				try
				{
					TLDialogsSlice? dialogs = (TLDialogsSlice)await client.GetUserDialogsAsync();

					List<TeleSharp.TL.TLAbsMessage>? messages = dialogs.Messages
						.Select(x => x)
						.ToList();

					TeleSharp.TL.TLMessage? message = messages.OfType<TeleSharp.TL.TLMessage>()
						.FirstOrDefault(x => x.FromId == targetUserId);

					DateTime messageDate = new DateTime(message.Date);

					if(messageDate > DateTime.UtcNow.AddSeconds(-appSettings.MessagesDelay.TotalSeconds))
					{
						string chatGPTResponse = await GetChatGPTAnswer(message.Message, appSettings);
						await client.SendMessageAsync(peer, chatGPTResponse);
					}

					
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message);
				}

				await Task.Delay(TimeSpan.FromSeconds(appSettings.MessagesDelay.TotalSeconds));

			}
		}

		static async Task<string> GetChatGPTAnswer(string message, AppSettings appSettings)
		{

			var response = await _httpClient.SendAsync(new HttpRequestMessage()
			{
				RequestUri = new Uri($"{appSettings.ChatCPTApiUrl}/ask?qustion={message}")
			});

			string textResponse = await response.Content.ReadAsStringAsync();

			textResponse = textResponse.Replace("\\n", "");
			textResponse = textResponse.Replace("\\r", "");

			textResponse = textResponse.Trim('"');

			return textResponse;
		}

		static AppSettings GetConfig()
		{
			var builder = new ConfigurationBuilder()
			   .SetBasePath(Directory.GetCurrentDirectory())
			   .AddJsonFile("AppSettings.json", optional: false);

			IConfiguration config = builder.Build();

			return config.GetSection("BotSettings").Get<AppSettings>();
		}

		static async Task AuthAsync(AppSettings appSettings, TelegramClient client)
		{
			await client.ConnectAsync();
			string hash;

			try
			{
				hash = await client.SendCodeRequestAsync(appSettings.PhoneNumber);
			}
			catch(Exception ex)
			{
				hash = await client.SendCodeRequestAsync(appSettings.PhoneNumber);

			}

			Console.WriteLine("Enter the code:");
			var code = Console.ReadLine();

			var user = await client.MakeAuthAsync(appSettings.PhoneNumber, hash, code);
		}
	}


}