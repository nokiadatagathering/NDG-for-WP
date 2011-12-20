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
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace com.comarch.mobile.ndg.BusyIndicator
{
    /// <summary>
    /// Control displays processing bar which indicates that application is busy.
    /// <see cref="ProcessingBar" /> class can be used to simplify control. 
    /// </summary>
    public class BusyIndicatorControl : ContentControl
    {
        /// <summary>
        /// Represents status of current visual state of busy indicator.
        /// </summary>
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            "IsBusy",
            typeof(bool),
            typeof(BusyIndicatorControl),
            new PropertyMetadata(false, (d, e) => ((BusyIndicatorControl)d).OnIsBusyChanged(e)));

        /// <summary>
        /// Represents information displayed when busy indicator is enabled.
        /// </summary>
        public static readonly DependencyProperty BusyTextProperty = DependencyProperty.Register(
            "BusyText",
            typeof(string),
            typeof(BusyIndicatorControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Indicates whether application bar should be hidden when busy indicator is enabled or not.
        /// </summary>
        public static readonly DependencyProperty HideApplicationBarProperty = DependencyProperty.Register(
            "HideApplicationBar",
            typeof(bool),
            typeof(BusyIndicatorControl),
            new PropertyMetadata(true));
        
        /// <summary>
        /// Represents current visual state of busy indicator.
        /// </summary>
        /// <value>Gets/Sets IsBusyProperty dependancy property.</value>
        public bool IsBusy
        {
            get
            {
                return (bool)GetValue(IsBusyProperty);
            }
            set
            {
                SetValue(IsBusyProperty, value);
            }
        }

        /// <summary>
        /// Represents information displayed when busy indicator is enabled.
        /// </summary>
        /// <value>Gets/Sets BusyTextProperty dependancy property.</value>
        public string BusyText
        {
            get
            {
                return (string)GetValue(BusyTextProperty);
            }
            set
            {
                SetValue(BusyTextProperty, value);
            }
        }

        /// <summary>
        /// Indicates whether application bar should be hidden when busy indicator is enabled or not.
        /// </summary>
        /// <value>Gets/Sets HideApplicationBarProperty dependancy property.</value>
        public bool HideApplicationBar
        {
            get
            {
                return (bool)GetValue(HideApplicationBarProperty);
            }
            set
            {
                SetValue(HideApplicationBarProperty, value);
            }
        }

        /// <summary>
        /// Initializes control template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ChangeVisualState(false);

            OnIsBusyChanged(new DependencyPropertyChangedEventArgs());
        }

        private void ChangeVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsBusy ? VisualStates.Visible.ToString() : VisualStates.Hidden.ToString(), useTransitions);
        }

        private void OnIsBusyChanged(DependencyPropertyChangedEventArgs e)
        {
            var phoneApplicationFrame = Application.Current.RootVisual as PhoneApplicationFrame;
            if ((phoneApplicationFrame != null) && (phoneApplicationFrame.Content is PhoneApplicationPage))
            {
                var page = (PhoneApplicationPage)phoneApplicationFrame.Content;
                if ((page.ApplicationBar != null) && HideApplicationBar)
                {
                    page.ApplicationBar.IsVisible = !this.IsBusy;
                }
            }
            this.ChangeVisualState(true);
        }

        /// <summary>
        /// Defines types of control visual states.
        /// </summary>
        public enum VisualStates
        {
            /// <summary>
            /// Indicates that control is visible.
            /// </summary>
            Visible,
            /// <summary>
            /// Indicates that control is hidden.
            /// </summary>
            Hidden,
        }

    }
}
