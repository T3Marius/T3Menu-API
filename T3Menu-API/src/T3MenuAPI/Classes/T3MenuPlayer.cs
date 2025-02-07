﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using Microsoft.Extensions.Localization;
using System.Text;
using T3MenuAPI.Classes;

namespace T3MenuAPI
{
    public class T3MenuPlayer
    {
        public CCSPlayerController? player { get; set; }
        public T3Menu? MainMenu = null;
        public IT3Menu? CurrentMenu = null;

        public IT3Option? CurrentInputOption { get; set; }
        public LinkedListNode<IT3Option>? CurrentChoice = null;
        public LinkedListNode<IT3Option>? MenuStart = null;
        public string CenterHtml = "";
        public int VisibleOptions = 4;
        public static IStringLocalizer? Localizer = null;
        public PlayerButtons Buttons { get; set; }

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

            CurrentChoice = MainMenu.Options.First;
            while (CurrentChoice != null && CurrentChoice.Value.IsDisabled)
            {
                CurrentChoice = CurrentChoice.Next;
            }
            if (CurrentChoice == null)
            {
                CurrentChoice = MainMenu.Options.First;
            }

            if (MainMenu.FreezePlayer && player != null)
            {
                player.Freeze();
            }

            if (MainMenu.HasSound && player != null)
            {
                player.ExecuteClientCommand("play Ui/buttonrollover.vsnd_c");
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
                // Set MenuStart to the beginning so that all options (including text) are visible.
                MenuStart = MainMenu?.Options.First;
                CurrentChoice = MainMenu?.Options.First;
                while (CurrentChoice != null && CurrentChoice.Value.IsDisabled)
                {
                    CurrentChoice = CurrentChoice.Next;
                }
                if (CurrentChoice == null)
                {
                    CurrentChoice = MainMenu?.Options.First;
                }
                UpdateCenterHtml();
                return;
            }

            CurrentMenu = menu;
            VisibleOptions = menu.Title != "" ? 4 : 5;

            MenuStart = menu.Options.First;

            CurrentChoice = menu.Options.First;
            while (CurrentChoice != null && CurrentChoice.Value.IsDisabled)
            {
                CurrentChoice = CurrentChoice.Next;
            }
            if (CurrentChoice == null)
            {
                CurrentChoice = menu.Options.First;
            }

            if (CurrentMenu.FreezePlayer && player != null)
            {
                player.Freeze();
            }

            if (CurrentMenu.HasSound && player != null)
            {
                player.ExecuteClientCommand("play Ui/buttonrollover.vsnd_c");
            }

            UpdateCenterHtml();
        }


        public void CloseSubMenu()
        {
            if (CurrentMenu == null || CurrentMenu == MainMenu)
            {
                CloseAllSubMenus();
                return;
            }

            if (CurrentMenu is T3Menu current && current.ParentMenu != null)
            {
                CurrentMenu = current.ParentMenu;
            }
            else
            {
                CurrentMenu = MainMenu;
            }

            CurrentChoice = CurrentMenu?.Options.First;
            MenuStart = CurrentChoice;

            if (MainMenu!.HasSound && player != null)
            {
                player.ExecuteClientCommand("play Ui/buttonrollover.vsnd_c");
            }
            UpdateCenterHtml();
        }

        public void CloseAllSubMenus()
        {
            if (MainMenu == null)
                return;

            CurrentMenu = MainMenu;
            CurrentChoice = MainMenu.Options.First;
            MenuStart = CurrentChoice;
            VisibleOptions = 4;

            if (CurrentMenu.HasSound && player != null)
            {
                player.ExecuteClientCommand("play Ui/buttonrollover.vsnd_c");
            }

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

            if (player != null)
            {
                player.UnFreeze();
                OpenMainMenu(null);
            }
        }
        public void Choose()
        {
            if (player != null && CurrentChoice?.Value != null && !CurrentChoice.Value.IsDisabled)
            {
                if (MainMenu!.HasSound)
                {
                    player.ExecuteClientCommand($"play {Controls_Config.Sounds.Choose}");
                }

                CurrentChoice.Value.OnChoose.Invoke(player, CurrentChoice.Value);
            }

            UpdateCenterHtml();
        }

        public void ScrollDown()
        {
            if (CurrentMenu == null || CurrentChoice == null)
                return;

            LinkedListNode<IT3Option>? nextSelectable = null;
            var candidate = CurrentChoice;

            while (candidate?.Next != null)
            {
                candidate = candidate.Next;
                if (!candidate.Value.IsDisabled)
                {
                    nextSelectable = candidate;
                    break;
                }
            }

            if (nextSelectable == null)
            {
                candidate = CurrentMenu.Options.First;
                while (candidate != null && candidate != CurrentChoice)
                {
                    if (!candidate.Value.IsDisabled)
                    {
                        nextSelectable = candidate;
                        break;
                    }
                    candidate = candidate.Next;
                }
            }

            // Only update if we found a selectable option.
            if (nextSelectable != null)
            {
                CurrentChoice = nextSelectable;
            }

            AdjustVisibleWindow();

            if (MainMenu!.HasSound && player != null)
            {
                player.ExecuteClientCommand($"play {Controls_Config.Sounds.ScrollDown}");
            }
            UpdateCenterHtml();
        }

