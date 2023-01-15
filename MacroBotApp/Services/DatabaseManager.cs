﻿using System.Data.SQLite;
using MacroBot.Models;

namespace MacroBot.Services;

public static class DatabaseManager {
	private static SQLiteConnection database;

	public static async Task Initialize (string filepath) {
		database = new SQLiteConnection(new SQLiteConnectionStringBuilder() {
			DataSource = filepath,
			DateTimeFormat = SQLiteDateFormats.UnixEpoch,
			DateTimeKind = DateTimeKind.Utc,
			DefaultDbType = System.Data.DbType.Object,
			FailIfMissing = false,
			ForeignKeys = true,
			SyncMode = SynchronizationModes.Full,
		}.ToString());

		try {
			await database.OpenAsync();
			using (var command = database.CreateCommand()) {
				command.CommandText = "CREATE TABLE IF NOT EXISTS 'Tickets' ('Author' INTEGER, 'Channel' INTEGER UNIQUE, 'Message' INTEGER UNIQUE, 'Created' INTEGER, 'Modified' INTEGER, PRIMARY KEY('Author'));";
				await command.ExecuteNonQueryAsync();
			}

			using (var command = database.CreateCommand())
			{
				command.CommandText = "CREATE TABLE IF NOT EXISTS 'Tags' ('Name' VARCHAR, 'Content' VARCHAR, 'Author' INTEGER, 'Guild' INTEGER, 'LastEditTimestamp' INTEGER, PRIMARY KEY('Name'));";
				await command.ExecuteNonQueryAsync();
			}

			using (var command = database.CreateCommand()) {
				command.CommandText = "CREATE TABLE IF NOT EXISTS 'Devs' ('ID' INTEGER, 'DiscordName' TEXT NULL, 'DevName' TEXT, PRIMARY KEY('ID'));";
				await command.ExecuteNonQueryAsync();
			}
			using (var command = database.CreateCommand()) {
				command.CommandText = "CREATE TABLE IF NOT EXISTS 'Plugins' ('ID' INTEGER AUTO_INCREMENT, 'Name' TEXT UNIQUE, 'Author' INTEGER, PRIMARY KEY('ID'), FOREIGN KEY ('Author') REFERENCES 'Devs'('ID') ON DELETE SET NULL);";
				await command.ExecuteNonQueryAsync();
			}
			using (var command = database.CreateCommand())
			{
				command.CommandText = "CREATE TABLE IF NOT EXISTS 'Plugins' ('BanID' INTEGER AUTO_INCREMENT, 'Name' TEXT UNIQUE, 'Author' INTEGER, PRIMARY KEY('ID'), FOREIGN KEY ('Author') REFERENCES 'Devs'('ID') ON DELETE SET NULL);";
				await command.ExecuteNonQueryAsync();
			}
		}
		catch (SQLiteException ex) { }
		catch (Exception ex) { }
		finally {
			await database.CloseAsync();
		}
	}

	public static async Task<bool> TicketExists (ulong author) {
		var exists = false;
		try {
			await DatabaseManager.database.OpenAsync();
			using var cmd = database.CreateCommand();
			cmd.CommandText = $"SELECT * FROM 'Tickets' WHERE 'Tickets'.'Author' == {author}";
			exists = await cmd.ExecuteScalarAsync() != null;
		}
		catch (SQLiteException ex) { }
		catch (Exception ex) { }
		finally {
			await database.CloseAsync();
		}
		return exists;
	}

	public static async Task CreateTicket (ulong author, ulong channel, ulong message, DateTimeOffset created) {
		try {
			await DatabaseManager.database.OpenAsync();
			using var command = database.CreateCommand();
			command.CommandText = $"INSERT INTO 'Tickets' VALUES ({author}, {channel}, {message}, {created.ToUnixTimeSeconds()}, {created.ToUnixTimeSeconds()});";
			await command.ExecuteNonQueryAsync();
		}
		catch (SQLiteException ex) { }
		catch (Exception ex) { }
		finally {
			await database.CloseAsync();
		}
	}

