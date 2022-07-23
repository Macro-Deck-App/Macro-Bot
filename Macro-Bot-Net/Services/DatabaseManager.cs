using Develeon64.MacroBot.Commands.Polls;
using Develeon64.MacroBot.Models;
using System.Data.SQLite;

namespace Develeon64.MacroBot.Services {
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
				using (SQLiteCommand command = database.CreateCommand()) {
					command.CommandText = "CREATE TABLE IF NOT EXISTS 'Tickets' ('Author' INTEGER, 'Channel' INTEGER UNIQUE, 'Message' INTEGER UNIQUE, 'Created' INTEGER, 'Modified' INTEGER, PRIMARY KEY('Author'));";
					await command.ExecuteNonQueryAsync();
				}

				using (SQLiteCommand command = database.CreateCommand())
				{
					command.CommandText = "CREATE TABLE IF NOT EXISTS 'Tags' ('Name' VARCHAR, 'Content' VARCHAR, 'Author' INTEGER, 'Guild' INTEGER, 'LastEditTimestamp' INTEGER, PRIMARY KEY('Name'));";
					await command.ExecuteNonQueryAsync();
				}

				using (SQLiteCommand command = database.CreateCommand()) {
					command.CommandText = "CREATE TABLE IF NOT EXISTS 'Devs' ('ID' INTEGER, 'DiscordName' TEXT NULL, 'DevName' TEXT, PRIMARY KEY('ID'));";
					await command.ExecuteNonQueryAsync();
				}
				using (SQLiteCommand command = database.CreateCommand()) {
					command.CommandText = "CREATE TABLE IF NOT EXISTS 'Plugins' ('ID' INTEGER AUTO_INCREMENT, 'Name' TEXT UNIQUE, 'Author' INTEGER, PRIMARY KEY('ID'), FOREIGN KEY ('Author') REFERENCES 'Devs'('ID') ON DELETE SET NULL);";
					await command.ExecuteNonQueryAsync();
				}

