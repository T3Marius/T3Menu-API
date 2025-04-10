using CounterStrikeSharp.API.Core;

public interface IT3Menu
{
    public string? Title { get; set; }
    public LinkedList<IT3Option> Options { get; set; }
    public LinkedList<IT3Option>? Prev { get; set; }
    public bool FreezePlayer { get; set; }
    public bool HasSound { get; set; }
    public bool IsSubMenu { get; set; }
    public bool showDeveloper { get; set; }
    public IT3Menu? ParentMenu { get; set; }

    public LinkedListNode<IT3Option> AddOption(string display, Action<CCSPlayerController, IT3Option> onChoice, bool isDisabled = false);
    public LinkedListNode<IT3Option> AddBoolOption(string display, bool defaultValue = false, Action<CCSPlayerController, IT3Option>? onToggle = null);
    public LinkedListNode<IT3Option> AddSliderOption(string display, List<object> values, object? defaultValue = null, int displayItems = 3, Action<CCSPlayerController, IT3Option, int>? onSlide = null);
    public void AddTextOption(string display, bool selectable = false);
}