	public static async Task CreateTicket (ulong author, ulong channel, ulong message, DateTime created) {
		await DatabaseManager.CreateTicket(author, channel, message, (DateTimeOffset)created.ToUniversalTime());
	}

	public static async Task CreateTicket (ulong author, ulong channel, ulong message) {
		await DatabaseManager.CreateTicket(author, channel, message, DateTimeOffset.Now);
	}

	public static async Task DeleteTicket (ulong id, IdType type) {
		if (type is not IdType.Channel and not IdType.User) return;

		try {
			await DatabaseManager.database.OpenAsync();
			using var command = database.CreateCommand();
			command.CommandText = $"DELETE FROM 'Tickets' WHERE 'Tickets'.'{(type is IdType.User ? "Author" : "Channel")}' == {id};";
			await command.ExecuteNonQueryAsync();
		}
		catch (SQLiteException ex) { }
		catch (Exception ex) { }
		finally {
			await database.CloseAsync();
		}
	}

	public static async Task UpdateTicket (ulong author) {
		try {
			await DatabaseManager.database.OpenAsync();
			using var command = database.CreateCommand();
			command.CommandText = $"UPDATE 'Tickets' SET 'Modified' = {DateTimeOffset.Now.ToUnixTimeSeconds()} WHERE 'Tickets'.'Author' == {author};";
			await command.ExecuteNonQueryAsync();
		}
		catch (SQLiteException ex) { }
		catch (Exception ex) { }
		finally {
			await database.CloseAsync();
		}
	}

	public static async Task<List<Ticket>> GetTickets () {
		List<Ticket> tickets = new();
		try {
			await DatabaseManager.database.OpenAsync();
			using var command = database.CreateCommand();
			command.CommandText = $"SELECT * FROM 'Tickets'";
			var reader = await command.ExecuteReaderAsync();
			while (reader.Read()) {
				tickets.Add(new Ticket() {
					Author = (ulong)reader.GetInt64(0),
					Channel = (ulong)reader.GetInt64(1),
					Message = (ulong)reader.GetInt64(2),
					Created = DateTimeOffset.FromUnixTimeSeconds((long)reader.GetValue(3)).DateTime,
					Modified = DateTimeOffset.FromUnixTimeSeconds((long)reader.GetValue(4)).DateTime,
				});
			}
		}
		catch (SQLiteException ex) { }
		catch (Exception ex) { }
		finally {
			await database.CloseAsync();
		}

		return tickets;
	}

	public static async Task<Ticket> GetTicket (ulong author) {
		foreach (var ticket in await DatabaseManager.GetTickets()) {
			if (ticket.Author == author) return ticket;
		}
		return null;
	}

	public static async Task<bool> TagExists(string name,ulong guildId)
	{
		var exists = false;
		try
		{
			await DatabaseManager.database.OpenAsync();
			using var cmd = database.CreateCommand();
			cmd.CommandText = $"SELECT * FROM 'Tags' WHERE 'Tags'.'Name' == '{name}' AND 'Tags'.'Guild' == {guildId}";
			exists = await cmd.ExecuteScalarAsync() != null;
		}
		catch (SQLiteException ex) { }
		catch (Exception ex) { }
		finally
		{
			await database.CloseAsync();
		}
		return exists;
	}

	public static async Task CreateTag(string name, string content, ulong author, ulong guildId)
	{
		try
		{
			await DatabaseManager.database.OpenAsync();
			using var command = database.CreateCommand();
			command.CommandText = $"INSERT INTO 'Tags' VALUES ('{name}', '{content}', {author}, {guildId},{DateTimeOffset.Now.ToUnixTimeSeconds()});";
			await command.ExecuteNonQueryAsync();
		}
		catch (SQLiteException ex) { }
		catch (Exception ex) { }
		finally
		{
			await database.CloseAsync();
		}
	}

