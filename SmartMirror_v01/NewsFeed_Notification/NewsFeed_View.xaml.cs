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

namespace SmartMirror.NewsFeed_Notification
{
    public sealed partial class NewsFeed_View : UserControl
    {
        private static List<SyndicationItem> list;
        private static List<String> uriList;
        
        public NewsFeed_View()
        {
            this.InitializeComponent();

            newsItem_headline.Text = "<News Headline>";
            //newsItem_summary.Text = "<News Summary>";
            newsItem_ticker.Text = "<News Ticker>";
            FadeAway.Begin();

            uriList = new List<string>();
            uriList.Add("http://newsrss.bbc.co.uk/rss/newsonline_uk_edition/front_page/rss.xml");
            uriList.Add("http://rss.cnn.com/rss/edition.rss");
            uriList.Add("http://www.spiegel.de/schlagzeilen/tops/index.rss");

            list = new List<SyndicationItem>();
            foreach (String uri in uriList)
            {
                load(new Uri(uri));
            }

            displayContent();
        }

        private async void load(Uri uri)
        {
            Debug.WriteLine("Load news feed..");
            SyndicationClient client = new SyndicationClient();
            SyndicationFeed feed = await client.RetrieveFeedAsync(uri);

            await Task.Delay(TimeSpan.FromSeconds(5));

            if (feed != null)
            {
                foreach (SyndicationItem item in feed.Items)
                {
                    list.Add(item);

                }
                await Task.Delay(TimeSpan.FromSeconds(5));
                Debug.WriteLine("Total feed items=" + list.Count);


            }
        }

        private async void displayContent()
        {
            await Task.Delay(TimeSpan.FromSeconds(20));

            String strFormat = "ddd, MMM dd";
            String summary = "";
            String source = "News_Source";
            if (list.Count != 0)
            {
                int i = 1;
                foreach (SyndicationItem item in list)
                {
                    Debug.WriteLine(i + "-" + item.Summary.Text);
                    summary = Regex.Replace(item.Summary.Text, "<.*?>", String.Empty);
                    if (summary.Length == 0)
                    {
                        summary = "Please see more details in web view.";
                    }
                    newsItem_headline.Text
                        = "[" + i + "]" + source + "-" + item.PublishedDate.DateTime.ToString(strFormat) + ":" +
                        item.Title.Text;
                    //newsItem_summary.Text = Environment.NewLine + summary;
                    newsItem_ticker.Text = summary;
                    i++;
                    FadeAway.Begin();
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
                newsItem_headline.Text
                        = "End.";
            }
        }

    }
}
