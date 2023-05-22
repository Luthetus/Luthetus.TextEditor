using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Theme;

namespace Luthetus.TextEditor.RazorLib;

public class LuthetusTextEditorCustomThemeFacts
{
    public static readonly ThemeRecord LightTheme = new ThemeRecord(
        new ThemeKey(Guid.Parse("8165209b-0cea-45b4-b6dd-e5661b319c73")),
        "Luthetus IDE Light Theme",
        "bcrl_light-theme",
        ThemeContrastKind.Default,
        ThemeColorKind.Light,
        new[] { ThemeScope.TextEditor }.ToImmutableList());

    public static readonly ThemeRecord DarkTheme = new ThemeRecord(
        new ThemeKey(Guid.Parse("56d64327-03c2-48a3-b086-11b101826efb")),
        "Luthetus IDE Dark Theme",
        "bcrl_dark-theme",
        ThemeContrastKind.Default,
        ThemeColorKind.Dark,
        new[] { ThemeScope.TextEditor }.ToImmutableList());

    public static readonly ImmutableArray<ThemeRecord> AllCustomThemes = new[]
    {
        LightTheme,
        DarkTheme
    }.ToImmutableArray();
}