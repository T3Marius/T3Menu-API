using CounterStrikeSharp.API.Core;

public class T3Option : IT3Option
{
    public IT3Menu? Parent { get; set; }
    public string? OptionDisplay { get; set; }
    public Action<CCSPlayerController, IT3Option> OnChoose { get; set; } = delegate { };
    public int Index { get; set; }
    public OptionType Type { get; set; }

    // Updated slider-related properties
    public object? SliderValue { get; set; } // Changed from int to object
    public List<object>? CustomValues { get; set; } // Changed from List<int> to List<object>

    public Action<CCSPlayerController, IT3Option, int>? OnSlide { get; set; }
}
