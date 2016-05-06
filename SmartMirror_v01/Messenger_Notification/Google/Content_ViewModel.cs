using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartMirror.Common;
using System.Collections.ObjectModel;
using Google.Apis.Gmail.v1.Data;

namespace SmartMirror.Messenger_Notification.Google
{
    public class Content_ViewModel : ViewModelBase
    {
        private ObservableCollection<GmailMessage> _messageList;
        private GmailMessage _gmailMessage;

        public GmailMessage GmailMessage
        {
            get { return _gmailMessage; }
            set { SetProperty(ref _gmailMessage, value); }
        }

        public ObservableCollection<GmailMessage> MessageList
        {
            get { return _messageList; }
            set { SetProperty(ref _messageList, value); }
        }

        public Content_ViewModel()
        {

        }
    }
}