        public void ScrollUp()
        {
            if (CurrentMenu == null || CurrentChoice == null)
                return;

            LinkedListNode<IT3Option>? prevSelectable = null;
            var candidate = CurrentChoice;

            while (candidate?.Previous != null)
            {
                candidate = candidate.Previous;
                if (!candidate.Value.IsDisabled)
                {
                    prevSelectable = candidate;
                    break;
                }
            }

            if (prevSelectable == null)
            {
                candidate = CurrentMenu.Options.Last;
                while (candidate != null && candidate != CurrentChoice)
                {
                    if (!candidate.Value.IsDisabled)
                    {
                        prevSelectable = candidate;
                        break;
                    }
                    candidate = candidate.Previous;
                }
            }

            if (prevSelectable != null)
            {
                CurrentChoice = prevSelectable;
            }

            AdjustVisibleWindow();

            if (MainMenu!.HasSound && player != null)
            {
                player.ExecuteClientCommand($"play {Controls_Config.Sounds.ScrollUp}");
            }
            UpdateCenterHtml();
        }

        public void SlideLeft()
        {
            if (CurrentChoice?.Value?.Type == OptionType.Slider)
            {
                T3Option sliderOption = (T3Option)CurrentChoice.Value;

                // Move to the previous value in CustomValues
                int currentIndex = sliderOption.CustomValues!.IndexOf(sliderOption.SliderValue!);
                if (currentIndex > 0) // Ensure we don't go out of bounds
                {
                    sliderOption.SliderValue = sliderOption.CustomValues[currentIndex - 1];
                    sliderOption.OptionDisplay = $"{sliderOption.OptionDisplay?.Split(':')[0]}: {sliderOption.SliderValue}";

                    if (CurrentMenu!.HasSound && player != null)
                    {
                        player.ExecuteClientCommand("play Ui/buttonclick.vsnd_c");
                    }
                }

                UpdateCenterHtml();
            }
        }


