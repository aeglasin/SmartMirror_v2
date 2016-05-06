using System;
using System.Diagnostics;
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

namespace SmartMirror.Datetime.Calendar
{
    public sealed partial class Calendar : UserControl
    {

        public Calendar()
        {
            this.InitializeComponent();
        }

        /*private void CalendarView_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {

            if (args.Item.Date.DayOfWeek == DayOfWeek.Saturday ||
            args.Item.Date.DayOfWeek == DayOfWeek.Sunday)
            {
                args.Item.IsBlackout = true;
            }
        }*/
    }
}
