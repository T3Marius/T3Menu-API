using CounterStrikeSharp.API.Core;
using T3MenuSharedApi;
public enum OptionType
{
    Button,
    Bool,
    Text,
    Slider,
}
public interface IT3Option
{
    IT3Menu? Parent { get; set; }
    string? OptionDisplay { get; set; }
    Action<CCSPlayerController, IT3Option> OnChoose { get; set; }
    int Index { get; set; }
    OptionType Type { get; set; }
    object? SliderValue { get; set; } // Supports any type for the slider value
    List<object>? CustomValues { get; set; } // Supports any type for custom values

    Action<CCSPlayerController, IT3Option, int>? OnSlide { get; set; }
}