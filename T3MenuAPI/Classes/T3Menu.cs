using CounterStrikeSharp.API.Core;
using T3MenuAPI;

public class T3Menu : IT3Menu
{
    public string? Title { get; set; } = "";
    public LinkedList<IT3Option> Options { get; set; } = new();
    public LinkedList<IT3Option>? Prev { get; set; } = null;
    public bool FreezePlayer { get; set; } = true;
    public bool HasSound { get; set; } = true;
    public bool IsSubMenu { get; set; } = false;
    public bool showDeveloper { get; set; } = true;
    public IT3Menu? ParentMenu { get; set; } = null;

    public LinkedListNode<IT3Option> Add(string display, Action<CCSPlayerController, IT3Option> onChoice)
    {
        if (Options == null)
            Options = new();

        T3Option newOption = new()
        {
            OptionDisplay = display,
            OnChoose = onChoice,
            Index = Options.Count,
            Parent = this,
            Type = OptionType.Button
        };

        return Options.AddLast(newOption);
    }

    public LinkedListNode<IT3Option> AddBoolOption(string display, bool defaultValue = false, Action<CCSPlayerController, IT3Option>? onToggle = null)
    {
        if (Options == null)
            Options = new();

        T3Option newBoolOption = new()
        {
            OptionDisplay = $"{display}: {(defaultValue ? "<font color='green'>✔</font>" : "<font color='red'>❌</font>")}",
            Index = Options.Count,
            Parent = this,
            Type = OptionType.Bool,
            OnChoose = (player, option) =>
            {
                if (option is T3Option boolOption)
                {
                    bool isDisabled = boolOption.OptionDisplay!.Contains("❌");
                    boolOption.OptionDisplay = $"{display}: {(isDisabled ? "<font color='green'>✔</font>" : "<font color='red'>❌</font>")}";
                    onToggle?.Invoke(player, boolOption);
                }
            }
        };

        return Options.AddLast(newBoolOption);
    }

    public LinkedListNode<IT3Option> AddTextOption(string display)
    {
        if (Options == null)
            Options = new();

        T3Option newTextOption = new()
        {
            OptionDisplay = display,
            Index = Options.Count,
            Parent = this,
            Type = OptionType.Text
        };

        return Options.AddLast(newTextOption);
    }

    public LinkedListNode<IT3Option> AddSliderOption(string display, int minValue, int maxValue, int step, int defaultValue, Action<CCSPlayerController, IT3Option>? onSlide = null)
    {
        if (Options == null)
            Options = new();

        T3Option newSliderOption = new()
        {
            OptionDisplay = $"{display}: {defaultValue}",
            SliderValue = defaultValue,
            MinValue = minValue,
            MaxValue = maxValue,
            Step = step,
            Index = Options.Count,
            Parent = this,
            Type = OptionType.Slider,
            OnChoose = (player, option) =>
            {
                onSlide?.Invoke(player, option);
            }
        };

        return Options.AddLast(newSliderOption);
    }
}
