using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

//using Microsoft.Expression.Interactivity.Core;

namespace BarcodeAppDesktop.Helper
{
    public class TextBoxFilterBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty LegalCharsProperty = DependencyProperty.RegisterAttached(
            "LegalChars", typeof (string), typeof (TextBoxFilterBehavior),
            new PropertyMetadata("1234567890abcdefghijklmnopqrstuvwxyz"));

        public static void SetLegalChars(DependencyObject element, string value)
        {
            element.SetValue(LegalCharsProperty, value);
        }

        public static string GetLegalChars(DependencyObject element)
        {
            return (string) element.GetValue(LegalCharsProperty);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextChanged += TextBoxTextChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.TextChanged -= TextBoxTextChanged;
        }

        private void FilterLegalChars()
        {
            int cursorLocation = AssociatedObject.SelectionStart;
            for (int i = AssociatedObject.Text.Length - 1; i >= 0; i--)
            {
                if (AssociatedObject.Text.Length == 0)
                    return;
                char c = AssociatedObject.Text[i];
                string legalChars = GetLegalChars(this);
                if (legalChars.ToCharArray().ToList().Contains(c))
                    continue;
                AssociatedObject.Text = AssociatedObject.Text.Remove(i, 1);
                cursorLocation--;
            }
            AssociatedObject.SelectionStart = Math.Min(AssociatedObject.Text.Length, Math.Max(0, cursorLocation));
        }


        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterLegalChars();
        }
    }
}