using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections;
using System.Windows.Media.Animation;
using WindowsPhoneUtils.Utils;
using System.Windows.Media;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Automation.Peers;
using System.Diagnostics;

namespace WindowsPhoneUtils.Controls
{
    public partial class ListContentPresenter : UserControl
    {

        public delegate void SelectedIndexChanged(object sender, int index);
        public event SelectedIndexChanged OnSelectedIndexChanged = delegate { };

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.RegisterAttached("ItemTemplate", typeof(DataTemplate),
            typeof(ListContentPresenter),
            new PropertyMetadata(null, new PropertyChangedCallback(ItemTemplateChanged)));

        public static readonly DependencyProperty ItemsSourceProperty =
                    DependencyProperty.Register("ItemsSource", typeof(IList),
                    typeof(ListContentPresenter),
                    new PropertyMetadata(null, new PropertyChangedCallback(ItemsSourceChanged)));

        public static readonly DependencyProperty SelectedIndexProperty =
                    DependencyProperty.Register("SelectedIndex", typeof(int),
                    typeof(ListContentPresenter),
                    new PropertyMetadata(0, new PropertyChangedCallback(SelectedIdxChanged)));

        private static void ItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListContentPresenter instance = (ListContentPresenter)d;
            instance.ItemTemplate = (DataTemplate)e.NewValue;
        }

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListContentPresenter instance = (ListContentPresenter)d;
            instance.ItemsSource = ((IList)e.NewValue);
            instance.SelectedIndex = 0;
            instance.UpdateData();

