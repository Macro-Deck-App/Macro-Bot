using Discord;

namespace Develeon64.MacroBot.Utils;
public class DiscordEmbedBuilder : EmbedBuilder {
	/*public DiscordEmbedBuilder (Color? color) : base() {
		if (color == null)
			this.Color = new Color(63, 127, 191);
		else
			this.Color = color;
	}*/

	public EmbedBuilder AddBlankField (bool inline = false) {
		return this.AddField("\u200b", "\u200b", inline);
	}
}
