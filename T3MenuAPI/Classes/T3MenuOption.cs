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
    public int MinValue { get; set; }
    public int MaxValue { get; set; }
    public int Step { get; set; }
}

