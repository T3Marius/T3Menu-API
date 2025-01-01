using CounterStrikeSharp.API.Core;
using T3MenuSharedApi;
namespace T3MenuAPI;
public class T3Option : IT3Option
{
    public IT3Menu? Parent { get; set; }
    public string? OptionDisplay { get; set; }
    public Action<CCSPlayerController, IT3Option> OnChoose { get; set; } = delegate { };
    public int Index { get; set; }
    public OptionType Type { get; set; } = OptionType.Button;
    public int SliderValue { get; set; }
    public List<int>? CustomValues { get; set; } = new();
    public Action<CCSPlayerController, IT3Option, int>? OnSlide { get; set; } = (_, _, _) => { };
}

