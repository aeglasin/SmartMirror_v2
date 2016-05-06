using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace SmartMirror.Datetime.Time
{
    public sealed partial class Time : UserControl
    {

        static int hr, min, sec;

        public Time()
        {
            this.InitializeComponent();
            displayTime();
        }

        private async void displayTime()
        {
            hr = DateTime.Now.Hour;
            min = DateTime.Now.Minute;
            sec = DateTime.Now.Second;

            while (true)
            {
                hr = DateTime.Now.Hour;
                min = DateTime.Now.Minute;
                sec = DateTime.Now.Second;

                //FadeAway.Begin();
                if (hr > 12)
                {
                    hr -= 12;
                }
                if (sec % 2 == 0)
                {
                    datetime.Text = hr + ":" + min + ":" + sec;
                    Debug.WriteLine(hr + ":" + min + ":" + sec);
                }
                else
                {
                    datetime.Text = hr + ":" + min + ":" + sec;
                    Debug.WriteLine(hr + ":" + min + ":" + sec);
                }
                await Task.Delay(TimeSpan.FromMilliseconds(950));
            }
        }
    }
}
