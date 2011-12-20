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
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace com.comarch.mobile.ndg.Model.SurveyForms
{
    /// <summary>
    /// Stores multiple choice question data.
    /// </summary>
    public class MultipleChoiceQuestion : Question, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes all data members. Allows to set parent category.
        /// </summary>
        /// <param name="parent"><see cref="Category"/> instance that question belongs to.</param>
        public MultipleChoiceQuestion(Category parent)
        {
            Parent = parent;
            CheckBoxItems = new List<CheckBoxItem>();
            IsEnabled = true;
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
        /// Represents identification number of result.
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
        private bool IsCompleted
        {
            get
            {
                bool correct = true;
                foreach (CheckBoxItem item in CheckBoxItems)
                {
                    if (!item.IsCorrect)
                    {
                        correct = false;
                        break;
                    }
                }
                return correct;
            }
        }

        /// <summary>
        /// Represents list of items you can choice from.
        /// </summary>
        public List<CheckBoxItem> CheckBoxItems { get; private set; }

        /// <summary>
        /// Creates a copy of the question.
        /// </summary>
        /// <param name="parent">Instance of <see cref="Category"/> that question belongs to.</param>
        /// <returns>New instance of question.</returns>
        public Question Copy(Category parent)
        {
            MultipleChoiceQuestion question = new MultipleChoiceQuestion(parent) { Description = this.Description, Id = this.Id };
            foreach (CheckBoxItem checkBoxItem in CheckBoxItems)
            {
                question.CheckBoxItems.Add(new CheckBoxItem(checkBoxItem, question));
            }
            return question;
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

        /// <summary>
        /// Adds question result to xml file.
        /// </summary>
        /// <param name="parent">Xml node that contains question data.</param>
        /// <returns>True if result was added successfully, in any other case false. </returns>
        public bool AddResult(XElement parent)
        {
            if (IsEnabled)
            {
                var selectedItems = from selectedItem in CheckBoxItems where selectedItem.IsChecked select selectedItem;
                foreach (CheckBoxItem item in selectedItems)
                {
                    string selectedIndex = string.Format("{0}", CheckBoxItems.IndexOf(item));
                    if (item.IsMoreDetailsEnabled)
                    {
                        XElement other = new XElement("other",new XAttribute("index",selectedIndex));
                        other.Value = item.MoreDetails;
                        parent.Add(other);
                    }
                    else
                    {
                        XElement itemElement = new XElement("item");
                        itemElement.Value =selectedIndex;
                        parent.Add(itemElement);
                    }
                }
            }
            return IsCompleted || !IsEnabled;
        }

        /// <summary>
        /// Reads last answer.
        /// </summary>
        /// <param name="parent">Xml node that contains question data.</param>
        public void ReadLastResult(XElement parent)
        {
            foreach (MultipleChoiceQuestion.CheckBoxItem choiceItem in CheckBoxItems)
            {
                choiceItem.IsChecked = false;
            }
            foreach (XElement item in parent.Elements("item"))
            {
                string strSelected;
                int selected;
                strSelected = item.Value;
                selected = Convert.ToInt32(strSelected);
                CheckBoxItems[selected].IsChecked = true;
            }
            foreach (XElement other in parent.Elements("other"))
            {
                string strSelected;
                int selected;
                strSelected = other.Attribute("index").Value;
                selected = Convert.ToInt32(strSelected);
                CheckBoxItems[selected].IsChecked = true;
                CheckBoxItems[selected].MoreDetails = other.Value;
            }
            RaisePropertyChanged("CheckBoxItems");
        }

        /// <summary>
        /// Stores information of single multiple check box item.
        /// </summary>
        public class CheckBoxItem : INotifyPropertyChanged 
        {
            /// <summary>
            /// Initializes all data members. Allows to set parent question.
            /// </summary>
            /// <param name="parent"><see cref="MultipleChoiceQuestion"/> instance that question belongs to.</param>
            public CheckBoxItem(Question parent)
            {
                MoreDetails = string.Empty;
                Parent = parent;
            }
            /// <summary>
            /// Initializes all data members. Allows to set parent question and copy data from another choice item.
            /// </summary>
            /// <param name="old"><see cref="CheckBoxItem"/> instance that constructor takes data from.</param>
            /// <param name="parent"><see cref="MultipleChoiceQuestion"/> instance that item belongs to.</param>
            public CheckBoxItem(CheckBoxItem old, Question parent)
            {
                Parent = parent;
                IsChecked = old.IsChecked;
                _isMoreDetails = old._isMoreDetails;
                Name = old.Name;
                MoreDetails = string.Empty;
            }

            /// <summary>
            /// Represents instance of <see cref="MultipleChoiceQuestion"/> that item belongs to.
            /// </summary>
            public Question Parent{get;set;}

            /// <summary>
            /// Represents choice item title.
            /// </summary>
            public string Name { get; set; }

            private bool _isChecked;
            /// <summary>
            /// Represents status that indicates whether check box item is selected or not.
            /// </summary>
            /// <value>Gets/Sets _isChecked data member.</value>
            public bool IsChecked 
            {
                get
                {
                    return _isChecked;
                }
                set
                {
                    _isChecked = value;
                    RaisePropertyChanged("IsChecked");
                    if (_isMoreDetails)
                        RaisePropertyChanged("IsMoreDetailsEnabled");
                    Parent.Parent.Parent.IsResultChanged = true;
                }
            }

            private bool _isMoreDetails;
            /// <summary>
            /// Represents status that indicates whether check box item has more details option enabled or not.
            /// </summary>
            /// <value>Gets/Sets _isMoreDetails data member.</value>
            public bool IsMoreDetailsEnabled 
            {
                get
                {
                    return _isMoreDetails && IsChecked;
                }
                set
                {
                    _isMoreDetails = value;
                }
            }

            private string _moreDetails;
            /// <summary>
            /// Represents answer in "more details" field.
            /// </summary>
            /// <value>Gets/Sets _moreDetails data member.</value>
            public string MoreDetails
            {
                get { return _moreDetails; }
                set
                {
                    _moreDetails = value;
                    RaisePropertyChanged("MoreDetails");
                    if (Parent!=null)
                        Parent.Parent.Parent.IsResultChanged = true;
                }
            }

            /// <summary>
            /// Represents status that indicates whether check box item answer is correct or not.
            /// </summary>
            public bool IsCorrect
            {
                get
                {
                    if (IsMoreDetailsEnabled && IsChecked)
                        return !string.IsNullOrEmpty(MoreDetails);
                    else
                        return true;
                }
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
}
