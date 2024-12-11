using CounterStrikeSharp.API.Core;
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
            CurrentChoice = MainMenu.Options.First;
            MenuStart = CurrentChoice;

            if (Controls_Config.Settings.FreezePlayersInMenu && player != null)
            {
                player.Freeze();
            }

            if (MainMenu.HasSound && player != null)
            {
                player.ExecuteClientCommand("play Ui/buttonrollover.vsnd_c");
            }

            UpdateCenterHtml();
        }

        public void OpenSubMenu(IT3Menu? menu)
        {
            if (menu == null)
            {
                CurrentMenu = MainMenu;
                CurrentChoice = MainMenu?.Options.First;
                MenuStart = CurrentChoice;
                UpdateCenterHtml();
                return;
            }

            CurrentMenu = menu;
            VisibleOptions = menu.Title != "" ? 4 : 5;
            CurrentChoice = menu.Options.First;
            MenuStart = CurrentChoice;

            if (Controls_Config.Settings.FreezePlayersInMenu && player != null)
            {
                player.Freeze();
            }

            if (Controls_Config.Settings.EnableMenuSounds && player != null)
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

            if (Controls_Config.Settings.EnableMenuSounds == true && player != null)
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

            if (Controls_Config.Settings.EnableMenuSounds && player != null)
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
                player.PrintToCenterHtml(" ");
            }
        }

        public void Choose()
        {
            if (player != null && CurrentChoice?.Value != null)
            {
                if (Controls_Config.Settings.EnableMenuSounds == true)
                {
                    player.ExecuteClientCommand("play Ui/buttonrollover.vsnd_c");
                }

                CurrentChoice.Value.OnChoose.Invoke(player, CurrentChoice.Value);
            }

            UpdateCenterHtml();
        }

        public void ScrollUp()
        {
            if (CurrentChoice?.Previous != null)
            {
                do
                {
                    CurrentChoice = CurrentChoice.Previous;
                } while (CurrentChoice?.Value?.Type == OptionType.Text && CurrentChoice?.Previous != null);

                if (CurrentChoice == MenuStart?.Previous && MenuStart?.Previous != null)
                {
                    MenuStart = MenuStart.Previous;
                }

                if (Controls_Config.Settings.EnableMenuSounds == true && player != null)
                {
                    player.ExecuteClientCommand("play Ui/buttonclick.vsnd_c");
                }

                UpdateCenterHtml();
            }
        }

        public void ScrollDown()
        {
            if (CurrentChoice?.Next != null)
            {
                do
                {
                    CurrentChoice = CurrentChoice.Next;
                } while (CurrentChoice?.Value?.Type == OptionType.Text && CurrentChoice?.Next != null);

                var lastVisible = MenuStart;

                for (int i = 0; i < VisibleOptions - 1 && lastVisible?.Next != null; i++)
                {
                    lastVisible = lastVisible.Next;
                }

                if (CurrentChoice == lastVisible?.Next && lastVisible?.Next != null)
                {
                    MenuStart = MenuStart?.Next;
                }

                if (Controls_Config.Settings.EnableMenuSounds == true && player != null)
                {
                    player.ExecuteClientCommand("play Ui/buttonclick.vsnd_c");
                }

                UpdateCenterHtml();
            }
        }

        public void SlideLeft()
        {
            if (CurrentChoice?.Value?.Type == OptionType.Slider)
            {
                T3Option sliderOption = (T3Option)CurrentChoice.Value;
                sliderOption.SliderValue = Math.Max(sliderOption.SliderValue - sliderOption.Step, sliderOption.MinValue);
                sliderOption.OptionDisplay = $"{sliderOption.OptionDisplay?.Split(':')[0]}: {sliderOption.SliderValue}";

                if (Controls_Config.Settings.EnableMenuSounds && player != null)
                {
                    player.ExecuteClientCommand("play Ui/buttonclick.vsnd_c");
                }

                UpdateCenterHtml();
            }
        }

        public void SlideRight()
        {
            if (CurrentChoice?.Value?.Type == OptionType.Slider)
            {
                T3Option sliderOption = (T3Option)CurrentChoice.Value;
                sliderOption.SliderValue = Math.Min(sliderOption.SliderValue + sliderOption.Step, sliderOption.MaxValue);
                sliderOption.OptionDisplay = $"{sliderOption.OptionDisplay?.Split(':')[0]}: {sliderOption.SliderValue}";

                if (Controls_Config.Settings.EnableMenuSounds && player != null)
                {
                    player.ExecuteClientCommand("play Ui/buttonclick.vsnd_c");
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

            string leftArrow = "◄";
            string rightArrow = "►";
            var current = MenuStart;
            for (int i = 0; i < VisibleOptions && current != null; i++)
            {
                string color = (current == CurrentChoice) ? "#9acd32" : "white";
                string optionDisplay = current.Value?.OptionDisplay ?? "";

                string optionText;

                if (current.Value?.Type == OptionType.Text)
                {
                    optionText = $"<font color='{color}' class='fontSize-m'>{optionDisplay}</font>";
                }
                else if (current.Value?.Type == OptionType.Slider)
                {
                    int startValue = Math.Max(current.Value.SliderValue - 1, current.Value.MinValue);
                    int endValue = Math.Min(current.Value.SliderValue + 1, current.Value.MaxValue);

                    var sliderValues = new List<string>();
                    for (int j = startValue; j <= endValue; j++)
                    {
                        if (j == current.Value.SliderValue)
                        {
                            sliderValues.Add($"<b><font color='#ffd700'>{j}</font></b>");
                        }
                        else
                        {
                            sliderValues.Add($"<font color='white'>{j}</font>");
                        }
                    }

                    string sliderDisplay = string.Join(", ", sliderValues);
                    optionText = $"<font color='yellow' class='fontSize-m'>[{sliderDisplay}]</font>";
                }
                else if (current == CurrentChoice)
                {
                    optionText = $"<b><font color='yellow'>{rightArrow}[</font> <font color='{color}' class='fontSize-m'>{optionDisplay}</font> <font color='yellow'>]{leftArrow}</font></b>";
                }
                else
                {
                    optionText = $"<font color='{color}' class='fontSize-m'>{optionDisplay}</font>";
                }

                builder.Append(optionText);
                builder.AppendLine("<br>");
                current = current.Next;
            }

            builder.AppendLine("<br>");

            if (Controls_Config.Settings.ShowDeveloperInfo)
            {
                string developerInfo = "<font class='fontSize-s' color='white'>Developed by <font color='#ff3333'>T3Marius</font></font>";
                builder.Append(developerInfo);
            }

            builder.AppendLine("<br>");

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

    }
}
