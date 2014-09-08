using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WindowsPhoneUtils.Utils;

namespace WindowsPhoneUtils.Controls
{
    public partial class SlidingMenu : UserControl
    {

        public delegate void OnOpen(object sender, Boolean isOpen);
        public event OnOpen IsOpenChanged = delegate { };

        public static readonly DependencyProperty MenuContentProperty =
            DependencyProperty.RegisterAttached("MenuContent", typeof(Object),
            typeof(SlidingMenu),
            new PropertyMetadata(null, new PropertyChangedCallback(MenuContentChanged)));

        public static readonly DependencyProperty DataContentProperty =
            DependencyProperty.RegisterAttached("DataContent", typeof(Object),
            typeof(SlidingMenu),
            new PropertyMetadata(null, new PropertyChangedCallback(DataContentChanged)));

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(Boolean),
            typeof(SlidingMenu),
            new PropertyMetadata(false, new PropertyChangedCallback(IsOpenChanged_)));

        public static readonly DependencyProperty TouchControlProperty =
            DependencyProperty.Register("TouchControl", typeof(Boolean),
            typeof(SlidingMenu),
            new PropertyMetadata(false, new PropertyChangedCallback(TouchControlChanged)));

        private static void MenuContentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            SlidingMenu instance = (SlidingMenu)obj;
            instance.menu.Content = args.NewValue;
        }

        private static void DataContentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            SlidingMenu instance = (SlidingMenu)obj;
            instance.content.Content = args.NewValue;
        }

        private static void IsOpenChanged_(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            SlidingMenu instance = (SlidingMenu)obj;
            instance.IsOpen = (Boolean)args.NewValue;
        }

        private static void TouchControlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            SlidingMenu instance = (SlidingMenu)obj;
            instance.TouchControl = (Boolean)args.NewValue;
        }

        public static void SetDataContent(DependencyObject obj, object value)
        {
            obj.SetValue(DataContentProperty, value);
        }
        public static object GetDataContent(DependencyObject obj)
        {
            return obj.GetValue(DataContentProperty);
        }

        public static void SetMenuContent(DependencyObject obj, object value)
        {
            obj.SetValue(MenuContentProperty, value);
        }
        public static object GetMenuContent(DependencyObject obj)
        {
            return obj.GetValue(MenuContentProperty);
        }

        private enum SlidingDirection
        {
            Left,
            Right
        }

        public Boolean IsOpen
        {
            get
            {
                return (Boolean)GetValue(IsOpenProperty);
            }
            set
            {
                if (value != (Boolean)GetValue(IsOpenProperty))
                {
                    SetValue(IsOpenProperty, value);
                    IsOpenChanged(this, value);

                    if (!userManipulating)
                    {

                        CompositeTransform ct = content.RenderTransform as CompositeTransform;
                        slideLastX = (double)ct.GetValue(CompositeTransform.TranslateXProperty);

                        if (value)
                        {
                            direction = SlidingDirection.Right;
                            UserLetGo();
                        }
                        else
                        {
                            direction = SlidingDirection.Left;
                            UserLetGo();
                        }
                    }
                }
            }
        }

        public Object MenuContent
        {
            get
            {
                return GetValue(MenuContentProperty);
            }
            set
            {
                SetValue(MenuContentProperty, value);
            }
        }

        public Object DataContent
        {
            get
            {
                return GetValue(DataContentProperty);
            }
            set
            {
                SetValue(DataContentProperty, value);
            }
        }

        public Boolean TouchControl
        {
            get
            {
                return (Boolean)GetValue(TouchControlProperty);
            }
            set
            {
                if (value != TouchControl)
                {
                    SetValue(TouchControlProperty, value);
                }
            }
        }

        private double slideLastX;
        private Boolean userManipulating;
        private SlidingDirection direction;
        private Storyboard storyboard;

        public SlidingMenu()
        {
            TouchControl = true;
            InitializeComponent();
            direction = SlidingDirection.Right;
        }

        private void LayoutRoot_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            if (!TouchControl)
            {
                return;
            }

            Boolean storyboardWasRunning = false;
            if (storyboard != null)
            {
                storyboardWasRunning = true;
                storyboard.Pause();
                storyboard = null;
            }

            Double manipulationOriginX = (e.OriginalSource as UIElement).TransformToVisual(LayoutRoot).Transform(new Point(0, 0)).X + e.ManipulationOrigin.X;

            if (storyboardWasRunning ||
                (!IsOpen && manipulationOriginX < 100) ||
                (IsOpen && manipulationOriginX > ScreenUtils.ScreenSize().Width - 100))
            {
                //e.Handled = true;
                //TODO: Test Handling event
                CompositeTransform ct = content.RenderTransform as CompositeTransform;
                slideLastX = (double)ct.GetValue(CompositeTransform.TranslateXProperty);
                userManipulating = true;
            }

        }

        private void LayoutRoot_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            if (!TouchControl)
            {
                return;
            }

            if (!userManipulating)
            {
                return;
            }

            //e.Handled = true;

            if (e.DeltaManipulation.Translation.X > 0)
            {
                direction = SlidingDirection.Right;
            }
            else if (e.DeltaManipulation.Translation.X < 0)
            {
                direction = SlidingDirection.Left;
            }

            CompositeTransform ct = content.RenderTransform as CompositeTransform;
            slideLastX = (double)ct.GetValue(CompositeTransform.TranslateXProperty) + e.DeltaManipulation.Translation.X;
            slideLastX = Math.Max(slideLastX, 0);
            ct.SetValue(CompositeTransform.TranslateXProperty, slideLastX);
            colorLayer.Opacity = Math.Min(Math.Max((1 - slideLastX / 480), 0), 0.8);
        }

        private void LayoutRoot_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (!TouchControl)
            {
                return;
            }

            if (userManipulating)
            {
                //e.Handled = true;
                userManipulating = false;
                UserLetGo();
            }
        }

        private void UserLetGo()
        {
            TimeSpan duration = TimeSpan.FromSeconds(0.5);
            storyboard = new Storyboard();
            storyboard.Duration = duration;

            DoubleAnimation tXAnimation = new DoubleAnimation();
            tXAnimation.EasingFunction = new CubicEase();
            tXAnimation.Duration = duration;
            tXAnimation.FillBehavior = FillBehavior.HoldEnd;
            tXAnimation.From = slideLastX;

            DoubleAnimation oAnimation = new DoubleAnimation();
            oAnimation.Duration = duration;
            oAnimation.EasingFunction = new CubicEase();
            oAnimation.From = colorLayer.Opacity;

            switch (direction)
            {
                case SlidingDirection.Left:
                    tXAnimation.To = 0;
                    oAnimation.To = .8f;
                    break;
                case SlidingDirection.Right:
                    tXAnimation.To = ScreenUtils.ScreenSize().Width - 30;
                    oAnimation.To = 0f;
                    break;
                default:
                    break;
            }

            storyboard.Children.Add(tXAnimation);
            storyboard.Children.Add(oAnimation);

            Storyboard.SetTarget(tXAnimation, content);
            Storyboard.SetTarget(oAnimation, colorLayer);
            Storyboard.SetTargetProperty(tXAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            Storyboard.SetTargetProperty(oAnimation, new PropertyPath("(UIElement.Opacity)"));

            storyboard.Completed += storyboard_Completed;
            storyboard.Begin();
        }

        private void storyboard_Completed(object sender, EventArgs e)
        {
            storyboard = null;
            SetIsOpen(direction == SlidingDirection.Right);
        }

        private void SetIsOpen(Boolean value)
        {
            if (IsOpen != value)
            {
                IsOpenChanged(this, value);
            }
            SetValue(IsOpenProperty, value);

            //Debug.WriteLine("IsOpen::" + IsOpen);
        }

    }
}
