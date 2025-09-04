using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Common.Extensions;

namespace SeroGlint.DotNet.Common.ConsoleUtilities
{
    public class MenuBuilder
    {
        private ILogger _logger;
        private List<string> _menuOptions = new List<string>();
        private string _title;
        private string _bodyText;
        private char _lineSeparatorCharacter = '*';
        private int _lineLength = 50;
        private bool _asUnorderedList;

        public MenuBuilder(ILogger logger)
        {
            _logger = logger;
        }

        public MenuBuilder WithTitle(string title)
        {
            _title = title;
            return this;
        }

        public MenuBuilder WithBodyText(string bodyText)
        {
            _bodyText = bodyText;
            return this;
        }

        public MenuBuilder WithLineSeparatorCharacter(char character)
        {
            _lineSeparatorCharacter = character;
            return this;
        }

        public MenuBuilder WithLineLength(int length)
        {
            _lineLength = length;
            return this;
        }

        public MenuBuilder AsUnorderedList(bool asUnorderedList = true)
        {
            _asUnorderedList = asUnorderedList;
            return this;
        }

        public MenuBuilder AddMenuOption(string menuOption)
        {
            if (menuOption.IsNullOrWhitespace())
            {
                _logger?.LogInformation("No menu option provided. Not adding an empty line.");
            }

            if (_menuOptions == null)
            {
                _menuOptions = new List<string>();
            }

            _menuOptions.Add(menuOption);
            return this;
        }

        public MenuBuilder AddMenuOptions(List<string> menuOptions)
        {
            if (menuOptions.Count <= 0)
            {
                _logger?.LogInformation("No menu options provided. Not adding any lines.");
                return this;
            }

            if (_menuOptions == null)
            {
                _menuOptions = new List<string>();
            }

            _menuOptions.AddRange(menuOptions);
            return this;
        }

        public string Build()
        {
            _logger.LogInformation("Building menu based on given options.");
            var paddedTitle = _title.PadLeft((_lineLength + _title.Length) / 2).PadRight(_lineLength);
            var stringBuilder = new StringBuilder();
            
            stringBuilder.AppendLine(new string(_lineSeparatorCharacter, _lineLength));
            stringBuilder.AppendLine(paddedTitle);
            stringBuilder.AppendLine(new string(_lineSeparatorCharacter, _lineLength));

            stringBuilder.AppendLine(string.Empty);
            stringBuilder.AppendLine(_bodyText);
            stringBuilder.AppendLine(string.Empty);

            for (var i = 0; i < _menuOptions.Count; i++)
            {
                var menuOption = _menuOptions[i];

                var optionStarter = _asUnorderedList ? "-" : $"{i + 1}.";
                var renderedLine = $"{optionStarter} {menuOption}";
                stringBuilder.AppendLine(renderedLine.PadRight(_lineLength));
            }

            stringBuilder.AppendLine(new string(_lineSeparatorCharacter, _lineLength));
            stringBuilder.AppendLine(string.Empty);

            _logger.LogInformation("Menu built successfully with {0} options.", _menuOptions.Count);
            return stringBuilder.ToString();
        }
    }
}
