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
using System.Globalization;
using System.Windows.Data;
using com.comarch.mobile.ndg.Model.SurveyForms;

namespace com.comarch.mobile.ndg.ViewModel
{
    /// <summary>
    /// Converter which changes selected item in survey to list item or sets "---" string if none item was selected.
    /// </summary>
    public class ChoiceItemsToListItemsConverter : IValueConverter
    {
        /// <summary>
        /// Class stores properties used in choice items to list of selected items.
        /// </summary>
        public class ListItem
        {
            /// <summary>
            /// Selected answer name.
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// String contains more details information.
            /// </summary>
            public string MoreDetails { get; set; }
            /// <summary>
            /// Information about more details visibility.
            /// </summary>
            public bool ShowMoreDetails { get; set; }
        }

        /// <summary>
        /// Creates list of selected items. If none of item was selected returns "---" string.
        /// </summary>
        /// <param name="value">List of answers in survey question with information which item was selected by user.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns list of (only) selected items or "---" string when none of item was selected.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            List<ListItem> itemsToList = new List<ListItem>();
            if (value is List<ExclusiveChoiceQuestion.ChoiceItem>)
            {
                List<ExclusiveChoiceQuestion.ChoiceItem> items = value as List<ExclusiveChoiceQuestion.ChoiceItem>;
                foreach (ExclusiveChoiceQuestion.ChoiceItem item in items)
                {
                    if (item.IsChosen)
                    {
                        ListItem listItem = new ListItem() { Title = item.Name };
                        if (item.IsMoreDetailsEnabled)
                        {
                            listItem.ShowMoreDetails = true;
                            if (String.IsNullOrEmpty(item.MoreDetails))
                                listItem.MoreDetails = "---";
                            else
                                listItem.MoreDetails = item.MoreDetails;
                        }
                        itemsToList.Add(listItem);
                    }
                }
            }
            else if (value is List<MultipleChoiceQuestion.CheckBoxItem>)
            {
                List<MultipleChoiceQuestion.CheckBoxItem> items = value as List<MultipleChoiceQuestion.CheckBoxItem>;
                foreach (MultipleChoiceQuestion.CheckBoxItem item in items)
                {
                    if (item.IsChecked)
                    {
                        ListItem listItem = new ListItem() { Title = item.Name };
                        if (item.IsMoreDetailsEnabled)
                        {
                            listItem.ShowMoreDetails = true;
                            if (String.IsNullOrEmpty(item.MoreDetails))
                                listItem.MoreDetails = "---";
                            else
                                listItem.MoreDetails = item.MoreDetails;
                        }
                        itemsToList.Add(listItem);
                    }
                }
            }

            if (itemsToList.Count == 0)
            {
                itemsToList.Add(new ListItem() { Title = "---" });
            }
            return itemsToList;
        }

        /// <summary>
        /// This converter is never used but must be defined.
        /// </summary>
        /// <param name="value">List of selected items.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns null value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
