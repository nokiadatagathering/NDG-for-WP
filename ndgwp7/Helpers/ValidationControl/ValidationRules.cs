/* 
    Copyright (C) 2011  Comarch
  
    NDG for WP7 is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either 
    version 2.1 of the License, or (at your option) any later version.
  
    NDG is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.
  
    You should have received a copy of the GNU Lesser General Public
    License along with NDG.  If not, see <http://www.gnu.org/licenses/
*/
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using com.comarch.mobile.ndg.Model.SurveyForms;

namespace com.comarch.mobile.ndg.Validation
{
    /// <summary>
    /// Interface that each validation rule must inherit.
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>
        /// Represents information that is displayed when validation fails.
        /// </summary>
        string Message { get; set; }
        /// <summary>
        /// Validates string input.
        /// </summary>
        /// <param name="input">Subject of validation.</param>
        /// <returns>Returns true if input matches validation rule, in any other case returns false.</returns>
        bool Validate(string input);
    }

    /// <summary>
    /// Inherits IValidationRule. Allows validation by value range of numeric data.
    /// </summary>
    public class RangeValidationRule : IValidationRule
    {
        private double? _max = 0;
        private double? _min = 0;
        private bool _hasMaxRange;
        private bool _hasMinRange;
        /// <summary>
        /// Represents information that is displayed when validation fails.
        /// </summary>
        public string Message { get; set; }


        /// <summary>
        /// Represents information that is displayed when input is empty.
        /// </summary>
        public string EmptyErrorMessage { get; set; }
        /// <summary>
        /// Represents information that is displayed when input reaches max value.
        /// </summary>
        public string MaxReachedMessage { get; set; }
        /// <summary>
        /// Represents information that is displayed when input reaches min value.
        /// </summary>
        public string MinReachedMessage { get; set; }

        private string _regexDecimalPattern = "^-?\\d*(\\.\\d+)?$";
        private string _regexIntegerPattern = "^[-+]?\\d*$";

        /// <summary>
        /// Represents RegexValidationRule used to validate by numeric characters.
        /// </summary>
        public RegexValidationRule RegexRule { get; set; }

        /// <summary>
        /// Creates validation rule.
        /// </summary>
        /// <param name="type">Type of numeric question, integer or decimal.</param>
        /// <param name="min">Min input value.</param>
        /// <param name="max">Max input value.</param>
        public RangeValidationRule(NumericQuestion.Types type, double? min, double? max)
        {
            switch (type)
            {
                case NumericQuestion.Types.DecimalType:
                    RegexRule = new RegexValidationRule(_regexDecimalPattern);
                    break;
                case NumericQuestion.Types.IntegerType:
                    RegexRule = new RegexValidationRule(_regexIntegerPattern);
                    break;
            }
            if (max != null)
            {
                _max = max;
                _hasMaxRange = true;
            }
            if (min != null)
            {
                _min = min;
                _hasMinRange = true;
            }

            Message = EmptyErrorMessage = Languages.AppResources.validationRules_LengthEmptyMessage;
            MaxReachedMessage = Languages.AppResources.validationRules_MaxReachedMessage;
            MinReachedMessage = Languages.AppResources.validationRules_MinReachedMessage;
        }

        /// <summary>
        /// Validates string input by rule defined in constructor. Sets Message property.
        /// </summary>
        /// <param name="input">Subject of validation.</param>
        /// <returns>Returns true if input matches validation rule, in any other case returns false.</returns>
        public bool Validate(string input)
        {
            if (input.Length == 0)
            {
                Message = EmptyErrorMessage;
                return false;
            }
            else if (RegexRule.Validate(input))
            {
                if (_hasMaxRange && (_max < Convert.ToDouble(input)))
                {
                    Message = string.Format(MaxReachedMessage, _max);
                    return false;
                }
                else if (_hasMinRange && (_min > Convert.ToDouble(input)))
                {
                    Message = string.Format(MinReachedMessage, _min);
                    return false;
                }
            }
            else
            {
                Message = RegexRule.Message;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Inherits IValidationRule. Allows validation by length of input data.
    /// </summary>
    public class LengthValidationRule : IValidationRule
    {
        private int _minLength;
        private int _maxLength;

        /// <summary>
        /// Represents information that is displayed when input is empty.
        /// </summary>
        public string EmptyErrorMessage { get; set; }
        /// <summary>
        /// Represents information that is displayed when input reaches max length.
        /// </summary>
        public string MaxErrorMessage { get; set; }
        /// <summary>
        /// Represents information that is displayed when input reaches min length.
        /// </summary>
        public string MinErrorMessage { get; set; }
        /// <summary>
        /// Represents information that is displayed when validation fails.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Creates validation rule.
        /// </summary>
        /// <param name="min">Min input length.</param>
        /// <param name="max">Max input length.</param>
        public LengthValidationRule(int min, int max)
        {
            _minLength = min;
            _maxLength = max;
            Message = EmptyErrorMessage = Languages.AppResources.validationRules_LengthEmptyMessage;
            MaxErrorMessage = Languages.AppResources.validationRules_LengthMaxMessage;
            MinErrorMessage = Languages.AppResources.validationRules_LengthMinMessage;
        }
        /// <summary>
        /// Validates string input by rule defined in constructor. Sets Message property.
        /// </summary>
        /// <param name="input">Subject of validation.</param>
        /// <returns>Returns true if input matches validation rule, in any other case returns false.</returns>
        public bool Validate(string input)
        {
            if (input.Length == 0)
            {
                Message = EmptyErrorMessage;
                return false;
            }
            else
            {
                if (input.Length > _maxLength)
                {
                    Message = string.Format(MaxErrorMessage, _maxLength);
                    return false;
                }
                if (input.Length < _minLength)
                {
                    Message = string.Format(MinErrorMessage, _minLength);
                    return false;
                }
                return true;
            }
        }
    }

    /// <summary>
    /// Inherits IValidationRule. Allows validation by regular expression pattern.
    /// </summary>
    public class RegexValidationRule : IValidationRule
    {

        private string _pattern;
        /// <summary>
        /// Represents information that is displayed when validation fails.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Creates validation rule.
        /// </summary>
        /// <param name="pattern">Pattern used for regular expression.</param>
        public RegexValidationRule(string pattern)
        {
            _pattern = pattern;
            Message = Languages.AppResources.validationRules_RegexValueNotValid;
        }

        /// <summary>
        /// Validates string input by rule defined in constructor.
        /// </summary>
        /// <param name="input">Subject of validation.</param>
        /// <returns>Returns true if input matches validation rule, in any other case returns false.</returns>
        public bool Validate(string input)
        {
            return Regex.IsMatch(input, _pattern);
        }
    }
    /// <summary>
    /// Inherits IValidationRule. Allows validation with multiple conditions.
    /// </summary>
    public class MultipleValidationRule : IValidationRule
    {
        /// <summary>
        /// Represents list of validation rules used for input validation.
        /// </summary>
        public List<IValidationRule> Rules { get; set; }
        /// <summary>
        /// Represents information that is displayed when validation fails.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Initializes all necesarry data members.
        /// </summary>
        public MultipleValidationRule()
        {
            Rules = new List<IValidationRule>();
            Message = Languages.AppResources.validationRules_RegexValueNotValid;
        }

        /// <summary>
        /// Validates string input by rules defined in Rules property.
        /// </summary>
        /// <param name="input">Subject of validation.</param>
        /// <returns>Returns true if input matches all validation rules, in any other case returns false.</returns>
        public bool Validate(string input)
        {
            foreach (var rule in Rules)
            {
                if (!rule.Validate(input))
                {
                    Message = rule.Message;
                    return false;
                }
            }
            return true;
        }
    }
}
