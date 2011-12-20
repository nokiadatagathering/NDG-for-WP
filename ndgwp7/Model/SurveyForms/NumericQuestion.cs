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
using System.ComponentModel;
using System.Xml.Linq;
using com.comarch.mobile.ndg.Validation;

namespace com.comarch.mobile.ndg.Model.SurveyForms
{
    /// <summary>
    /// Initializes all numeric members. Allows to set parent category.
    /// </summary>
    public class NumericQuestion : Question, INotifyPropertyChanged
    {
        /// <summary>
        /// Represents various type of numeric question.
        /// </summary>
        public enum Types
        {
            /// <summary>
            /// Decimal number.
            /// </summary>
            DecimalType,
            /// <summary>
            /// Integer number.
            /// </summary>
            IntegerType
        }
        /// <summary>
        /// Initializes all numeric members. Allows to set parent category.
        /// </summary>
        /// <param name="parent"><see cref="Category"/> instance that question belongs to.</param>
        /// <param name="questionType"><see cref="Types"/> instance that question type belongs to.</param>
        public NumericQuestion(Category parent, Types questionType)
        {
            Parent = parent;
            IsEnabled = true;
            Type = questionType;
            MinValue = null;
            MaxValue = null;
        }
        /// <summary>
        /// Represents instance of <see cref="Category"/> that question belongs to.
        /// </summary>
        public Category Parent { get; private set; }

        /// <summary>
        /// Represents question descriptions.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Represents identification number of question.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Stores min double limit.
        /// </summary>
        public double? MinValue { get; set; }
        /// <summary>
        /// Stores max double limit.
        /// </summary>
        public double? MaxValue { get; set; }

        private bool IsMaxValueSet()
        {
            if (MaxValue != null)
            {
                return true;
            }
            return false;
        }

        private bool IsMinValueSet()
        {
            if (MinValue != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Represents information about type of numeric question.
        /// </summary>
        public Types Type { get; private set; }

        private bool _isCorrectAnswer = true;
        /// <summary>
        /// Represents status that indicates whether question answer is correct or not.
        /// </summary>
        public bool IsCorrectAnswer
        {
            get
            {
                return _isCorrectAnswer;
            }
            set
            {
                _isCorrectAnswer = value;
                RaisePropertyChanged("IsCorrectAnswer");
            }
        }

        private IValidationRule _validationRule = null;
        /// <summary>
        /// Represents instance of <see cref="IValidationRule"/>. Contain rule for numeric question validation.
        /// </summary>
        public IValidationRule ValidationRule
        {
            get
            {
                return _validationRule;
            }
            set
            {
                _validationRule = value;
                RaisePropertyChanged("ValidationRule");
            }
        }

        /// <summary>
        /// Method to assign validation control to ValidationRule field.
        /// </summary>
        public void AssignValidationRule()
        {
            ValidationRule = new RangeValidationRule(Type, MinValue, MaxValue);
        }

        /// <summary>
        /// Represents max length of answer string.
        /// </summary>
        public int Length{ get; set; }
        private bool _isEnabled;
        /// <summary>
        /// Represents status that indicates whether answer is correct or not.
        /// </summary>
        /// <value>Gets/Sets _isEnabled data member.</value>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged("IsEnabled");
            }
        }

        /// <summary>
        /// Represents information about range limit numeric value.
        /// </summary>
        public bool HasRange
        {
            get
            {
                return IsMaxValueSet() || IsMinValueSet();
            }
        }
        /// <summary>
        /// Represents information about range limit which is diplaying on page.
        /// </summary>
        public string Range
        {
            get
            {
                string range = string.Empty;
                if (IsMinValueSet())
                {
                    if (Type == Types.DecimalType)
                        range = string.Format("Min: {0}, ", MinValue);
                    else
                        range = string.Format("Min: {0}, ", (int)MinValue);
                }
                if (IsMaxValueSet())
                {
                    if (Type == Types.DecimalType)
                        range += string.Format("Max: {0}", MaxValue);
                    else
                        range += string.Format("Max: {0}", (int)MaxValue);
                }
                return range;
            }
        }

        private string _answer;
        /// <summary>
        /// Represents answer for date question. Property includes date validation.
        /// </summary>
        /// <value>Gets/Sets _answer data member.</value>
        public string Answer
        {
            get
            {
                return _answer;
            }
            set
            {
                _answer = value;
                RaisePropertyChanged("Answer");
                Parent.Parent.IsResultChanged = true;
            }
        }

        /// <summary>
        /// Adds question result to xml file.
        /// </summary>
        /// <param name="parent">Xml node that contains question data.</param>
        /// <returns>True if result was added successfully, in any other case false. </returns>
        public bool AddResult(XElement parent)
        {
            XElement child;

            if (Type == Types.DecimalType)
            {
                child = new XElement("decimal");
            }
            else
            {
                child = new XElement("int");
            }
            if (IsCorrectAnswer && !string.IsNullOrEmpty(Answer) && IsEnabled)
            {
                double value;
                try
                {
                    value = Double.Parse(Answer);
                }
                catch (FormatException)
                {
                    return false;
                }
                if (Type == Types.DecimalType)
                {
                    child.Value = string.Format("{0}", value);
                }
                else
                {
                    child.Value = string.Format("{0}", (int)value);
                }
            }
            parent.Add(child);
            return (IsCorrectAnswer && !string.IsNullOrEmpty(Answer)) || !IsEnabled;
        }
        /// <summary>
        /// Reads last answer.
        /// </summary>
        /// <param name="parent">Xml node that contains question data.</param>
        public void ReadLastResult(XElement parent)
        {
            XElement answer = null;
            if (Type == Types.DecimalType)
            {
                answer = parent.Element("decimal");
            }
            else
            {
                answer = parent.Element("int");
            }
            _answer = answer.Value;
            RaisePropertyChanged("Answer");
        }
        /// <summary>
        /// Creates a copy of the question.
        /// </summary>
        /// <param name="parent">Instance of <see cref="Category"/> that question belongs to.</param>
        /// <returns>New instance of question.</returns>
        public Question Copy(Category parent)
        {
            return new NumericQuestion(parent,Type) 
            { 
                Id = this.Id, Description = this.Description, 
                Length = this.Length, 
                MaxValue = this.MaxValue, 
                MinValue = this.MinValue, 
            };
        }

        /// <summary>
        /// Triggers when property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string arg)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(arg));
            }
        }
    }
}
