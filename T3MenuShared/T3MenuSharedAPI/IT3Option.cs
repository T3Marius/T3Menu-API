using CounterStrikeSharp.API.Core;
using T3MenuSharedApi;
public enum OptionType
{
    Button,
    Bool,
    Text,
    Slider,
    Input,
}
public interface IT3Option
{
    IT3Menu? Parent { get; set; }
    string? OptionDisplay { get; set; }
    Action<CCSPlayerController, IT3Option> OnChoose { get; set; }
    int Index { get; set; }
    OptionType Type { get; set; }
    public bool IsDisabled { get; set; }
    object? SliderValue { get; set; }
    List<object>? CustomValues { get; set; }
    Action<CCSPlayerController, IT3Option, int>? OnSlide { get; set; }
}