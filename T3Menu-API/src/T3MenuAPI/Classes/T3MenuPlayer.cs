using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using Microsoft.Extensions.Localization;
using System.Text;
using T3MenuAPI.Classes;
using static T3MenuAPI.T3MenuAPI;
using CounterStrikeSharp.API.Modules.Utils;

namespace T3MenuAPI
{
    public class T3MenuPlayer
    {
        public CCSPlayerController? player { get; set; }
        public bool InputMode { get; set; } = false;
        public IT3Option? CurrentInputOption { get; set; } = null;
        public T3Menu? MainMenu = null;
        public IT3Menu? CurrentMenu = null;

        public LinkedListNode<IT3Option>? CurrentChoice = null;
        public LinkedListNode<IT3Option>? MenuStart = null;
        public string CenterHtml = "";
        public int VisibleOptions = 4;
        public static IStringLocalizer? Localizer = null;
        public PlayerButtons Buttons { get; set; }
        private readonly Dictionary<IT3Menu, LinkedListNode<IT3Option>?> _lastSelectedOptions = new();

        public void OpenMainMenu(T3Menu? menu)
        {
            if (menu == null)
            {
                CloseMenu();
                return;
            }

            MainMenu = menu;
            CurrentMenu = menu;

            MenuStart = MainMenu.Options.First;
            CurrentChoice = FindFirstSelectableOption(MainMenu.Options);

            if (player != null)
            {
                ActiveMenus[player] = menu;
            }

            if (player != null && Instance.Config.Settings.FreezePlayer)
            {
                player.Freeze();
                System.Console.WriteLine("Freeze player called");
            }
            Server.NextFrame(() =>
            {
                UpdateCenterHtml();
            });
        }

        public void OpenSubMenu(IT3Menu? menu)
        {
            if (menu == null)
            {
                CurrentMenu = MainMenu;
                MenuStart = MainMenu?.Options.First;
                CurrentChoice = FindFirstSelectableOption(MainMenu?.Options);
                UpdateCenterHtml();
                return;
            }

            if (CurrentMenu != null && CurrentChoice != null)
                _lastSelectedOptions[CurrentMenu] = CurrentChoice;

            CurrentMenu = menu;
            VisibleOptions = string.IsNullOrEmpty(menu.Title) ? 5 : 4;

            MenuStart = menu.Options.First;
            CurrentChoice = FindFirstSelectableOption(menu.Options);

            UpdateCenterHtml();
        }
        public void Close()
        {
            if (MainMenu != null && player != null)
            {
                if (MainMenu.IsExitable)
                {
                    CloseMenu();
                }
            }
        }
        private LinkedListNode<IT3Option>? FindFirstSelectableOption(LinkedList<IT3Option>? options)
        {
            if (options == null || options.Count == 0)
                return null;

            var node = options.First;
            while (node != null && node.Value.IsDisabled)
            {
                node = node.Next;
            }

            return node ?? options.First;
        }

        public void CloseSubMenu()
        {
            if (CurrentMenu == null || CurrentMenu == MainMenu)
            {
                return;
            }

            IT3Menu? parentMenu = null;
            if (CurrentMenu is T3Menu current && current.ParentMenu != null)
            {
                parentMenu = current.ParentMenu;
            }
            else
            {
                parentMenu = MainMenu;
            }

            if (parentMenu == null)
                return;

            LinkedListNode<IT3Option>? lastSelected = null;
            if (_lastSelectedOptions.TryGetValue(parentMenu, out lastSelected))
            {
                if (lastSelected == null || !parentMenu.Options.Contains(lastSelected.Value) || !IsSelectable(lastSelected.Value))
                {
                    lastSelected = FindFirstSelectableOption(parentMenu.Options);
                }
            }
            else
            {
                lastSelected = FindFirstSelectableOption(parentMenu.Options);
            }

            CurrentMenu = parentMenu;

            if (parentMenu == MainMenu)
            {
                VisibleOptions = 4;
            }
            else
            {
                VisibleOptions = string.IsNullOrEmpty(parentMenu.Title) ? 5 : 4;
            }

            MenuStart = parentMenu.Options.First;
            CurrentChoice = lastSelected;

            if (CurrentChoice != null)
            {
                int selectedIndex = GetIndex(CurrentChoice);

                if (selectedIndex >= VisibleOptions)
                {
                    int targetStart = Math.Max(0, selectedIndex - (VisibleOptions / 2));
                    MenuStart = parentMenu.Options.First;
                    for (int i = 0; i < targetStart && MenuStart?.Next != null; i++)
                    {
                        MenuStart = MenuStart.Next;
                    }
                }
            }
            Server.NextFrame(() =>
            {
                UpdateCenterHtml();
            });
        }
        public void CloseAllSubMenus()
        {
            if (MainMenu == null)
                return;

            CurrentMenu = MainMenu;
            CurrentChoice = FindFirstSelectableOption(MainMenu.Options);
            MenuStart = MainMenu.Options.First;
            VisibleOptions = 4;

            UpdateCenterHtml();
        }

