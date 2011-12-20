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
    /// Stores time question data.
    /// </summary>
    public class TimeQuestion : Question, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes all time members. Allows to set parent category.
        /// </summary>
        /// <param name="parent"><see cref="Category"/> instance that question belongs to.</param>
        public TimeQuestion(Category parent)
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
        /// Represents status that indicates whether question answer is correct or not.
        /// </summary>
        public bool IsCorrectAnswer { get; set; }

        /// <summary>
        /// Represents information about time convension.
        /// </summary>
        public TimeConvensions TimeConvension { get; set; }

        private string _answer;
        /// <summary>
        /// Represents answer for time question. Property includes time validation.
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
            string strTime;
            XAttribute convension;

            DateTime timeDT = DateTime.Parse(Answer, new CultureInfo("en-US"));
            if (TimeConvension == TimeConvensions.TwelveConvension)
            {
                if (timeDT.Hour < 12)
                {
                    strTime = string.Format("{0}:{1}", timeDT.Hour, timeDT.Minute);
                    convension = new XAttribute("convention", "am");
                }
                else
                {
                    strTime = string.Format("{0}:{1}", timeDT.Hour - 12, timeDT.Minute);
                    convension = new XAttribute("convention", "pm");
                }
            }
            else
            {
                strTime = string.Format("{0}:{1}", timeDT.Hour, timeDT.Minute);
                convension = new XAttribute("convention", "24");
            }
            if (_isEnabled)
            {
                XElement time = new XElement("time");
                time.Value = strTime;
                parent.Add(time);
            } 
            parent.Add(convension);
            return (IsCorrectAnswer && !string.IsNullOrEmpty(Answer)) || !IsEnabled;
        }
        /// <summary>
        /// Reads last answer value.
        /// </summary>
        /// <param name="parent">Xml node that contains question data.</param>
        public void ReadLastResult(XElement parent)
        {
            XElement time =parent.Element("time"); 
            if (time != null)
            {
                if (TimeConvension == TimeConvensions.TwelveConvension)
                {
                    string convension = parent.Attribute("convention").Value;
                    string strTime = time.Value;
                    string[] tokens = strTime.Split(':');
                    int hour = Convert.ToInt32(tokens[0]);
                    int minute = Convert.ToInt32(tokens[1]);
                    DateTime timeDT;
                    if (convension.Equals("am"))
                    {
                        timeDT = new DateTime(2011, 8, 12, hour, minute, 0);
                    }
                    else
                    {
                        timeDT = new DateTime(2011, 8, 12, hour+12, minute, 0);
                    }
                    Answer = timeDT.ToShortTimeString();
                }
                else
                {
                    string strTime = time.Value;
                    string[] tokens = strTime.Split(':');
                    int hour = Convert.ToInt32(tokens[0]);
                    int minute = Convert.ToInt32(tokens[1]);
                    DateTime timeDT = new DateTime(2011, 8, 12, hour, minute, 0);
                    Answer = timeDT.ToShortTimeString();
                }
            }
        }

        /// <summary>
        /// Creates a copy of the question.
        /// </summary>
        /// <param name="parent">Instance of <see cref="Category"/> that question belongs to.</param>
        /// <returns>New instance of question.</returns>
        public Question Copy(Category parent)
        {
            return new TimeQuestion(parent) 
            { 
                Description = this.Description, 
                Id = this.Id, 
                TimeConvension = this.TimeConvension,
                Answer = DateTime.Now.ToShortTimeString()
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

        /// <summary>
        /// Represents various type of time convensions.
        /// </summary>
        public enum TimeConvensions
        {
            /// <summary>
            /// 24h convension.
            /// </summary>
            TwentyFourConvension,
            /// <summary>
            /// 12h AM/PM convension.
            /// </summary>
            TwelveConvension
        }
    }
}