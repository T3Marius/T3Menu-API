using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using T3MenuAPI;

public class T3Menu : IT3Menu
{
    public string? Title { get; set; } = "";
    public LinkedList<IT3Option> Options { get; set; } = new();
    public LinkedList<IT3Option>? Prev { get; set; } = null;
    public bool FreezePlayer { get; set; } = true;
    public bool HasSound { get; set; } = true;
    public IT3Menu? ParentMenu { get; set; } = null;
    public bool IsSubMenu { get; set; } = false;
    public bool showDeveloper { get; set; } = true;
    public Action<CCSPlayerController, IT3Option, int>? OnSlide { get; set; }
    public int LastSelectedIndex { get; set; } = 0;
    public LinkedListNode<IT3Option> AddOption(string display, Action<CCSPlayerController, IT3Option> onChoice, bool isDisabled = false)
    {
        T3Option newOption = new()
        {
            OptionDisplay = isDisabled ? $"<font color='#34282C'>{display}</font>" : display,
            OnChoose = isDisabled ? null! : onChoice,
            Index = Options.Count,
            Parent = this,
            Type = OptionType.Button,
            IsDisabled = isDisabled
        };

        return Options.AddLast(newOption);
    }
    public LinkedListNode<IT3Option> AddSliderOption(string display, List<object> values, object? defaultValue = null, int displayItems = 3, Action<CCSPlayerController, IT3Option, int>? onSelectConfirm = null)
    {
        if (values == null || values.Count == 0)
        {
            T3Option newOption = new()
            {
                OptionDisplay = $"{display}: No items",
                Index = Options.Count,
                Parent = this,
                Type = OptionType.Slider,
                IsDisabled = true,
                CustomValues = new List<object>()
            };
            return Options.AddLast(newOption);
        }

        if (defaultValue == null && values.Count > 0)
        {
            defaultValue = values[0];
        }

        displayItems = Math.Max(1, Math.Min(displayItems, values.Count));

        T3Option sliderOption = new()
        {
            OptionDisplay = display,
            OnChoose = (player, option) => {
                if (option is T3Option sOption && sOption.CustomValues != null)
                {
                    int idx = sOption.GetSelectedIndex();
                    onSelectConfirm?.Invoke(player, sOption, idx);
                }
            },
            Index = Options.Count,
            Parent = this,
            Type = OptionType.Slider,
            DefaultValue = defaultValue,
            DisplayItems = displayItems,
            CustomValues = values,
            OnSlide = (player, option, index) => {
            }
        };

        return Options.AddLast(sliderOption);
    }
    public LinkedListNode<IT3Option> AddBoolOption(string display, bool defaultValue = false, Action<CCSPlayerController, IT3Option>? onToggle = null)
    {
        if (Options == null)
            Options = new();

        T3Option newBoolOption = new()
        {
            OptionDisplay = $"{display}: {(defaultValue ? "[<font color='green'>✔</font>]" : "[<font color='red'>❌</font>]")}",
            Index = Options.Count,
            Parent = this,
            Type = OptionType.Bool,
            OnChoose = (player, option) =>
            {
                if (option is T3Option boolOption)
                {
                    bool isDisabled = boolOption.OptionDisplay!.Contains("❌");
                    boolOption.OptionDisplay = $"{display}: {(isDisabled ? "[<font color='green'>✔</font>]" : "[<font color='red'>❌</font>]")}";
                    onToggle?.Invoke(player, boolOption);
                }
            }
        };

        return Options.AddLast(newBoolOption);
    }
    public void AddTextOption(string display, bool selectable = false)
    {
        if (Options == null)
            Options = new();

        T3Option newTextOption = new()
        {
            OptionDisplay = display,
            Parent = this,
            Type = OptionType.Text,
            IsDisabled = !selectable
        };

        Options.AddLast(newTextOption);
    }
}