	public static async Task UpdateTag(string name, string content, ulong guildId, ulong editor)
	{
		try
		{
			await DatabaseManager.database.OpenAsync();
			using var command = database.CreateCommand();
			command.CommandText = $"UPDATE 'Tags' SET 'Content' = '{content}', 'LastEditTimestamp' = {DateTimeOffset.Now.ToUnixTimeSeconds()} WHERE 'Tags'.'Name' == '{name}' AND 'Tags'.'Guild' == {guildId}";
			await command.ExecuteNonQueryAsync();
		}
		catch (SQLiteException ex) { }
		catch (Exception ex) { }
		finally
		{
			await database.CloseAsync();
		}
	}

	public static async Task DeleteTag(string name)
	{
		try
		{
			await DatabaseManager.database.OpenAsync();
			using var command = database.CreateCommand();
			command.CommandText = $"DELETE FROM 'Tags' WHERE 'Tags'.'Name' == '{name}';";
			await command.ExecuteNonQueryAsync();
		}
		catch (SQLiteException ex) { }
		catch (Exception ex) { }
		finally
		{
			await database.CloseAsync();
		}
	}

	public static async Task<List<Tag>> GetTagsForGuild(ulong guildId)
	{
		List<Tag> tags = new();
		try
		{
			await DatabaseManager.database.OpenAsync();
			using var command = database.CreateCommand();
			command.CommandText = $"SELECT * FROM 'Tags' WHERE 'Tags'.'Guild' == {guildId}";
			var reader = await command.ExecuteReaderAsync();
			while (reader.Read())
			{
				tags.Add(new Tag()
				{
					Name = reader.GetString(0),
					Content = reader.GetString(1),
					Author = (ulong)reader.GetInt64(2),
					Guild = (ulong)reader.GetInt64(3),
					LastEdited = DateTimeOffset.FromUnixTimeSeconds((long)reader.GetValue(4)).DateTime
				});
			}
		}
		catch (SQLiteException ex) { }
		catch (Exception ex) { }
		finally {
			await database.CloseAsync();
		}

		return tags;
	}

	public static async Task<ulong> GetPluginAuthorId (string name) {
		ulong id = 0;
		try {
			await DatabaseManager.database.OpenAsync();
			using var command = database.CreateCommand();
			command.CommandText = $"SELECT Author FROM 'Plugins' WHERE Name LIKE '{name}';";
			var reader = await command.ExecuteReaderAsync();
			reader.Read();
			id = (ulong)reader.GetInt64(0);
		}
		catch (SQLiteException ex) { }
		catch (Exception ex) { }
		finally
		{
			await database.CloseAsync();
		}

		return id;
	}

	public static async Task<List<Tag>> GetTagsFromUser(ulong guildId,ulong userId)
	{
		List<Tag> tags = new();
		try
		{
			await DatabaseManager.database.OpenAsync();
			using var command = database.CreateCommand();
			command.CommandText = $"SELECT * FROM 'Tags' WHERE 'Tags'.'Author' == {userId} AND 'Tags'.'Guild' == {guildId}";
			var reader = await command.ExecuteReaderAsync();
			while (reader.Read())
			{
				tags.Add(new Tag()
				{
					Name = reader.GetString(0),
					Content = reader.GetString(1),
					Author = (ulong)reader.GetInt64(2),
					Guild = (ulong)reader.GetInt64(3),
					LastEdited = DateTimeOffset.FromUnixTimeSeconds((long)reader.GetValue(4)).DateTime
				});
			}
		}
		catch (SQLiteException ex) { }
		catch (Exception ex) { }
		finally
		{
			await database.CloseAsync();
		}

		return tags;
	}
	public static async Task<Tag> GetTag(string name, ulong guild)
	{
		foreach (var tag in await DatabaseManager.GetTagsForGuild(guild))
		{
			if (tag.Name == name) return tag;
		}
		return null;
	}
}

public enum IdType {
	User,
	Channel,
	Guild,
	Message,
}