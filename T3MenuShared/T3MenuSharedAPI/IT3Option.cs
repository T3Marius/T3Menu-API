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
    int SliderValue { get; set; }
    int MinValue { get; set; }
    int MaxValue { get; set; }
    int Step { get; set; }
}
