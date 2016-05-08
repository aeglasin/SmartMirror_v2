using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Gmail.v1;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SmartMirror.Common;

namespace SmartMirror.Messenger_Notification.Google
{
    public class Gmail_ViewModel:ViewModelBase
    {
        private UserCredential _credential;
        private GmailService _service;
        private string _messageCount;
        private List<GmailMessage> _messageList;
        private List<GmailMessage> _gmailMesageList;
        private GmailMessage _gmailMessage;

        public delegate void ListChanged(List<GmailMessage> _messageList);
        public ListChanged OnListChanged { get; set; }

        public delegate void OpenEmail(GmailMessage _gmailMessage);
        public OpenEmail OnOpenEmailRequest { get; set; }

        public  Gmail_ViewModel()
        {
            MessageCount = "0";
            BitmapImage bitmap_Gmail = new BitmapImage();
            bitmap_Gmail.UriSource = new Uri("ms-appx:///Messenger_Notification/Google/gmail-icon.png");
            GmailIcon = bitmap_Gmail;
            SignIn = new RelayCommand(OnSignIn);
            ListMessages = new RelayCommand(OnListMessages);
            OpenEmailMessage = new RelayCommand(OnOpenEmailMessage);
            _gmailMesageList = new List<GmailMessage>();


        }

        private async Task AuthenticateAsync()
        {
            string[] scopes = { GmailService.Scope.MailGoogleCom };

            if (_service != null)
                return;

            _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new Uri("ms-appx:///Assets/client_secret.json"),
            scopes, 
            "user", 
            CancellationToken.None);

            var initializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = "SmartMirror",
            };

            _service = new GmailService(initializer);
            OnUnreadMessageCount();
        }

        public void OnOpenEmailMessage()
        {
            GmailMessage = MessageList[1];
        }

        public void OnUnreadMessageCount()
        {
            UsersResource.LabelsResource.GetRequest request = _service.Users.Labels.Get("me", "INBOX");
            Label response = request.Execute();
            MessageCount = response.MessagesUnread.ToString();
        }

        public void OnListMessages()
        {
            if (_service == null)
                return;

            UsersResource.MessagesResource.ListRequest request = _service.Users.Messages.List("me");
            ListMessagesResponse messageList = request.Execute();
            List <Message> messages = new List<Message>(messageList.Messages);
            List<GmailMessage> gmailMessageList = new List<GmailMessage>();
            for (int i = 0; i < 10; i++)
            {
                Message apiMessage = _service.Users.Messages.Get("me", messages[i].Id).Execute();
                GmailMessage gmailMessage = new GmailMessage();
                foreach (var head in apiMessage.Payload.Headers)
                {
                    if (head.Name.Equals("Date"))
                    {
                        if (head.Value.Contains('+'))
                            gmailMessage.DateMessage = head.Value.Remove(head.Value.IndexOf('+'));
                        else if (head.Value.Contains('-'))
                            gmailMessage.DateMessage = head.Value.Remove(head.Value.IndexOf('-'));
                        else
                            gmailMessage.DateMessage = "";

                    }
                    else if (head.Name.Equals("Subject"))
                        gmailMessage.HeadlineMessage = head.Value;
                    else if (head.Name.Equals("From"))
                        gmailMessage.FromMessage = head.Value.Remove(head.Value.IndexOf('<'));
                }
                try
                {
                    byte[] data = Convert.FromBase64String(apiMessage.Payload.Parts[0].Body.Data);
                    gmailMessage.BodyMessage = Encoding.UTF8.GetString(data);
                }
                catch (Exception)
                {
                    gmailMessage.BodyMessage = "";
                }
               
                
                gmailMessageList.Add(gmailMessage);

            }
            MessageList = gmailMessageList;
        }
 
        public async void OnSignIn()
        {
            await AuthenticateAsync();
        }

        public string MessageCount
        {
            get { return _messageCount; }
            set { SetProperty(ref _messageCount, value); }
        }

        public List<GmailMessage> MessageList
        {
            get { return _messageList; }
            set { SetProperty(ref _messageList, value);
                if (OnListChanged != null) 
                    OnListChanged(_messageList); }
        }
        public GmailMessage GmailMessage
        {
            get { return _gmailMessage; }
            set
            {
                SetProperty(ref _gmailMessage, value);
                if (OnListChanged != null)
                    OnOpenEmailRequest(_gmailMessage);
            }
        }

        public ImageSource GmailIcon { get; set; }
        public RelayCommand SignIn { get; private set; }
        public RelayCommand OpenEmailMessage { get; private set; }
        public RelayCommand UnreadMessageCount { get; private set; }
        public RelayCommand ListMessages { get; private set; }

    }
}
