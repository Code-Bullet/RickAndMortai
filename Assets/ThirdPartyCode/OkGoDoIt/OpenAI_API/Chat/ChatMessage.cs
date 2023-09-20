using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAI_API.Chat
{
	/// <summary>
	/// Chat message sent or received from the API. Includes who is speaking in the "role" and the message text in the "content"
	/// </summary>
	public class ChatMessage
	{
		/// <summary>
		/// Creates an empty <see cref="ChatMessage"/>, with <see cref="Role"/> defaulting to <see cref="ChatMessageRole.User"/>
		/// </summary>
		public ChatMessage()
		{
			this.Role = ChatMessageRole.User;
		}

		/// <summary>
		/// Constructor for a new Chat Message
		/// </summary>
		/// <param name="role">The role of the message, which can be "system", "assistant" or "user"</param>
		/// <param name="content">The text to send in the message</param>
		public ChatMessage(ChatMessageRole role, string content)
		{
			this.Role = role;
			this.Content = content;
		}

		[JsonProperty("role")]
		internal string rawRole { get; set; }

		/// <summary>
		/// The role of the message, which can be "system", "assistant" or "user"
		/// </summary>
		[JsonIgnore]
		public ChatMessageRole Role
		{
			get
			{
				return ChatMessageRole.FromString(rawRole);
			}
			set
			{
				rawRole = value.ToString();
			}
		}

		/// <summary>
		/// The content of the message
		/// </summary>
		[JsonProperty("content")]
		public string Content { get; set; }

		/// <summary>
		/// The content of the message
		/// </summary>
		[JsonProperty("function_call")]
		public object Function_Call { get; set; }
	}


	public class ChatMessageFunctionCall
	{
		/// <summary>
		/// Creates an empty <see cref="ChatMessage"/>, with <see cref="Role"/> defaulting to <see cref="ChatMessageRole.User"/>
		/// </summary>
		public ChatMessageFunctionCall()
		{
			
		}


		[JsonProperty("name")]
		internal string Name { get; set; }


		/// <summary>
		/// The content of the message
		/// </summary>
		[JsonProperty("arguments")]
		public object Arguments { get; set; }


	}




}
