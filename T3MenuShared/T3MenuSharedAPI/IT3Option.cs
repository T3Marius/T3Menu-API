using CounterStrikeSharp.API.Core;
public enum OptionType
{
    Button,
    Bool,
    Text,
    Slider,
}
public interface IT3Option
{
    public IT3Menu? Parent { get; set; }
    public string? OptionDisplay { get; set; }
    public Action<CCSPlayerController, IT3Option> OnChoose { get; set; }
    public int Index { get; set; }
    public OptionType Type { get; set; }
    public bool IsDisabled { get; set; }
    public Action<CCSPlayerController, IT3Option, int>? OnSlide { get; set; }
    public object? DefaultValue { get; set; }
    public int DisplayItems { get; set; }
    public List<object>? CustomValues { get; set; }
    public object? Value
    {
        get => DefaultValue;
        set => DefaultValue = value;
    }
    public int GetSelectedIndex()
    {
        if (CustomValues == null || DefaultValue == null)
            return 0;

        int idx = CustomValues.FindIndex(x => x?.Equals(DefaultValue) == true);
        return idx >= 0 ? idx : 0;
    }

}