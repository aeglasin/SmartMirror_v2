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

        private Dictionary<String,String> uriMap;
        private Dictionary<String,List<SyndicationItem>> sourceSyndicationListMap;
        private List<SyndicationItem> syndicationList;

        public News_ViewModel()
        {
            TickerLine = "hello world!";

            HeadLine = "<News Headline>";
            TickerLine = "<News Ticker>";

            //Predefined list of news uri , to be replaced with add news source method.
            uriMap = new Dictionary<String, String>();
            uriMap.Add("BBC","http://newsrss.bbc.co.uk/rss/newsonline_uk_edition/front_page/rss.xml");
            uriMap.Add("CNN", "http://rss.cnn.com/rss/edition.rss");
            uriMap.Add("SPIEGEL", "http://www.spiegel.de/schlagzeilen/tops/index.rss");

            sourceSyndicationListMap = new Dictionary<String, List<SyndicationItem>>();
            foreach (String source in uriMap.Keys)
            {
                load(source, new Uri(uriMap[source]));
            }
            
            displayContent();
        }

        private async void load(String source,Uri uri)
        {
            
            SyndicationClient client = new SyndicationClient();
            SyndicationFeed feed = await client.RetrieveFeedAsync(uri);
            await Task.Delay(TimeSpan.FromSeconds(10));

            if (feed != null)
            {
                syndicationList = new List<SyndicationItem>();
                foreach (SyndicationItem item in feed.Items)
                {
                    syndicationList.Add(item);
                }

                sourceSyndicationListMap.Add(source, syndicationList);
                await Task.Delay(TimeSpan.FromSeconds(10));
            }

        }

        public async void displayContent()
        {
            while (sourceSyndicationListMap.Count == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            
            String strFormat = "ddd, MMM dd";
            String summary = "";
            int i = 1;
            foreach (String newsSource in sourceSyndicationListMap.Keys)
            {
                List<SyndicationItem> syndItemList = sourceSyndicationListMap[newsSource];
                foreach (SyndicationItem item in syndItemList)
                {
                    summary = Regex.Replace(item.Summary.Text, "<.*?>", String.Empty);
                    if (summary.Length == 0)
                    {
                        summary = "Please see more details in web view.";
                    }

                    HeadLine = "[" + i + "]" + newsSource + "-" + item.PublishedDate.DateTime.ToString(strFormat) + ":" +
                        item.Title.Text;

                    TickerLine = summary;
                    i++;
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            }


        }
    }
}