        public void SlideRight()
        {
            if (CurrentChoice?.Value?.Type == OptionType.Slider)
            {
                T3Option sliderOption = (T3Option)CurrentChoice.Value;

                // Move to the next value in CustomValues
                int currentIndex = sliderOption.CustomValues!.IndexOf(sliderOption.SliderValue!);
                if (currentIndex < sliderOption.CustomValues.Count - 1) // Ensure we don't go out of bounds
                {
                    sliderOption.SliderValue = sliderOption.CustomValues[currentIndex + 1];
                    sliderOption.OptionDisplay = $"{sliderOption.OptionDisplay?.Split(':')[0]}: {sliderOption.SliderValue}";

                    if (CurrentMenu!.HasSound && player != null)
                    {
                        player.ExecuteClientCommand("play Ui/buttonclick.vsnd_c");
                    }
                }

                UpdateCenterHtml();
            }
        }
        public void UpdateCenterHtml()
        {
            if (player == null || CurrentMenu == null)
            {
                return;
            }

            var builder = new StringBuilder();

            int totalItems = CurrentMenu.Options.Count;
            int currentIndex = 0;
            var node = CurrentMenu.Options.First;

            // Find the index of the current choice
            while (node != null)
            {
                if (node == CurrentChoice)
                {
                    break;
                }
                currentIndex++;
                node = node.Next;
            }

            builder.Append($"<b><font color='red' class='fontSize-m'>{CurrentMenu.Title}</font></b> <font color='yellow' class='fontSize-sm'>{currentIndex + 1}</font>/<font color='orange' class='fontSize-sm'>{totalItems}</font>");
            builder.AppendLine("<br>");

            string leftArrow = Controls_Config.ControlsInfo.LeftArrow;
            string rightArrow = Controls_Config.ControlsInfo.RightArrow;
            string leftBracket = Controls_Config.ControlsInfo.LeftBracket;
            string rightBracket = Controls_Config.ControlsInfo.RightBracket;
            var current = MenuStart;

            for (int i = 0; i < VisibleOptions && current != null; i++)
            {
                string color = (current == CurrentChoice && !current.Value!.IsDisabled) ? "#9acd32" : "white";
                string optionDisplay = current.Value?.OptionDisplay ?? "";
                string optionText;

                if (current.Value!.IsDisabled)
                {
                    // Display disabled options in grey
                    optionText = $"<font color='grey' class='fontSize-m'>{optionDisplay}</font>";
                }
                else if (current.Value?.Type == OptionType.Text)
                {
                    if (current == CurrentChoice)
                    {
                        optionText = $"<font color='#FFFF00'>{rightArrow}{rightBracket} </font><font class='fontSize-m'>{optionDisplay}</font><font color='#FFFF00'> {leftBracket}{leftArrow}</font>";
                    }
                    else
                    {
                        optionText = $"<font color='{color}' class='fontSize-m'>{optionDisplay}</font>";
                    }
                }
                else if (current.Value?.Type == OptionType.Slider)
                {
                    T3Option sliderOption = (T3Option)current.Value;
                    var customValues = sliderOption.CustomValues!;
                    int currentIndexInValues = customValues.IndexOf(sliderOption.SliderValue!);

                    string sliderValues = string.Join(" ", customValues.Select((value, index) =>
                        index == currentIndexInValues
                            ? $"<b><font color='#00FF00'>{value}</font></b>"
                            : $"<font color='#FFFF00'>{value}</font>"));

                    optionText = $"<font color='#FFFF00'>{leftArrow}</font> <font color='#FFFFFF'>{rightBracket}</font> {sliderValues} <font color='#FFFFFF'>{leftBracket}</font> <font color='#FFFF00'>{rightArrow}</font>";
                }
                else if (current == CurrentChoice)
                {
                    optionText = $"<b><font color='yellow'>{rightArrow}{rightBracket}</font> <font color='{color}' class='fontSize-m'>{optionDisplay}</font> <font color='yellow'>{leftBracket}{leftArrow}</font></b>";
                }
                else
                {
                    optionText = $"<font color='{color}' class='fontSize-m'>{optionDisplay}</font>";
                }

                builder.Append(optionText);
                builder.AppendLine("<br>");
                current = current.Next;
            }

            if (current != null)
            {
                builder.AppendLine("<img src='https://raw.githubusercontent.com/ssypchenko/GG1MapChooser/main/Resources/arrow.gif' class=''> <img src='https://raw.githubusercontent.com/ssypchenko/GG1MapChooser/main/Resources/arrow.gif' class=''> <img src='https://raw.githubusercontent.com/ssypchenko/GG1MapChooser/main/Resources/arrow.gif' class=''><br>");
            }
            if (current == null)
            {
                builder.AppendLine("<br>");
            }
            if (Controls_Config.Settings.ShowDeveloperInfo)
            {
                string developerInfo = "<font class='fontSize-s' color='white'>Developed by <font color='#ff3333'>T3Marius</font></font>";
                builder.Append(developerInfo);
                builder.AppendLine("<br>");
            }
            // control info
            string controlsInfo;
            if (CurrentMenu.IsSubMenu)
            {
                controlsInfo = $"<font color='#ff3333' class='fontSize-sm'>Select: <font color='#f5a142'>{Controls_Config.ControlsInfo.Select} <font color='#FFFFFF'>| <font color='#ff3333' class='fontSize-m'>Back: <font color='#f5a142'>{Controls_Config.ControlsInfo.Back} <font color='#FFFFFF'>| <font color='#ff3333'>Exit: <font color='#f5a142'>{Controls_Config.ControlsInfo.Exit}</font><br>";
            }
            else
            {
                controlsInfo = $"<font color='#ff3333' class='fontSize-sm'>Move: <font color='#f5a142'>{Controls_Config.ControlsInfo.Move} <font color='#FFFFFF'>| <font color='#ff3333' class='fontSize-m'>Select: <font color='#f5a142'>{Controls_Config.ControlsInfo.Select} <font color='#FFFFFF'>| <font color='#ff3333'>Exit: <font color='#f5a142'>{Controls_Config.ControlsInfo.Exit}</font><br>";
            }

            builder.Append(controlsInfo);
            player.PrintToCenterHtml(builder.ToString());
        }
        private void AdjustVisibleWindow()
        {
            if (CurrentMenu == null || CurrentChoice == null)
                return;

            int currentIndex = GetIndex(CurrentChoice);
            int menuStartIndex = GetIndex(MenuStart);

            if (currentIndex < menuStartIndex)
            {
                MenuStart = GetNodeAt(currentIndex);
            }
            else if (currentIndex >= menuStartIndex + VisibleOptions)
            {
                MenuStart = GetNodeAt(currentIndex - VisibleOptions + 1);
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
        private LinkedListNode<IT3Option>? GetNodeAt(int index)
        {
            if (CurrentMenu == null)
                return null;
            int currentIndex = 0;
            for (var cur = CurrentMenu.Options.First; cur != null; cur = cur.Next)
            {
                if (currentIndex == index)
                    return cur;
                currentIndex++;
            }
            return null;
        }
    }
}