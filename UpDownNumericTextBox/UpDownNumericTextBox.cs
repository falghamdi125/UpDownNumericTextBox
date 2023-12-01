using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Globalization;

namespace CustomNumericTextBox
{
    public class UpDownNumericTextBox : TextBox
    {
        private decimal stepValue = 1M;
        private decimal maxValue = decimal.MaxValue;
        private decimal minValue = decimal.MinValue;
        private decimal magnificationWithCtrlPressed = 10;

        private static bool isCtrlKeyPressed = false;

        private enum WheelDirection { up = 1, down = -1 };
        static UpDownNumericTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UpDownNumericTextBox),
                                             new FrameworkPropertyMetadata(typeof(TextBox)));
            EventManager.RegisterClassHandler(typeof(UpDownNumericTextBox),
                TextBox.PreviewTextInputEvent, new TextCompositionEventHandler(Mod_PreviewTextInput));
            EventManager.RegisterClassHandler(typeof(UpDownNumericTextBox),
                TextBox.PreviewKeyDownEvent, new KeyEventHandler(Mod_PreviewKeyDown));
            EventManager.RegisterClassHandler(typeof(UpDownNumericTextBox),
                        TextBox.MouseWheelEvent, new MouseWheelEventHandler(Mod_MouseWheel));
            EventManager.RegisterClassHandler(typeof(UpDownNumericTextBox),
                                  TextBox.KeyDownEvent, new RoutedEventHandler(Mod_KeyDown));
            EventManager.RegisterClassHandler(typeof(UpDownNumericTextBox),
                                      TextBox.KeyUpEvent, new RoutedEventHandler(Mod_KeyUp));
        }


        public decimal StepValue
        {
            get { return stepValue; }
            set
            {
                stepValue = value;
            }
        }
        public decimal MaxValue
        {
            get { return maxValue; }
            set {
                maxValue = value;
                if(maxValue < minValue)
                {
                    maxValue = minValue;
                }
            }
        }
        public decimal MinValue
        {
            get { return minValue; }
            set {
                minValue = value;
                if (minValue > maxValue)
                {
                    minValue = maxValue;
                }
            }
        }
        public decimal MagnificationWithCtrlPressed
        {
            get { return magnificationWithCtrlPressed; }
            set
            {
                magnificationWithCtrlPressed = value;
            }
        }
        static void Mod_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }
        static void Mod_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                UpDownNumericTextBox numericControl = sender as UpDownNumericTextBox;
                string currenttext = numericControl.Text;
                string futuretext = currenttext.Insert(numericControl.CaretIndex, e.Text);
                Console.WriteLine(futuretext);
                if (futuretext == "-" && 
                    numericControl.minValue < 0 && 
                    !numericControl.Text.Contains('-') && 
                    numericControl.SelectionStart == 0)
                {
                    e.Handled = false;
                    return;
                }
                decimal futuredecimal = decimal.Parse(futuretext);
                if (futuredecimal > decimal.MaxValue ||
                    futuredecimal > numericControl.MaxValue ||
                    futuredecimal < numericControl.MinValue)
                {
                    throw new Exception();
                }

                e.Handled = false;
            }
            catch (Exception) { e.Handled = true; }

        }
        internal static void Mod_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            WheelDirection wheelDirection = mouseWheelDirectionFinder(e.Delta);
            UpDownNumericTextBox numericControl = sender as UpDownNumericTextBox;
            incrementDriver(numericControl, wheelDirection);
        }
        internal static void Mod_KeyDown(object sender, RoutedEventArgs e)
        {
            KeyEventArgs keyEve = (KeyEventArgs)e;
            UpDownNumericTextBox numericControl = sender as UpDownNumericTextBox;
            if (keyEve.Key == Key.LeftCtrl) isCtrlKeyPressed = true;
            if (keyEve.Key == Key.Up) incrementDriver(numericControl, WheelDirection.up);
            if (keyEve.Key == Key.Down) incrementDriver(numericControl, WheelDirection.down);
        }
        internal static void Mod_KeyUp(object sender, RoutedEventArgs e)
        {
            KeyEventArgs keyEve = (KeyEventArgs)e;
            if (keyEve.Key == Key.LeftCtrl) isCtrlKeyPressed = false;
        }
        private static void incrementDriver(UpDownNumericTextBox numericControl, WheelDirection wheelDirection)
        {
            initializeNullTextBox(numericControl);
            try
            {
                incrementOrDecrement(numericControl, wheelDirection);
            }
            catch (FormatException) { textBoxReset(numericControl); }
        }
        
        private static void incrementOrDecrement(UpDownNumericTextBox numericControl, WheelDirection wheelDirection)
        {
            long stepDirection = (long)wheelDirection;
            decimal val = stepDirection * numericControl.StepValue;
            val = incrementDecrementWithCTRL(numericControl, val);
            if ((decimal.Parse(numericControl.Text) + val) > decimal.MaxValue )
            {
                numericControl.Text = (decimal.MaxValue).ToString();
            } else if ((decimal.Parse(numericControl.Text) + val) > numericControl.maxValue)
            {
                numericControl.Text = (numericControl.maxValue).ToString();
            } else if ((decimal.Parse(numericControl.Text) + val) < numericControl.minValue)
            {
                numericControl.Text = (numericControl.minValue).ToString();
            } else
            {
                decimal decimalVal = decimal.Parse(numericControl.Text) + val;
                numericControl.Text = (decimalVal).ToString();
            }
        }
        private static decimal incrementDecrementWithCTRL(UpDownNumericTextBox numericControl, decimal val)
        {
            return (isCtrlKeyPressed && (decimal.Parse(numericControl.Text) + (numericControl.magnificationWithCtrlPressed * val)) > 0) ? val *= numericControl.magnificationWithCtrlPressed : val;
        }
        private static void initializeNullTextBox(UpDownNumericTextBox numericControl)
        {
            if (String.IsNullOrEmpty(numericControl.Text) || String.IsNullOrWhiteSpace(numericControl.Text)) textBoxReset(numericControl);
        }
        private static WheelDirection mouseWheelDirectionFinder(int delta)
        {
            return ((delta / 120) > 0) ? WheelDirection.up : WheelDirection.down;
        }
        private static void textBoxReset(UpDownNumericTextBox numericControl)
        {
            numericControl.Text = "0";
        }
    }
}
