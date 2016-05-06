using SmartMirror.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Web.Syndication;

namespace SmartMirror.NewsFeed_Notification
{
    public class News_ViewModel : ViewModelBase
    {
        private string _tickerLine;
        private string _headLine;
        public string TickerLine
        {
            get { return _tickerLine; }
            set { SetProperty(ref _tickerLine, value); }
        }
        public string HeadLine
        {
            get { return _headLine; }
            set { SetProperty(ref _headLine, value); }
        }

        private List<String> uriList;
        private List<SyndicationItem> syndicationList;

        public News_ViewModel()
        {
            TickerLine = "hello world!";

            HeadLine = "<News Headline>";
            TickerLine = "<News Ticker>";

            uriList = new List<string>();
            uriList.Add("http://newsrss.bbc.co.uk/rss/newsonline_uk_edition/front_page/rss.xml");
            uriList.Add("http://rss.cnn.com/rss/edition.rss");
            uriList.Add("http://www.spiegel.de/schlagzeilen/tops/index.rss");

            syndicationList = new List<SyndicationItem>();
            foreach (String uri in uriList)
            {
                load(new Uri(uri));
            }
            displayContent();
        }

        private async void load(Uri uri)
        {
            SyndicationClient client = new SyndicationClient();
            SyndicationFeed feed = await client.RetrieveFeedAsync(uri);

            await Task.Delay(TimeSpan.FromSeconds(5));

            if (feed != null)
            {
                foreach (SyndicationItem item in feed.Items)
                {
                    syndicationList.Add(item);

                }
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        public async void displayContent()
        {
            await Task.Delay(TimeSpan.FromSeconds(10));

            String strFormat = "ddd, MMM dd";
            String summary = "";
            String source = "News_Source";
            if (syndicationList.Count != 0)
            {
                int i = 1;
                foreach (SyndicationItem item in syndicationList)
                {
                    summary = Regex.Replace(item.Summary.Text, "<.*?>", String.Empty);
                    if (summary.Length == 0)
                    {
                        summary = "Please see more details in web view.";
                    }
                    HeadLine = "[" + i + "]" + source + "-" + item.PublishedDate.DateTime.ToString(strFormat) + ":" +
                        item.Title.Text;
                    TickerLine = summary;
                    i++;
                    //FadeAway.Begin();
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            }
        }
    }
}

