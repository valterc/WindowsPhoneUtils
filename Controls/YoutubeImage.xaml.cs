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
    public partial class YoutubeImage : UserControl, INotifyPropertyChanged
    {
        private const string YoutubeImageUrl = "http://img.youtube.com/vi/{0}/hqdefault.jpg";

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static readonly DependencyProperty YoutubeIdProperty =
            DependencyProperty.Register("YoutubeId", typeof(String),
            typeof(YoutubeImage),
            new PropertyMetadata(String.Empty, new PropertyChangedCallback(YoutubeIdChanged)));

        public static readonly DependencyProperty LocalImageProperty =
            DependencyProperty.Register("LocalImage", typeof(String),
            typeof(YoutubeImage),
            new PropertyMetadata(String.Empty, new PropertyChangedCallback(LocalImageChanged)));

        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(Stretch),
            typeof(YoutubeImage),
            new PropertyMetadata(System.Windows.Media.Stretch.Fill, new PropertyChangedCallback(StretchChanged)));

        private static void YoutubeIdChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            YoutubeImage instance = (YoutubeImage)obj;
            String value = (String)args.NewValue;

            instance.Image.Stretch = instance.Stretch;
            instance.ImageLoading = true;
            instance.ImageSource = String.Format(YoutubeImageUrl, value);
        }

        private static void LocalImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            YoutubeImage instance = (YoutubeImage)obj;
            String value = (String)args.NewValue;

            instance.localImage = value;
        }

        private static void StretchChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            YoutubeImage instance = (YoutubeImage)obj;
            Stretch value = (Stretch)args.NewValue;

            instance.Image.Stretch = value;
        }


        public String YoutubeId
        {
            get
            {
                return (String)GetValue(YoutubeIdProperty);
            }
            set
            {
                SetValue(YoutubeIdProperty, value);
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

        public YoutubeImage()
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
