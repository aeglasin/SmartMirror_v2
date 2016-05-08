using SmartMirror.Common;
using System;
using System.Threading.Tasks;

namespace SmartMirror.Datetime.Timer
{
    public class Timer_ViewModel : ViewModelBase
    {
        
        private string _digitalTimer;
        public string DigitalTimer
        {
            get { return _digitalTimer; }
            set { SetProperty(ref _digitalTimer, value); }
        }

        private int hr, min, sec;
        public Timer_ViewModel()
        {
            displayTime();
        }

        public async void displayTime()
        {
            while (true)
            {
                hr = DateTime.Now.Hour;
                min = DateTime.Now.Minute;
                sec = DateTime.Now.Second;

                if (hr > 12)
                {
                    hr -= 12;
                }
                if (sec % 2 == 0)
                {
                    DigitalTimer = hr + ":" + min.ToString("D2") + ":" + sec.ToString("D2");
                }
                else
                {
                    DigitalTimer = hr + ":" + min.ToString("D2") + ":" + sec.ToString("D2");
                }
                await Task.Delay(TimeSpan.FromMilliseconds(950));
            }
        }

    }
}

