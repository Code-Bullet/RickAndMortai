﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace OpenAI_API.Chat
{
	/// <summary>
	/// Represents the Role of a <see cref="ChatMessage"/>.  Typically, a conversation is formatted with a system message first, followed by alternating user and assistant messages.  See <see href="https://platform.openai.com/docs/guides/chat/introduction">the OpenAI docs</see> for more details about usage.
	/// </summary>
	public class ChatMessageRole : IEquatable<ChatMessageRole>
	{
		/// <summary>
		/// Contructor is private to force usage of strongly typed values
		/// </summary>
		/// <param name="value"></param>
		private ChatMessageRole(string value) { Value = value; }

		/// <summary>
		/// Gets the singleton instance of <see cref="ChatMessageRole"/> based on the string value.
		/// </summary>
		/// <param name="roleName">Muse be one of "system", "user", or "assistant"</param>
		/// <returns></returns>
		public static ChatMessageRole FromString(string roleName)
		{
			switch (roleName)
			{
				case "system":
					return System;
				case "user":
					return User;
				case "assistant":
					return Assistant;
				default:
					return null;
			}
		}

		private string Value { get; }

		/// <summary>
		/// The system message helps set the behavior of the assistant. 
		/// </summary>
		public static ChatMessageRole System { get; } = new ChatMessageRole("system");
		/// <summary>
		/// The user messages help instruct the assistant. They can be generated by the end users of an application, or set by a developer as an instruction.
		/// </summary>
		public static ChatMessageRole User { get; } = new ChatMessageRole("user");
		/// <summary>
		/// The assistant messages help store prior responses. They can also be written by a developer to help give examples of desired behavior.
		/// </summary>
		public static ChatMessageRole Assistant { get; } = new ChatMessageRole("assistant");

		/// <summary>
		/// Gets the string value for this role to pass to the API
		/// </summary>
		/// <returns>The size as a string</returns>
		public override string ToString()
		{
			return Value;
		}

		/// <summary>
		/// Determines whether this instance and a specified object have the same value.
		/// </summary>
		/// <param name="obj">The ChatMessageRole to compare to this instance</param>
		/// <returns>true if obj is a ChatMessageRole and its value is the same as this instance; otherwise, false. If obj is null, the method returns false</returns>
		public override bool Equals(object obj)
		{
			return Value.Equals((obj as ChatMessageRole).Value);
		}

		/// <summary>
		/// Returns the hash code for this object
		/// </summary>
		/// <returns>A 32-bit signed integer hash code</returns>
		public override int GetHashCode()
		{
			return Value.GetHashCode();	
		}

		/// <summary>
		/// Determines whether this instance and a specified object have the same value.
		/// </summary>
		/// <param name="other">The ChatMessageRole to compare to this instance</param>
		/// <returns>true if other's value is the same as this instance; otherwise, false. If other is null, the method returns false</returns>
		public bool Equals(ChatMessageRole other)
		{
			return Value.Equals(other.Value);
		}

		/// <summary>
		/// Gets the string value for this role to pass to the API
		/// </summary>
		/// <param name="value">The ChatMessageRole to convert</param>
		public static implicit operator String(ChatMessageRole value) { return value; }

		///// <summary>
		///// Used during the Json serialization process
		///// </summary>
		//internal class ChatMessageRoleJsonConverter : JsonConverter<ChatMessageRole>
		//{
		//	public override void WriteJson(JsonWriter writer, ChatMessageRole value, JsonSerializer serializer)
		//	{
		//		writer.WriteValue(value.ToString());
		//	}

		//	public override ChatMessageRole ReadJson(JsonReader reader, Type objectType, ChatMessageRole existingValue, bool hasExistingValue, JsonSerializer serializer)
		//	{
		//		if (reader.TokenType != JsonToken.String)
		//		{
		//			throw new JsonSerializationException();
		//		}
		//		return new ChatMessageRole(reader.ReadAsString());
		//	}
		//}
	}
}
