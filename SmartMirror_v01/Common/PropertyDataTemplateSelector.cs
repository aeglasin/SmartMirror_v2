using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SmartMirror.Common
{
    public class PropertyDataTemplateSelector: DataTemplateSelector
    {
        public DataTemplate GmailDataTemplate { get; set; }
        public DataTemplate IdleDataTemplate { get; set; }
        public DataTemplate NewsDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            //string type = item.GetType.ToString();
            ViewModelBase viewModel = item as ViewModelBase;

            if (item != null)
            { 
                string type = viewModel.ToString();
                if (type == "SmartMirror.Messenger_Notification.Google.Content_ViewModel")
                    return GmailDataTemplate;
                else if (type == "SmartMirror.Messenger_Notification.Google.Idle_ViewModel")
                    return IdleDataTemplate;
                
            }
            
            return null;
        }

    }
}
