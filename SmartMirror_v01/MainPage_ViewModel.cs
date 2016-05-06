using System;
using SmartMirror.Messenger_Notification.Google;
using SmartMirror.Common;
using System.Collections.Generic;
using Google.Apis.Gmail.v1.Data;
using System.Collections.ObjectModel;

using System.Text.RegularExpressions;
using SmartMirror.Auxileriers.Speech;
using Windows.UI.Core;
using SmartMirror.NewsFeed_Notification;

namespace SmartMirror
{
    public class MainPage_ViewModel : ViewModelBase
    {
        /*gmail module properties*/
        public Gmail_ViewModel Gmail_Module { get; set; }
        public Content_ViewModel GmailContent_Module { get; set; }
        private Gmail_View gmail_View;
        Content_View gmailContetnt_View;

        /*News feed module properties*/
        public News_ViewModel News_Module { get; set; }
        private News_View news_View;

        /*Speech recognition properties*/
        private SpeechComponent speechPart;
        private CoreDispatcher dispatcher;
        public Windows.UI.Xaml.Media.Brush indicator { get; set; }

        public MainPage_ViewModel()
        {

            indicator = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
            speechPart = new SpeechComponent();
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            speechPart.commandsGenerated += reactOnSpeech;
            speechPart.sessionsExpired += speechSessionExpired;
            speechPart.startSession();
            indicator = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Green);

            Gmail_Module = new Gmail_ViewModel();
            GmailContent_Module = new Content_ViewModel();
            Gmail_Module.OnListChanged += Gmail_ViewModel_ListChanged;
            Gmail_Module.OnOpenEmailRequest += Gmail_ViewModel_OpenEmail;
            gmail_View = new Gmail_View();
            gmailContetnt_View = new Content_View();
            gmail_View.DataContext = Gmail_Module;

            news_View = new News_View();
            News_Module = new News_ViewModel();
            news_View.DataContext = News_Module;
            //News_Module.displayContent();
            
        }

        private async void speechSessionExpired()
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>{ indicator = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red); });
            //tbd
            speechPart.startSession();

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { indicator = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Green); });
        }

        private async void  reactOnSpeech(string command,string param)
        {

             switch (command)
                {
                    case "showMailList": break; //TBD
                    case "showMails": break; //TBD has param
                    case "closeCalender":break; //TBD
                    case "openCalender":break; //TBD
                    case "openNews":
                    case "closeNews":
                    case "openSpecificNews": //has param
                    case "closeSpecificNews":  //has param
                    default: break;

                }
              await  dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(command+" parameter was: " +param, "Command detected");
                    await messageDialog.ShowAsync();
                });
               
            

            
        }

        public void Gmail_ViewModel_ListChanged(List<GmailMessage> messageList)
        {
            gmailContetnt_View.DataContext = GmailContent_Module;
            GmailContent_Module.MessageList = new ObservableCollection<GmailMessage>(messageList);
        }

        public void Gmail_ViewModel_OpenEmail(GmailMessage gmailMessage)
        {
            gmailContetnt_View.DataContext = GmailContent_Module;
            GmailContent_Module.GmailMessage = gmailMessage;
        }
    }
}