        public void CloseMenu()
        {
            if (MainMenu == null)
                return;

            MainMenu = null;
            CurrentMenu = null;
            CurrentChoice = null;
            MenuStart = null;
            CenterHtml = "";

            _lastSelectedOptions.Clear();

            if (player != null)
            {
                Players[player.Slot].OpenMainMenu(null);
                ActiveMenus.Remove(player);

                if (Instance.Config.Settings.FreezePlayer)
                {
                    player.Unfreeze();
                }
            }
        }
        public void Choose()
        {
            if (player != null && CurrentChoice?.Value != null && !CurrentChoice.Value.IsDisabled)
            {
                if (MainMenu?.HasSound == true)
                {
                    RecipientFilter filter = [player];
                    player.EmitSound(Instance.Config.Sounds.Select, filter, Instance.Config.Sounds.Volume);
                }

                CurrentChoice.Value.OnChoose.Invoke(player, CurrentChoice.Value);
            }

            UpdateCenterHtml();
        }
        public void ScrollUp()
        {
            if (CurrentMenu == null || CurrentChoice == null)
                return;

            int selectableCount = CurrentMenu.Options.Count(opt => IsSelectable(opt));
            if (selectableCount <= 1)
                return;


            int oldStartIndex = GetIndex(MenuStart);
            var prevSelectable = FindPreviousSelectableOption(CurrentChoice);

            if (prevSelectable == null)
            {
                var lastNode = CurrentMenu.Options.Last;
                while (lastNode != null)
                {
                    if (IsSelectable(lastNode.Value))
                    {
                        prevSelectable = lastNode;
                        break;
                    }
                    lastNode = lastNode.Previous;
                }
            }

            if (prevSelectable != null)
            {
                CurrentChoice = prevSelectable;
                int newChoiceIndex = GetIndex(CurrentChoice);

                if (newChoiceIndex > oldStartIndex + VisibleOptions - 1)
                {
                    int totalOptions = CurrentMenu.Options.Count;
                    int startIndex = Math.Max(0, totalOptions - VisibleOptions);

                    MenuStart = CurrentMenu.Options.First;
                    for (int i = 0; i < startIndex && MenuStart?.Next != null; i++)
                    {
                        MenuStart = MenuStart.Next;
                    }
                }
                else if (newChoiceIndex < oldStartIndex)
                {
                    MenuStart = CurrentChoice;
                }

                if (MainMenu?.HasSound == true)
                {
                    if (player != null)
                    {
                        RecipientFilter filter = [player];
                        player.EmitSound(Instance.Config.Sounds.ScrollUp, filter, Instance.Config.Sounds.Volume);
                    }
                }
                UpdateCenterHtml();
            }
        }
        public void ScrollDown()
        {
            if (CurrentMenu == null || CurrentChoice == null)
                return;

            int selectableCount = CurrentMenu.Options.Count(opt => IsSelectable(opt));
            if (selectableCount <= 1)
                return;

            int oldStartIndex = GetIndex(MenuStart);
            int visibleEnd = oldStartIndex + VisibleOptions - 1;
            var nextSelectable = FindNextSelectableOption(CurrentChoice);

            if (nextSelectable == null)
            {
                var firstNode = CurrentMenu.Options.First;
                while (firstNode != null)
                {
                    if (IsSelectable(firstNode.Value))
                    {
                        nextSelectable = firstNode;
                        break;
                    }
                    firstNode = firstNode.Next;
                }
            }

            if (nextSelectable != null)
            {
                CurrentChoice = nextSelectable;
                int newChoiceIndex = GetIndex(CurrentChoice);

                if (newChoiceIndex < oldStartIndex && nextSelectable == CurrentMenu.Options.First)
                {
                    MenuStart = CurrentMenu.Options.First;
                }
                else if (newChoiceIndex > visibleEnd)
                {
                    int nodesToMove = newChoiceIndex - visibleEnd;
                    var newStart = MenuStart;
                    for (int i = 0; i < nodesToMove && newStart?.Next != null; i++)
                    {
                        newStart = newStart.Next;
                    }
                    MenuStart = newStart;
                }

                if (MainMenu?.HasSound == true)
                {
                    if (player != null)
                    {
                        RecipientFilter filter = [player];
                        player.EmitSound(Instance.Config.Sounds.ScrollDown, filter, Instance.Config.Sounds.Volume);
                    }
                }
                UpdateCenterHtml();
            }
        }
        public void SlideLeft()
        {
            if (CurrentChoice?.Value == null || CurrentChoice.Value.Type != OptionType.Slider || player == null)
                return;

            T3Option sliderOption = (T3Option)CurrentChoice.Value;
            if (sliderOption.CustomValues == null || sliderOption.CustomValues.Count == 0)
                return;

            int currentIndex = 0;
            if (sliderOption.DefaultValue != null)
            {
                currentIndex = sliderOption.CustomValues.FindIndex(x => x.Equals(sliderOption.DefaultValue));
                if (currentIndex < 0) currentIndex = 0;
            }

            currentIndex--;
            if (currentIndex < 0)
                return;

            sliderOption.DefaultValue = sliderOption.CustomValues[currentIndex];

            sliderOption.OnSlide?.Invoke(player, sliderOption, currentIndex);

            if (MainMenu?.HasSound == true)
            {
                RecipientFilter filter = [player];
                player.EmitSound(Instance.Config.Sounds.SlideLeft, filter, Instance.Config.Sounds.Volume);
            }

            UpdateCenterHtml();
        }
        public void SlideRight()
        {
            if (CurrentChoice?.Value == null || CurrentChoice.Value.Type != OptionType.Slider || player == null)
                return;

            T3Option sliderOption = (T3Option)CurrentChoice.Value;
            if (sliderOption.CustomValues == null || sliderOption.CustomValues.Count == 0)
                return;

            int currentIndex = 0;
            if (sliderOption.DefaultValue != null)
            {
                currentIndex = sliderOption.CustomValues.FindIndex(x => x.Equals(sliderOption.DefaultValue));
                if (currentIndex < 0) currentIndex = 0;
            }

            currentIndex++;
            if (currentIndex >= sliderOption.CustomValues.Count)
                return;

            sliderOption.DefaultValue = sliderOption.CustomValues[currentIndex];

            sliderOption.OnSlide?.Invoke(player, sliderOption, currentIndex);

            if (MainMenu?.HasSound == true)
            {
                RecipientFilter filter = [player];
                player.EmitSound(Instance.Config.Sounds.SlideLeft, filter, Instance.Config.Sounds.Volume);
            }

            UpdateCenterHtml();
        }
        private LinkedListNode<IT3Option>? FindNextSelectableOption(LinkedListNode<IT3Option> current)
        {
            if (CurrentMenu == null)
                return null;

            var candidate = current.Next;
            while (candidate != null)
            {
                if (!candidate.Value.IsDisabled)
                    return candidate;
                candidate = candidate.Next;
            }

            candidate = CurrentMenu.Options.First;
            while (candidate != null && candidate != current)
            {
                if (!candidate.Value.IsDisabled)
                    return candidate;
                candidate = candidate.Next;
            }

            return current;
        }

