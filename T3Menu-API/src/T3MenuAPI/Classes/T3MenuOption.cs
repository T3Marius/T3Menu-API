using CounterStrikeSharp.API.Core;

public class T3Option : IT3Option
{
    public IT3Menu? Parent { get; set; }
    public string? OptionDisplay { get; set; }
    public Action<CCSPlayerController, IT3Option> OnChoose { get; set; } = delegate { };
    public int Index { get; set; }
    public OptionType Type { get; set; }
    public object? SliderValue { get; set; }
    public bool IsDisabled { get; set; }
    public List<object>? CustomValues { get; set; }
    public Action<CCSPlayerController, IT3Option, int>? OnSlide { get; set; }
    public string? InputValue { get; set; }
    public string PlaceHolder { get; set; } = "";

}
