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
    /// Stores descriptive question data.
    /// </summary>
    public class DescriptiveQuestion : Question, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes all data members. Allows to set parent category.
        /// </summary>
        /// <param name="parent"><see cref="Category"/> instance that question belongs to.</param>
        public DescriptiveQuestion(Category parent)
        {
            IsEnabled = true;
            Parent = parent;
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
        /// Represents max length of answer string.
        /// </summary>
        public int Length { get; set; }
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

        private string _answer;
        /// <summary>
        /// Represents answer for descriptive question. 
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

        private IValidationRule _validationRule = null;
        /// <summary>
        /// Represents instance of Validation Rule(inherited from <see cref="IValidationRule"/>).
        /// </summary>
        /// <value>Gets/Sets _validationRule data member.</value>
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
        /// Creates and sets new Validation Rule(inherited from <see cref="IValidationRule"/>).
        /// </summary>
        public void AssignValidationRule()
        {
            RangeValidationRule rule = new RangeValidationRule(NumericQuestion.Types.IntegerType, null, null);
            rule.RegexRule = new RegexValidationRule("");
            ValidationRule = rule;
        }

        private bool _isCorrectAnswer = true;
        /// <summary>
        /// Represents status that indicates whether question answer is correct or not.
        /// </summary>
        /// <value>Gets/Sets _isCorrectAnswer data member.</value>
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

        /// <summary>
        /// Adds question result to xml file.
        /// </summary>
        /// <param name="parent">Xml node that contains question data.</param>
        /// <returns>True if result was added successfully, in any other case false. </returns>
        public bool AddResult(XElement parent)
        {
            XElement child = new XElement("str");
            if (IsCorrectAnswer && !string.IsNullOrEmpty(Answer) && IsEnabled)
                child.Value = Answer;
            parent.Add(child);
            return (IsCorrectAnswer && !string.IsNullOrEmpty(Answer)) || !IsEnabled;
        }

        /// <summary>
        /// Reads last answer.
        /// </summary>
        /// <param name="parent">Xml node that contains question data.</param>
        public void ReadLastResult(XElement parent)
        {
            if (parent.Element("str") != null)
            {
                Answer = parent.Element("str").Value;
            }
        }

        /// <summary>
        /// Creates a copy of the question.
        /// </summary>
        /// <param name="parent">Instance of <see cref="Category"/> that question belongs to.</param>
        /// <returns>New instance of question.</returns>
        public Question Copy(Category parent)
        {
            return new DescriptiveQuestion(parent) { Description = this.Description, Id = this.Id, Length = this.Length } ;
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