        private LinkedListNode<IT3Option>? FindPreviousSelectableOption(LinkedListNode<IT3Option> current)
        {
            if (CurrentMenu == null)
                return null;

            var candidate = current.Previous;
            while (candidate != null)
            {
                if (!candidate.Value.IsDisabled)
                    return candidate;
                candidate = candidate.Previous;
            }

            candidate = CurrentMenu.Options.Last;
            while (candidate != null && candidate != current)
            {
                if (!candidate.Value.IsDisabled)
                    return candidate;
                candidate = candidate.Previous;
            }

            return current;
        }
        public void UpdateCenterHtml()
        {
            if (player == null || CurrentMenu == null)
            {
                return;
            }

            var builder = new StringBuilder();

            int totalMenuItems = CurrentMenu.Options.Count;
            int currentIndex = GetIndex(CurrentChoice);


            builder.Append($"<b><font color='red' class='fontSize-m'>{CurrentMenu.Title.TruncateHtml(CurrentMenu.MaxTitleLenght)}</font></b> <font color='yellow' class='fontSize-sm'>{currentIndex + 1}</font>/<font color='orange' class='fontSize-sm'>{totalMenuItems}</font>");
            builder.AppendLine("<br>");

            string leftArrow = Instance.Config.Controls.LeftArrow;
            string rightArrow = Instance.Config.Controls.RightArrow;
            string leftBracket = Instance.Config.Controls.LeftBracket;
            string rightBracket = Instance.Config.Controls.RightBracket;

            var current = MenuStart;
            int visibleCount = 0;

            for (int optionIndex = 0; optionIndex < VisibleOptions && current != null; optionIndex++)
            {
                string color = (current == CurrentChoice && !current.Value!.IsDisabled) ? "#9acd32" : "white";
                string optionDisplay = current.Value?.OptionDisplay ?? "";

                if (current.Value!.IsDisabled)
                {
                    builder.Append($"<font color='grey' class='fontSize-m'>{optionDisplay.TruncateHtml(CurrentMenu.MaxTitleLenght)}</font>");
                }
                else if (current.Value?.Type == OptionType.Slider)
                {
                    UpdateSliderOptionText(builder, current, color);
                }
                else if (current.Value?.Type == OptionType.Text)
                {
                    if (current == CurrentChoice)
                    {
                        builder.Append($"<font color='#FFFF00'>{rightArrow}{rightBracket} </font><font class='fontSize-m'>{optionDisplay.TruncateHtml(CurrentMenu.MaxTitleLenght)}</font><font color='#FFFF00'> {leftBracket}{leftArrow}</font>");
                    }
                    else
                    {
                        builder.Append($"<font color='{color}' class='fontSize-m'>{optionDisplay.TruncateHtml(CurrentMenu.MaxTitleLenght)}</font>");
                    }
                }
                else if (current.Value?.Type == OptionType.Input)
                {
                    string displayText = current.Value.OptionDisplay ?? "";

                    if (InputMode && CurrentInputOption == current.Value)
                    {
                        string displayPart = displayText.Split(':')[0];
                        displayText = $"{displayPart}: [<font color='grey'>Typing...</font>]";
                    }
                    if (current == CurrentChoice)
                    {
                        builder.Append($"<b><font color='yellow'>{rightArrow}{rightBracket}</font> <font color='{color}' class='fontSize-m'>{displayText.TruncateHtml(CurrentMenu.MaxOptionLenght)}</font> <font color='yellow'>{leftBracket}{leftArrow}</font></b>");
                    }
                    else
                    {
                        builder.Append($"<font color='{color}' class='fontSize-m'>{displayText.TruncateHtml(CurrentMenu.MaxOptionLenght)}</font>");
                    }
                }
                else if (current.Value?.Type == OptionType.Bool || current.Value?.Type == OptionType.Button)
                {
                    // Handle Button (default) and Bool types with MaxOptionLenght
                    if (current == CurrentChoice)
                    {
                        builder.Append($"<b><font color='yellow'>{rightArrow}{rightBracket}</font> <font color='{color}' class='fontSize-m'>{optionDisplay.TruncateHtml(CurrentMenu.MaxOptionLenght)}</font> <font color='yellow'>{leftBracket}{leftArrow}</font></b>");
                    }
                    else
                    {
                        builder.Append($"<font color='{color}' class='fontSize-m'>{optionDisplay.TruncateHtml(CurrentMenu.MaxOptionLenght)}</font>");
                    }
                }
                else if (current == CurrentChoice)
                {
                    builder.Append($"<b><font color='yellow'>{rightArrow}{rightBracket}</font> <font color='{color}' class='fontSize-m'>{optionDisplay.TruncateHtml(CurrentMenu.MaxOptionLenght)}</font> <font color='yellow'>{leftBracket}{leftArrow}</font></b>");
                }
                else
                {
                    builder.Append($"<font color='{color}' class='fontSize-m'>{optionDisplay.TruncateHtml(CurrentMenu.MaxOptionLenght)}</font>");
                }

                builder.AppendLine("<br>");
                current = current.Next;
                visibleCount++;
            }
            if (current != null)
            {
                builder.AppendLine("<img src='https://raw.githubusercontent.com/ssypchenko/GG1MapChooser/main/Resources/arrow.gif' class=''> <img src='https://raw.githubusercontent.com/ssypchenko/GG1MapChooser/main/Resources/arrow.gif' class=''> <img src='https://raw.githubusercontent.com/ssypchenko/GG1MapChooser/main/Resources/arrow.gif' class=''><br>");
            }
            else if (visibleCount < VisibleOptions)
            {
                for (int paddingIndex = visibleCount; paddingIndex < VisibleOptions; paddingIndex++)
                {
                    builder.AppendLine("<br>");
                }
            }

            if (Instance.Config.Settings.ShowDeveloperInfo)
            {
                string developerInfo = "<font class='fontSize-s' color='white'>Developed by <font color='#ff3333'>T3Marius</font></font>";
                builder.Append(developerInfo);
                builder.AppendLine("<br>");
            }

            if (CurrentMenu != null && CurrentMenu is T3Menu menu)
            {
                string controlsInfo;
                if (menu.IsSubMenu)
                {
                    controlsInfo = $"<font color='#ff3333' class='fontSize-sm'>Select: <font color='#f5a142'>{menu.GetEffectiveControlInfo("Select")} <font color='#FFFFFF'>| <font color='#ff3333' class='fontSize-m'>Back: <font color='#f5a142'>{menu.GetEffectiveControlInfo("Back")} <font color='#FFFFFF'>| <font color='#ff3333'>Exit: <font color='#f5a142'>{menu.GetEffectiveControlInfo("Exit")}</font><br>";
                }
                else
                {
                    controlsInfo = $"<font color='#ff3333' class='fontSize-sm'>Move: <font color='#f5a142'>{menu.GetEffectiveControlInfo("Move")} <font color='#FFFFFF'>| <font color='#ff3333' class='fontSize-m'>Select: <font color='#f5a142'>{menu.GetEffectiveControlInfo("Select")} <font color='#FFFFFF'>| <font color='#ff3333'>Exit: <font color='#f5a142'>{menu.GetEffectiveControlInfo("Exit")}</font><br>";
                }
                builder.Append(controlsInfo);
            }

            player.PrintToCenterHtml(builder.ToString());
        }
        private void UpdateSliderOptionText(StringBuilder builder, LinkedListNode<IT3Option> current, string color)
        {
            T3Option sliderOption = (T3Option)current.Value!;
            if (sliderOption.CustomValues == null || sliderOption.CustomValues.Count == 0)
            {
                builder.Append($"<font color='grey' class='fontSize-m'>{sliderOption.OptionDisplay}: No items</font>");
                return;
            }

            int selectedIndex = sliderOption.GetSelectedIndex();
            int displayCount = sliderOption.DisplayItems;
            int sliderTotalItems = sliderOption.CustomValues.Count;

            int startIdx = Math.Max(0, selectedIndex - (displayCount / 2));
            if (startIdx + displayCount > sliderTotalItems)
                startIdx = Math.Max(0, sliderTotalItems - displayCount);

            int endIdx = Math.Min(sliderTotalItems - 1, startIdx + displayCount - 1);

            StringBuilder sliderContent = new StringBuilder();

            if (selectedIndex > 0)
            {
                if (startIdx > 0)
                    sliderContent.Append("<font color='#FFFF00'>«</font> ");
                else
                    sliderContent.Append("<font color='#FFFF00'>‹</font> ");
            }
            else
            {
                sliderContent.Append("<font color='#888888'>‹</font> ");
            }

            for (int sliderItemIndex = startIdx; sliderItemIndex <= endIdx; sliderItemIndex++)
            {
                string itemColor = (sliderItemIndex == selectedIndex) ? "#9acd32" : "silver";
                sliderContent.Append($"<font color='{itemColor}'>{sliderOption.CustomValues[sliderItemIndex]}</font> ");
            }

            if (selectedIndex < sliderTotalItems - 1)
            {
                if (endIdx < sliderTotalItems - 1)
                    sliderContent.Append("<font color='#FFFF00'>»</font>");
                else
                    sliderContent.Append("<font color='#FFFF00'>›</font>");
            }
            else
            {
                sliderContent.Append("<font color='#888888'>›</font>");
            }
            if (current == CurrentChoice)
            {
                builder.Append($"<font color='{color}' class='fontSize-m'><b>{sliderOption.OptionDisplay}: {sliderContent}</b></font>");
            }
            else
            {
                builder.Append($"<font color='{color}' class='fontSize-m'>{sliderOption.OptionDisplay}: {sliderContent}</font>");
            }
        }
        private int GetIndex(LinkedListNode<IT3Option>? node)
        {
            if (node == null || CurrentMenu == null)
                return -1;

            int index = 0;
            for (var cur = CurrentMenu.Options.First; cur != null; cur = cur.Next)
            {
                if (cur == node)
                    return index;
                index++;
            }
            return -1;
        }
        private bool IsSelectable(IT3Option option)
        {
            if (option == null || option.IsDisabled)
                return false;

            if (option.Type == OptionType.Text)
                return !option.IsDisabled;

            return true;
        }
    }

}