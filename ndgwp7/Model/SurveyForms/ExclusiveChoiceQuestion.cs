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
    /// Stores exclusive choice question data.
    /// </summary>
    public class ExclusiveChoiceQuestion : Question, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes all data members. Allows to set parent category.
        /// </summary>
        /// <param name="parent"><see cref="Category"/> instance that question belongs to.</param>
        public ExclusiveChoiceQuestion(Category parent)
        {
            ChoiceItems = new List<ChoiceItem>();
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
        /// Represents status that indicates whether question answer is correct or not.
        /// </summary>
        public bool IsCorrectAnswer { get; set; }
        private bool IsCompleted
        {
            get
            {
                bool correct = true;
                bool isChosenOne = false;
                foreach (ChoiceItem item in ChoiceItems)
                {
                    if (!item.IsCorrect)
                    {
                        correct = false;
                        break;
                    }
                    if (item.IsChosen)
                        isChosenOne = true;
                }
                return correct && isChosenOne;
            }
        }

        /// <summary>
        /// Represents list of items you can choice from.
        /// </summary>
        public List<ChoiceItem> ChoiceItems { get; set; }

        /// <summary>
        /// Creates a copy of the question.
        /// </summary>
        /// <param name="parent">Instance of <see cref="Category"/> that question belongs to.</param>
        /// <returns>New instance of question.</returns>
        public Question Copy(Category parent)
        {
            ExclusiveChoiceQuestion copy = new ExclusiveChoiceQuestion(parent) { Description = this.Description, Id = this.Id };
            
            foreach (ChoiceItem item in ChoiceItems)
            {
                copy.ChoiceItems.Add(new ChoiceItem(item, copy));
            }
            return copy;
        }

        /// <summary>
        /// Updates skip logic state.
        /// </summary>
        public void RefershSkipLogic()
        {
            foreach (ChoiceItem item in ChoiceItems)
            {
                item.RaiseSkipLogic();
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

        /// <summary>
        /// Adds question result to xml file.
        /// </summary>
        /// <param name="parent">Xml node that contains question data.</param>
        /// <returns>True if result was added successfully, in any other case false. </returns>
        public bool AddResult(XElement parent)
        {
            if (IsEnabled)
            {
                var selectedItems = from selectedItem in ChoiceItems where selectedItem.IsChosen select selectedItem;
                foreach (ChoiceItem item in selectedItems)
                {
                    string selectedIndex = string.Format("{0}", ChoiceItems.IndexOf(item));
                    if (item.IsMoreDetailsEnabled)
                    {
                        XElement other = new XElement("other", new XAttribute("index", selectedIndex));
                        other.Value = item.MoreDetails;
                        parent.Add(other);
                    }
                    else
                    {
                        XElement itemElement = new XElement("item");
                        itemElement.Value = selectedIndex;
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
            foreach (XElement item in parent.Elements("item"))
            {
                string strSelected;
                int selected;
                strSelected = item.Value;
                selected = Convert.ToInt32(strSelected);
                UnselectAll();
                ChoiceItems[selected].IsChosen = true;
            }
            foreach (XElement other in parent.Elements("other"))
            {
                string strSelected;
                int selected;
                strSelected = other.Attribute("index").Value;
                selected = Convert.ToInt32(strSelected);
                UnselectAll();
                ChoiceItems[selected].IsChosen = true;
                ChoiceItems[selected].MoreDetails = other.Value;
            }
            RaisePropertyChanged("ChoiceItems");
        }
        private void UnselectAll()
        {
            foreach (ChoiceItem item in ChoiceItems)
            {
                item.IsChosen = false;
            }
        }

        /// <summary>
        /// Stores information of single exclusive choice item.
        /// </summary>
        public class ChoiceItem : INotifyPropertyChanged
        {
            /// <summary>
            /// Initializes all data members. Allows to set parent question.
            /// </summary>
            /// <param name="parent"><see cref="ExclusiveChoiceQuestion"/> instance that question belongs to.</param>
            public ChoiceItem(Question parent)
            {
                Parent = parent;
                MoreDetails = string.Empty;
            }

            /// <summary>
            /// Initializes all data members. Allows to set parent question and copy data from another choice item.
            /// </summary>
            /// <param name="oldInstance"><see cref="ChoiceItem"/> instance that constructor takes data from.</param>
            /// <param name="parent"><see cref="ExclusiveChoiceQuestion"/> instance that item belongs to.</param>
            public ChoiceItem(ChoiceItem oldInstance, Question parent)
            {
                Parent = parent;
                Name = oldInstance.Name;
                IsChosen = oldInstance._isChosen;
                _isMoreDetails = oldInstance._isMoreDetails;
                _isSkipLogic = oldInstance._isSkipLogic;
                _skipTo = oldInstance._skipTo;
                _isInverse = oldInstance._isInverse;
                MoreDetails = string.Empty;
            }

            /// <summary>
            /// Represents instance of <see cref="ExclusiveChoiceQuestion"/> that item belongs to.
            /// </summary>
            public Question Parent { get; private set; }

            /// <summary>
            /// Represents status that indicates whether choice item answer is correct or not.
            /// </summary>
            public bool IsCorrect
            {
                get
                {
                    if (IsMoreDetailsEnabled && IsChosen)
                    {
                        return !string.IsNullOrEmpty(MoreDetails);
                    }
                    else return true;
                }
            }

            /// <summary>
            /// Represents choice item title.
            /// </summary>
            public string Name { get; set; }
            private bool _isChosen;
            /// <summary>
            /// Represents status that indicates whether choice item is selected or not.
            /// </summary>
            /// <value>Gets/Sets _isChosen data member.</value>
            public bool IsChosen
            {
                get
                {
                    return _isChosen;
                }
                set
                {
                    _isChosen = value;
                    RaisePropertyChanged("IsChosen");

                    if (_isMoreDetails)
                        RaisePropertyChanged("IsMoreDetailsEnabled");
                    RaiseSkipLogic();
                    Parent.Parent.Parent.IsResultChanged = true;
                    
                }
            }
            private bool _isMoreDetails;
            /// <summary>
            /// Represents status that indicates whether choice item has more details option enabled or not.
            /// </summary>
            /// <value>Gets/Sets _isMoreDetails data member.</value>
            public bool IsMoreDetailsEnabled
            {
                get
                {
                    return _isMoreDetails && IsChosen;
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
                    Parent.Parent.Parent.IsResultChanged = true;
                }
            }

            /// <summary>
            /// Represents name of a group of choice items. 
            /// </summary>
            public string GroupName
            {
                get
                {
                    return string.Format("category{0}question{1}", Parent.Parent.Id, Parent.Id);
                }
            }

            private bool _isSkipLogic;
            private int _skipTo;
            private bool _isInverse;

            /// <summary>
            /// Triggers assigned skip logic.
            /// </summary>
            public void RaiseSkipLogic()
            {
                if (_isSkipLogic && Parent.Parent != null)
                {
                    if (_isInverse)
                    {
                        if (Parent.Parent is NormalCategory)
                            ((NormalCategory)Parent.Parent).HideOrShowQuestions(IsChosen, Parent.Id, _skipTo);
                    }
                    else
                    {
                        if (Parent.Parent is NormalCategory)
                            ((NormalCategory)Parent.Parent).HideOrShowQuestions(!IsChosen, Parent.Id, _skipTo);
                    }
                }
            }

            /// <summary>
            /// Assigns skip logic to current choice item.
            /// </summary>
            /// <param name="skipTo">Question id to skip to.</param>
            /// <param name="isInverse">Indicates whether skip logic should trigger when choice item is checked or unchecked.</param>
            public void SetAsSkipLogic( int skipTo, bool isInverse)
            {
                _isSkipLogic = true;
                _skipTo = skipTo;
                _isInverse = isInverse;
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
