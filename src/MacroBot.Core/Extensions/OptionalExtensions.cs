using System.Runtime.CompilerServices;
using Discord;

namespace MacroBot.Core.Extensions;

public static class OptionalExtensions {
    public static Optional<T> ToOptional<T>(this T typ){
        return typ;
    }
}