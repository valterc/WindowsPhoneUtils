﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WindowsPhoneUtils.Common
{
    public abstract class DataTemplateSelector : ContentControl
    {
        public abstract DataTemplate SelectTemplate(object item, DependencyObject container);

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            ContentTemplate = SelectTemplate(newContent, this);
        }
    }
}
