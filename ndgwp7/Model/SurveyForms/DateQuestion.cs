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
using System.Globalization;
using System.Xml.Linq;

namespace com.comarch.mobile.ndg.Model.SurveyForms
{
    /// <summary>
    /// Stores date question data.
    /// </summary>
    public class DateQuestion : Question, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes all data members. Allows to set parent category.
        /// </summary>
        /// <param name="parent"><see cref="Category"/> instance that question belongs to.</param>
        public DateQuestion(Category parent)
        {
            IsEnabled = true;
            IsCorrectAnswer = true;
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
        /// Represents instance of <see cref="DateTime"/> class. Stores max date limit.
        /// </summary>
        public DateTime MaxDate { get; set; }
        /// <summary>
        /// Represents instance of <see cref="DateTime"/> class. Stores min date limit.
        /// </summary>
        public DateTime MinDate { get; set; }

        private bool IsMaxDateSet()
        {
            return MaxDate != DateTime.MinValue;
        }

        private bool IsMinDateSet()
        {
            return MinDate != DateTime.MinValue;
        }

        /// <summary>
        /// Represents status that indicates whether question has date limits or not.
        /// </summary>
        public bool HasRange
        {
            get
            {
                return IsMaxDateSet() || IsMinDateSet();
            }
        }
        /// <summary>
        /// Calculates date range of allowed answers.
        /// </summary>
        public string Range
        {
            get
            {
                string range = string.Empty;
                if (IsMinDateSet())
                {
                    range = string.Format("Min: {0}, ", MinDate.ToShortDateString());
                }
                if (IsMaxDateSet())
                {
                    range += string.Format("Max: {0}", MaxDate.ToShortDateString());
                }
                return range;
            }
        }
        private bool _isNotValid;
        /// <summary>
        /// Represents inverted status that indicates whether question answer is valid or not.
        /// </summary>
        /// <value>Gets/Sets _isNotValid data member.</value>
        public bool IsNotValid
        {
            get
            {
                return _isNotValid;
            }
            private set
            {
                _isNotValid = value;
                IsCorrectAnswer = !value;
                RaisePropertyChanged("IsNotValid");
            }
        }

        /// <summary>
        /// Represents status that indicates whether question answer is correct or not.
        /// </summary>
        public bool IsCorrectAnswer { get; set; }

        private string _validationError;
        /// <summary>
        /// Represents information displayed when date validation fails.
        /// </summary>
        /// <value>Gets/Sets _validationError data member.</value>
        public string ValidationError
        {
            get
            {
                return _validationError;
            }
            set
            {
                _validationError = value;
                RaisePropertyChanged("ValidationError");
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
                DateTime selectedDate;
                try
                {
                    selectedDate = DateTime.Parse(value, new CultureInfo("en-US"));
                }
                catch (FormatException)
                {
                    return;
                }
                    IsNotValid = false;
                if (IsMaxDateSet())
                {
                    if (DateTime.Compare(MaxDate, selectedDate) < 0)
                    {
                        IsNotValid = true;
                        ValidationError = string.Format(Languages.AppResources.dateQuestion_MaxDateReached, MaxDate.ToShortDateString());
                    }
                }
                if (IsMinDateSet())
                {
                    if (DateTime.Compare(MinDate, selectedDate) > 0)
                    {
                        IsNotValid = true;
                        ValidationError = string.Format(Languages.AppResources.dateQuestion_MinDateReached, MinDate.ToShortDateString());
                    }
                }
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
            XElement child = new XElement("date");
            DateOperations operationsOnDate = new DateOperations();
            DateTime selectedDate = DateTime.Now;
            if (IsCorrectAnswer && !string.IsNullOrEmpty(Answer) && IsEnabled)
            {
                //selectedDate = DateTime.Parse(_answer, CultureInfo.CurrentCulture);
                selectedDate = DateTime.Parse(_answer, new CultureInfo("en-US"));
            }
                child.Value = operationsOnDate.DateTimeToMiliseconds(selectedDate).ToString();
            parent.Add(child);
            return (IsCorrectAnswer && !string.IsNullOrEmpty(Answer)) || !IsEnabled;
        }

        /// <summary>
        /// Reads last answer.
        /// </summary>
        /// <param name="parent">Xml node that contains question data.</param>
        public void ReadLastResult(XElement parent)
        {
            if (!string.IsNullOrEmpty(parent.Element("date").Value))
            {
                DateOperations operationsOnDate = new DateOperations();
                string strDate = parent.Element("date").Value;
                DateTime date = operationsOnDate.MilisecondsToDateTime(Convert.ToInt64(strDate));
                Answer = date.ToLongDateString();
                RaisePropertyChanged("Answer");
            }
        }

        /// <summary>
        /// Creates a copy of the question.
        /// </summary>
        /// <param name="parent">Instance of <see cref="Category"/> that question belongs to.</param>
        /// <returns>New instance of question.</returns>
        public Question Copy(Category parent)
        {
            return new DateQuestion(parent) { Description = this.Description, Id = this.Id, MaxDate = this.MaxDate, MinDate = this.MinDate, Answer = DateTime.Now.ToShortDateString() }; 
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