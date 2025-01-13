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

    public Action<CCSPlayerController, IT3Option, int>? OnSlide { get; set; }

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

    public void AddTextOption(string display)
    {
        if (Options == null)
            Options = new();

        T3Option newTextOption = new()
        {
            OptionDisplay = display,
            Parent = this,
            Type = OptionType.Text
        };

        Options.AddLast(newTextOption);
    }
    public LinkedListNode<IT3Option> AddSliderOption(
       string display,
       List<object> customValues,
       object defaultValue,
       Action<CCSPlayerController, IT3Option>? onSlide = null)
    {
        if (!customValues.Contains(defaultValue))
            throw new ArgumentException("Default value must be in the custom values list.");

        var newSliderOption = new T3Option
        {
            // Do not append defaultValue if display is empty or null
            OptionDisplay = !string.IsNullOrWhiteSpace(display)
                ? $"{display}: {defaultValue}" // Show display and value
                : null, // Set OptionDisplay to null when display is empty

            SliderValue = defaultValue,
            CustomValues = customValues,
            Index = Options.Count,
            Parent = this,
            Type = OptionType.Slider,
            OnChoose = (player, option) =>
            {
                onSlide?.Invoke(player, option);
            },
            OnSlide = (player, option, direction) =>
            {
                var sliderOption = (T3Option)option;

                var customValuesList = sliderOption.CustomValues!;
                int currentIndex = customValuesList.IndexOf(sliderOption.SliderValue!);
                int newIndex = Math.Clamp(currentIndex + direction, 0, customValuesList.Count - 1);

                sliderOption.SliderValue = customValuesList[newIndex];

                // Update OptionDisplay dynamically
                sliderOption.OptionDisplay = !string.IsNullOrWhiteSpace(display)
                    ? $"{display}: {sliderOption.SliderValue}" // Show display and value
                    : null; // Set OptionDisplay to null when display is empty

                onSlide?.Invoke(player, sliderOption);
            }
        };

        return Options.AddLast(newSliderOption);
    }





}