            if (e.NewValue is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)e.NewValue).CollectionChanged += instance.ListContentPresenter_CollectionChanged;
            }
            if (e.OldValue is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)e.NewValue).CollectionChanged -= instance.ListContentPresenter_CollectionChanged;
            }
        }

        private void ListContentPresenter_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateData();
        }

        private static void SelectedIdxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListContentPresenter instance = (ListContentPresenter)d;
            instance.SelectedIndex = (int)e.NewValue;
        }

        public static void SetItemTemplate(DependencyObject obj, object value)
        {
            obj.SetValue(ItemTemplateProperty, value);

            ListContentPresenter l = (ListContentPresenter)obj;

            l.content0.ContentTemplate = (DataTemplate)value;
            l.content1.ContentTemplate = (DataTemplate)value;
            l.content2.ContentTemplate = (DataTemplate)value;

            l.UpdateData();
        }
        public static object GetItemTemplate(DependencyObject obj)
        {
            return obj.GetValue(ItemTemplateProperty);
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set
            {
                if (SelectedIndex != value)
                {
                    if (ItemsSource == null)
                    {
                        throw new NullReferenceException("ItemsSource is null. Set ItemsSource before changing index.");
                    }
                    if ((SelectedIndex < 0 || SelectedIndex > ItemsSource.Count - 1) && ItemsSource.Count > 0)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    SetValue(SelectedIndexProperty, value);
                    UpdateData();
                }
            }
        }

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set
            {
                if (ItemsSource != value)
                {
                    SetValue(ItemsSourceProperty, value);
                    UpdateData();
                }
            }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set
            {
                if (ItemTemplate != value)
                {
                    SetValue(ItemTemplateProperty, value);
                    content0.ContentTemplate = value;
                    content1.ContentTemplate = value;
                    content2.ContentTemplate = value;

                    UpdateData();
                }

            }
        }

        private ContentControl left;
        private ContentControl center;
        private ContentControl right;

        private double slideLastX;
        private Boolean userManipulating;
        private Storyboard storyboard;
        private int direction;


        public ListContentPresenter()
        {
            InitializeComponent();
            left = content0;
            center = content1;
            right = content2;
        }

        public void PresenterHidden()
        {
            NotifyItemHidden(center);
        }

        public void PresenterShown()
        {
            NotifyItemShown(center);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PresenterShown();
        }

        private void NotifyItemShown(ContentControl c)
        {
            if (c == null)
            {
                return;
            }

            if (VisualTreeHelper.GetChildrenCount(c) > 0)
            {
                DependencyObject d = VisualTreeHelper.GetChild(center, 0);

                if (VisualTreeHelper.GetChildrenCount(d) > 0)
                {
                    d = VisualTreeHelper.GetChild(d, 0);
                    if (d is IListSmartItem)
                    {
                        (d as IListSmartItem).ContentShown();
                    }
                }

            }
        }

        private void NotifyItemHidden(ContentControl c)
        {
            if (c == null)
            {
                return;
            }

            if (VisualTreeHelper.GetChildrenCount(c) > 0)
            {
                DependencyObject d = VisualTreeHelper.GetChild(center, 0);

                if (VisualTreeHelper.GetChildrenCount(d) > 0)
                {
                    d = VisualTreeHelper.GetChild(d, 0);
                    if (d is IListSmartItem)
                    {
                        (d as IListSmartItem).ContentHidden();
                    }
                }

            }
        }

        private void UpdateData()
        {
            if (ItemsSource == null || ItemsSource.Count == 0)
            {
                return;
            }

            if (SelectedIndex > 0)
            {
                left.Visibility = Visibility.Visible;
                left.Content = ItemsSource[SelectedIndex - 1];
            }
            else
            {
                left.Visibility = Visibility.Collapsed;
            }

            center.Content = ItemsSource[SelectedIndex];

            if (SelectedIndex < ItemsSource.Count - 1)
            {
                right.Visibility = Visibility.Visible;
                right.Content = ItemsSource[SelectedIndex + 1];
            }
            else
            {
                right.Visibility = Visibility.Collapsed;
            }
        }

        private void LayoutRoot_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            if (storyboard != null)
            {
                storyboard.Pause();
                storyboard = null;

                Canvas.SetZIndex(center, -1);
            }

            Double manipulationOriginX = (e.OriginalSource as UIElement).TransformToVisual(LayoutRoot).Transform(new Point(0, 0)).X + e.ManipulationOrigin.X;

            userManipulating = true;
        }

        private void LayoutRoot_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            if (!userManipulating)
            {
                return;
            }

            if (ItemsSource == null || ItemsSource.Count == 0)
            {
                return;
            }

            if (SelectedIndex == 0)
            {
                CompositeTransform ct = left.RenderTransform as CompositeTransform;
                slideLastX = (double)ct.GetValue(CompositeTransform.TranslateXProperty) + e.DeltaManipulation.Translation.X;
                slideLastX = Math.Max(slideLastX, -480);
                slideLastX = Math.Min(slideLastX, -480);
                ct.SetValue(CompositeTransform.TranslateXProperty, slideLastX);

                ct = center.RenderTransform as CompositeTransform;
                slideLastX = (double)ct.GetValue(CompositeTransform.TranslateXProperty) + e.DeltaManipulation.Translation.X;
                slideLastX = Math.Max(slideLastX, -480);
                slideLastX = Math.Min(slideLastX, 0);
                ct.SetValue(CompositeTransform.TranslateXProperty, slideLastX);

                ct = right.RenderTransform as CompositeTransform;
                slideLastX = (double)ct.GetValue(CompositeTransform.TranslateXProperty) + e.DeltaManipulation.Translation.X;
                slideLastX = Math.Max(slideLastX, 0);
                slideLastX = Math.Min(slideLastX, 480);
                ct.SetValue(CompositeTransform.TranslateXProperty, slideLastX);
            }
            else if (SelectedIndex == ItemsSource.Count - 1)
            {
                CompositeTransform ct = left.RenderTransform as CompositeTransform;
                slideLastX = (double)ct.GetValue(CompositeTransform.TranslateXProperty) + e.DeltaManipulation.Translation.X;
                slideLastX = Math.Max(slideLastX, -480);
                slideLastX = Math.Min(slideLastX, 0);
                ct.SetValue(CompositeTransform.TranslateXProperty, slideLastX);

                ct = center.RenderTransform as CompositeTransform;
                slideLastX = (double)ct.GetValue(CompositeTransform.TranslateXProperty) + e.DeltaManipulation.Translation.X;
                slideLastX = Math.Max(slideLastX, 0);
                slideLastX = Math.Min(slideLastX, 480);
                ct.SetValue(CompositeTransform.TranslateXProperty, slideLastX);

                ct = right.RenderTransform as CompositeTransform;
                slideLastX = (double)ct.GetValue(CompositeTransform.TranslateXProperty) + e.DeltaManipulation.Translation.X;
                slideLastX = Math.Max(slideLastX, 480);
                slideLastX = Math.Min(slideLastX, 480);
                ct.SetValue(CompositeTransform.TranslateXProperty, slideLastX);
            }
            else
            {
                CompositeTransform ct = left.RenderTransform as CompositeTransform;
                slideLastX = (double)ct.GetValue(CompositeTransform.TranslateXProperty) + e.DeltaManipulation.Translation.X;
                slideLastX = Math.Max(slideLastX, -(480 * 2));
                slideLastX = Math.Min(slideLastX, 0);
                ct.SetValue(CompositeTransform.TranslateXProperty, slideLastX);

                ct = center.RenderTransform as CompositeTransform;
                slideLastX = (double)ct.GetValue(CompositeTransform.TranslateXProperty) + e.DeltaManipulation.Translation.X;
                slideLastX = Math.Max(slideLastX, -480);
                slideLastX = Math.Min(slideLastX, 480);
                ct.SetValue(CompositeTransform.TranslateXProperty, slideLastX);

                ct = right.RenderTransform as CompositeTransform;
                slideLastX = (double)ct.GetValue(CompositeTransform.TranslateXProperty) + e.DeltaManipulation.Translation.X;
                slideLastX = Math.Max(slideLastX, 0);
                slideLastX = Math.Min(slideLastX, 480 * 2);
                ct.SetValue(CompositeTransform.TranslateXProperty, slideLastX);
            }


        }

        private void LayoutRoot_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (userManipulating)
            {
                userManipulating = false;
                UserLetGo();
            }
        }

        private void UserLetGo()
        {
            direction = SelectDirection();

            TimeSpan duration = TimeSpan.FromSeconds(0.5);
            storyboard = new Storyboard();
            storyboard.Duration = duration;

            CreateAnimationForSlide(left, storyboard, direction);
            CreateAnimationForSlide(center, storyboard, direction);
            CreateAnimationForSlide(right, storyboard, direction);

            storyboard.Completed += storyboard_Completed;
            storyboard.Begin();
        }

        private int SelectDirection()
        {
            CompositeTransform ct = center.RenderTransform as CompositeTransform;
            double centerX = (double)ct.GetValue(CompositeTransform.TranslateXProperty);

            if (centerX < -200)
            {
                return 1;
            }
            else if (centerX > 200)
            {
                return -1;
            }

            return 0;
        }

        private int GetElementPositionModifier(FrameworkElement element, int direction)
        {
            if (element == left)
            {
                return (direction == 1 ? 1 : direction == -1 ? 0 : -1);
            }
            else if (element == center)
            {
                return (direction == 1 ? -1 : direction == -1 ? 1 : 0);
            }
            else if (element == right)
            {
                return (direction == 1 ? 0 : direction == -1 ? -1 : 1);
            }

            return 0;
        }

        private void CreateAnimationForSlide(FrameworkElement element, Storyboard storyboard, int direction)
        {
            CompositeTransform ct = element.RenderTransform as CompositeTransform;
            double eX = (double)ct.GetValue(CompositeTransform.TranslateXProperty);

            DoubleAnimation tXAnimation = new DoubleAnimation();
            tXAnimation.EasingFunction = new CubicEase();
            tXAnimation.Duration = storyboard.Duration;
            tXAnimation.FillBehavior = FillBehavior.HoldEnd;
            tXAnimation.From = eX;
            tXAnimation.To = 480 * GetElementPositionModifier(element, direction);

            if (Math.Abs((double)tXAnimation.From - (double)tXAnimation.To) > 480)
            {
                ObjectAnimationUsingKeyFrames vAnimation = new ObjectAnimationUsingKeyFrames();
                vAnimation.KeyFrames.Add(new DiscreteObjectKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = Visibility.Collapsed });
                vAnimation.KeyFrames.Add(new DiscreteObjectKeyFrame() { KeyTime = KeyTime.FromTimeSpan(storyboard.Duration.TimeSpan), Value = Visibility.Visible });
                storyboard.Children.Add(vAnimation);
                Storyboard.SetTarget(vAnimation, element);
                Storyboard.SetTargetProperty(vAnimation, new PropertyPath("(UIElement.Visibility)"));
            }

            storyboard.Children.Add(tXAnimation);

            Storyboard.SetTarget(tXAnimation, element);
            Storyboard.SetTargetProperty(tXAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
        }

        private void storyboard_Completed(object sender, EventArgs e)
        {
            if (direction == 1)
            {
                NotifyItemHidden(center);

                ContentControl center_ = center;
                ContentControl left_ = left;
                ContentControl right_ = right;
                center = right_;
                left = center_;
                right = left_;
                UpdateAfterSlide(SelectedIndex + 1);

                NotifyItemShown(center);
            }
            else if (direction == -1)
            {
                NotifyItemHidden(center);

                ContentControl center_ = center;
                ContentControl left_ = left;
                ContentControl right_ = right;
                center = left_;
                left = right_;
                right = center_;
                UpdateAfterSlide(SelectedIndex - 1);

                NotifyItemShown(center);
            }
        }

        private void UpdateAfterSlide(int index)
        {
            SetValue(SelectedIndexProperty, index);
            OnSelectedIndexChanged(this, index);

            if (SelectedIndex > 0)
            {
                left.Visibility = Visibility.Visible;
                left.Content = ItemsSource[SelectedIndex - 1];
            }
            else
            {
                left.Visibility = Visibility.Collapsed;
            }

            if (SelectedIndex < ItemsSource.Count - 1)
            {
                right.Visibility = Visibility.Visible;
                right.Content = ItemsSource[SelectedIndex + 1];
            }
            else
            {
                right.Visibility = Visibility.Collapsed;
            }
        }

    }
}
