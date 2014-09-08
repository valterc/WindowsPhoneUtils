using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindowsPhoneUtils.Controls
{
    public partial class RemoteImage : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static readonly DependencyProperty ImageUrlProperty =
            DependencyProperty.Register("ImageUrl", typeof(String),
            typeof(RemoteImage),
            new PropertyMetadata(String.Empty, new PropertyChangedCallback(ImageUrlChanged)));

        public static readonly DependencyProperty LocalImageProperty =
            DependencyProperty.Register("LocalImage", typeof(String),
            typeof(RemoteImage),
            new PropertyMetadata(String.Empty, new PropertyChangedCallback(LocalImageChanged)));

        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(Stretch),
            typeof(RemoteImage),
            new PropertyMetadata(System.Windows.Media.Stretch.Fill, new PropertyChangedCallback(StretchChanged)));

        private static void ImageUrlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            RemoteImage instance = (RemoteImage)obj;
            String value = (String)args.NewValue;

            instance.Image.Stretch = instance.Stretch;
            instance.ImageLoading = true;
            instance.ImageSource = value;
        }

        private static void LocalImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            RemoteImage instance = (RemoteImage)obj;
            String value = (String)args.NewValue;

            instance.localImage = value;
        }

        private static void StretchChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            RemoteImage instance = (RemoteImage)obj;
            Stretch value = (Stretch)args.NewValue;

            instance.Image.Stretch = value;
        }


        public String ImageUrl
        {
            get
            {
                return (String)GetValue(ImageUrlProperty);
            }
            set
            {
                SetValue(ImageUrlProperty, value);
            }
        }

        public String LocalImage
        {
            get
            {
                return (String)GetValue(LocalImageProperty);
            }
            set
            {
                SetValue(LocalImageProperty, value);
            }
        }

        public Stretch Stretch
        {
            get
            {
                return (Stretch)GetValue(StretchProperty);
            }
            set
            {
                SetValue(StretchProperty, value);
                OnPropertyChanged();
            }
        }



        private String localImage;

        private bool imageLoading;
        public bool ImageLoading
        {
            get { return imageLoading; }
            set
            {
                if (imageLoading != value)
                {
                    imageLoading = value;
                    OnPropertyChanged();
                }

            }
        }

        private String imageSource;
        public String ImageSource
        {
            get { return imageSource; }
            set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    OnPropertyChanged();
                }

            }
        }

        public RemoteImage()
        {
            InitializeComponent();
            imageLoading = true;
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            ImageLoading = false;
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ImageLoading = false;
            ImageSource = localImage;
        }
    }
}
