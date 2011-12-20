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

namespace com.comarch.mobile.ndg.Validation
{
    /// <summary>
    /// Control used for validation process. Contains two visual states that shows if input value matches set <see cref="IValidationRule" /> and displays proper information. Inherits TextBox properties.
    /// </summary>
	public class ValidationControl : TextBox
	{
        /// <summary>
        /// Represents visual state of validation control.
        /// </summary>
        public static readonly DependencyProperty IsValidProperty =
        DependencyProperty.Register("IsValid", typeof(bool), typeof(ValidationControl), new PropertyMetadata(true, new PropertyChangedCallback(ValidationControl.OnIsValidPropertyChanged)));

        /// <summary>
        /// Represents validation rule that is used to set visual state.
        /// </summary>
        public static readonly DependencyProperty ValidationRuleProperty =
        DependencyProperty.Register("ValidationRule", typeof(IValidationRule), typeof(ValidationControl), null);

        /// <summary>
        /// Represents information that is displayed when input does not match validation rule.
        /// </summary>
        public static readonly DependencyProperty ValidationContentProperty =
        DependencyProperty.Register("ValidationContent", typeof(object), typeof(ValidationControl), null);

        /// <summary>
        /// Represents style of ValidationContent.
        /// </summary>
        public static readonly DependencyProperty ValidationContentStyleProperty =
        DependencyProperty.Register("ValidationContentStyle", typeof(Style), typeof(ValidationControl), null);

        /// <summary>
        /// Represents current visual state validation control.
        /// </summary>
        /// <value>Gets/Sets IsValidProperty dependancy property.</value>
        public bool IsValid
        {
            get 
            {
                return (bool)GetValue(IsValidProperty); 
            }
            set 
            {
                if (ValidationRule != null)
                {
                    ValidationContent = ValidationRule.Message;
                }
                SetValue(IsValidProperty, value);
            }
        }

        /// <summary>
        /// Represents style of ValidationContent.
        /// </summary>
        /// <value>Gets/Sets ValidationContentStyleProperty dependancy property.</value>
        public Style ValidationContentStyle
        {
            get 
            { 
                return GetValue(ValidationContentStyleProperty) as Style; 
            }
            set 
            { 
                SetValue(ValidationContentStyleProperty, value);
            }
        }

        /// <summary>
        /// Represents information that is displayed when input does not match validation rule.
        /// </summary>
        /// <value>Gets/Sets ValidationContentProperty dependancy property.</value>
        public object ValidationContent
		{
            get 
            { 
                return GetValue(ValidationContentProperty) as object;
            }
            set 
            { 
                SetValue(ValidationContentProperty, value); 
            }
		}

        /// <summary>
        /// Represents <see cref="IValidationRule" /> that is used to set visual state. 
        /// </summary>
        /// <value>Gets/Sets ValidationRuleProperty dependancy property.</value>
        public IValidationRule ValidationRule
        {
            get 
            { 
                return GetValue(ValidationRuleProperty) as IValidationRule; 
            }
            set 
            {
                SetValue(ValidationRuleProperty, value);
                ValidationContent = ValidationRule.Message;
            }
        }

        /// <summary>
        /// Default construcor, sets DefaultStyleKey.
        /// </summary>
		public ValidationControl()
		{
			DefaultStyleKey = typeof(ValidationControl);
		}

        /// <summary>
        /// Initializes control template.
        /// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
		}

        /// <summary>
        /// Event raised when focus on input data field is lost. Starts validation process.
        /// </summary>
        /// <param name="e"></param>
		protected override void OnLostFocus(RoutedEventArgs e)
		{
            if (ValidationRule != null)
            {
                IsValid = true;
                IsValid = ValidationRule.Validate(Text);
                ValidationContent = ValidationRule.Message;
            }
			base.OnLostFocus(e);
		}

        private void ChangeVisualState(bool useTransitions)
        {
            if (!IsValid)
            {
                VisualStateManager.GoToState(this, "InValid", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Valid", useTransitions);
            }
        }

        private static void OnIsValidPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValidationControl control = d as ValidationControl;
            bool newValue = (bool)e.NewValue;
            control.ChangeVisualState(newValue);
        }
	}
}
