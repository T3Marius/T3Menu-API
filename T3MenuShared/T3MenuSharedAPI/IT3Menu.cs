using CounterStrikeSharp.API.Core;

public interface IT3Menu
{
    string? Title { get; set; }
    LinkedList<IT3Option> Options { get; set; }
    LinkedList<IT3Option>? Prev { get; set; }
    bool FreezePlayer { get; set; }
    bool HasSound { get; set; }
    bool IsSubMenu { get; set; }
    bool showDeveloper { get; set; }
    IT3Menu? ParentMenu { get; set; }

    LinkedListNode<IT3Option> Add(string display, Action<CCSPlayerController, IT3Option> onChoice);
    LinkedListNode<IT3Option> AddBoolOption(string display, bool defaultValue = false, Action<CCSPlayerController, IT3Option>? onToggle = null);
    void AddTextOption(string display);
    LinkedListNode<IT3Option> AddSliderOption(
        string display,
        List<object> customValues,
        object defaultValue,
        Action<CCSPlayerController, IT3Option>? onSlide = null);
}