				using (SQLiteCommand command = database.CreateCommand())
                {
					command.CommandText = "CREATE TABLE IF NOT EXISTS 'Polls' ('PollId' INTEGER NOT NULL UNIQUE, 'MessageId' INTEGER,'ChannelId' INTEGER,'GuildId' INTEGER,'Author' INTEGER,'Name' TEXT,'Description' TEXT,'Votes1' INTEGER,'Votes2' INTEGER,'Votes3' INTEGER,'Votes4' INTEGER,'Closed' INTEGER,'AutoClose' INTEGER,PRIMARY KEY('PollId' AUTOINCREMENT));";
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
			bool exists = false;
			try {
				await DatabaseManager.database.OpenAsync();
				using (SQLiteCommand cmd = database.CreateCommand()) {
					cmd.CommandText = $"SELECT * FROM 'Tickets' WHERE 'Tickets'.'Author' == {author}";
					exists = await cmd.ExecuteScalarAsync() != null;
				}
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
				using (SQLiteCommand command = database.CreateCommand()) {
					command.CommandText = $"INSERT INTO 'Tickets' VALUES ({author}, {channel}, {message}, {created.ToUnixTimeSeconds()}, {created.ToUnixTimeSeconds()});";
					await command.ExecuteNonQueryAsync();
				}
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
				using (SQLiteCommand command = database.CreateCommand()) {
					command.CommandText = $"DELETE FROM 'Tickets' WHERE 'Tickets'.'{(type is IdType.User ? "Author" : "Channel")}' == {id};";
					await command.ExecuteNonQueryAsync();
				}
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
				using (SQLiteCommand command = database.CreateCommand()) {
					command.CommandText = $"UPDATE 'Tickets' SET 'Modified' = {DateTimeOffset.Now.ToUnixTimeSeconds()} WHERE 'Tickets'.'Author' == {author};";
					await command.ExecuteNonQueryAsync();
				}
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
				using (SQLiteCommand command = database.CreateCommand()) {
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
			}
			catch (SQLiteException ex) { }
			catch (Exception ex) { }
			finally {
				await database.CloseAsync();
			}

			return tickets;
		}

		public static async Task<Ticket> GetTicket (ulong author) {
			foreach (Ticket ticket in await DatabaseManager.GetTickets()) {
				if (ticket.Author == author) return ticket;
			}
			return null;
		}

		public static async Task<bool> TagExists(string name,ulong guildId)
		{
			bool exists = false;
			try
			{
				await DatabaseManager.database.OpenAsync();
				using (SQLiteCommand cmd = database.CreateCommand())
				{
					cmd.CommandText = $"SELECT * FROM 'Tags' WHERE 'Tags'.'Name' == '{name}' AND 'Tags'.'Guild' == {guildId}";
					exists = await cmd.ExecuteScalarAsync() != null;
				}
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
				using (SQLiteCommand command = database.CreateCommand())
				{
					command.CommandText = $"INSERT INTO 'Tags' VALUES ('{name}', '{content}', {author}, {guildId},{DateTimeOffset.Now.ToUnixTimeSeconds()});";
					await command.ExecuteNonQueryAsync();
				}
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
				using (SQLiteCommand command = database.CreateCommand())
				{
					command.CommandText = $"UPDATE 'Tags' SET 'Content' = '{content}', 'LastEditTimestamp' = {DateTimeOffset.Now.ToUnixTimeSeconds()} WHERE 'Tags'.'Name' == '{name}' AND 'Tags'.'Guild' == {guildId}";
					await command.ExecuteNonQueryAsync();
				}
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
				using (SQLiteCommand command = database.CreateCommand())
				{
					command.CommandText = $"DELETE FROM 'Tags' WHERE 'Tags'.'Name' == '{name}';";
					await command.ExecuteNonQueryAsync();
				}
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
				using (SQLiteCommand command = database.CreateCommand())
				{
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
				using (SQLiteCommand command = database.CreateCommand()) {
					command.CommandText = $"SELECT Author FROM 'Plugins' WHERE Name LIKE '{name}';";
					var reader = await command.ExecuteReaderAsync();
					reader.Read();
					id = (ulong)reader.GetInt64(0);
				}
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
				using (SQLiteCommand command = database.CreateCommand())
				{
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
			foreach (Tag tag in await DatabaseManager.GetTagsForGuild(guild))
			{
				if (tag.Name == name) return tag;
			}
			return null;
		}

		public static async Task<long> CreatePoll(ulong Author, ulong MessageId, ulong ChannelId, ulong GuildId, string name, string description,PollOption option)
		{
			long pollId = 0;

			try
			{
				await DatabaseManager.database.OpenAsync();
				using (SQLiteCommand command = database.CreateCommand())
				{
					string argumentAdd = "";
					string valueAdd = "";

                    switch (option)
                    {
                        case PollOption.YesNo:
							argumentAdd += "'Votes1','Votes2'";
							valueAdd += "0,0";

							break;
                        case PollOption.OneTwo:
							argumentAdd += "'Votes1','Votes2'";
							valueAdd += "0,0";

							break;
                        case PollOption.OneTwoThree:
							argumentAdd += "'Votes1','Votes2','Votes3'";
							valueAdd += "0,0,0";

							break;
                        case PollOption.OneTwoThreeFour:
							argumentAdd += "'Votes1','Votes2','Votes3','Votes4'";
							valueAdd += "0,0,0,0";

							break;
                        default:
                            break;
                    }

					command.CommandText = $"INSERT INTO 'Polls' " +
						$"('MessageId','ChannelId','GuildId','Author','Name','Description','Closed',{argumentAdd}) VALUES" +
						$"({MessageId},{ChannelId},{GuildId},{Author},'{name}','{description}','FALSE',{valueAdd});";

					await command.ExecuteNonQueryAsync();
				}

				using (SQLiteCommand command = database.CreateCommand())
                {
					command.CommandText = $"SELECT * FROM 'Polls' WHERE MessageId = {MessageId};";
					var idpollId = await command.ExecuteScalarAsync();
					pollId = (long)idpollId;
				}
			}
			catch (SQLiteException ex) { }
			catch (Exception ex) { }
			finally
			{
				await database.CloseAsync();
			}

			return pollId;
		}
		public static async Task UpdatePoll(long pollId,Poll poll)
        {
			try
			{
				await DatabaseManager.database.OpenAsync();
				using (SQLiteCommand command = database.CreateCommand())
				{
					command.CommandText = $"UPDATE 'Polls' SET 'Name' = {poll.Name},'Name' = {poll.Description},'Name' = {poll.Votes1},'Name' = {poll.Votes2},'Name' = {poll.Votes3},'Name' = {poll.Votes4},'Name' = {poll.Closed} WHERE 'PollId' = {pollId}";
					await command.ExecuteNonQueryAsync();
				}
			}
			catch (SQLiteException ex) { }
			catch (Exception ex) { }
			finally
			{
				await database.CloseAsync();
			}
		}
		public static async Task<List<Poll>> GetPolls()
		{
			List<Poll> polls = new();
			try
			{
				await DatabaseManager.database.OpenAsync();
				using (SQLiteCommand command = database.CreateCommand())
				{
					command.CommandText = $"SELECT * FROM 'Polls'";
					var reader = await command.ExecuteReaderAsync();
					while (reader.Read())
					{
						polls.Add(new Poll()
						{
							Id = reader.GetInt64(0),
							MessageId = (ulong)reader.GetInt64(1),
							ChannelId = (ulong)reader.GetInt64(2),
							GuildId = (ulong)reader.GetInt64(3),
							Author = (ulong)reader.GetInt64(4),
							Name = reader.GetString(5),
							Description = reader.GetString(6),
							Votes1 = reader.GetInt32(7),
							Votes2 = reader.GetInt32(8),
							Votes3 = reader.GetInt32(9),
							Votes4 = reader.GetInt32(10),
							Closed = reader.GetBoolean(11),
						});
					}
				}
			}
			catch (SQLiteException ex) { }
			catch (Exception ex) { }
			finally
			{
				await database.CloseAsync();
			}

			return polls;
		}
		public static async Task<Poll> GetPoll(long pollId)
        {
			List<Poll> polls = await GetPolls();
			return polls.Find(poll => poll.Id == pollId);
        }
		public static async Task<Poll> GetPoll(ulong messageId)
		{
			List<Poll> polls = await GetPolls();
			return polls.Find(poll => poll.MessageId == messageId);
		}

	}

	public enum IdType {
		User,
		Channel,
		Guild,
		Message,
	}
}
