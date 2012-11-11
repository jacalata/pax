using System;
using System.Net;
using System.Windows;
using System.Windows.Documents;
using System.Collections.ObjectModel;
//using TweetSharp;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace PAX7.Model
{

    public class Tweet
    {
     //   TwitterService service;
     //   public ObservableCollection<TwitterStatus> tweetCollection;
        string consumerKey = "PElqE5JVkZn6fz63J4nEA";
        string consumerSecret = "EKVAW1vSWDqjRfHZ87sOI8J7gIRFt6B0tIo2dKofeo";
        string accessToken = "10348432-bFz4TYcR2DF0cMa0vAEGbSrJxypGqCB8wxN2lYrHd";
        string accessSecret = "Ckgl5VbzQqNFUpM1nqpHCdxDWjQt59qqVB9hmMlnec";

      
/*
        public Tweet()
        {
            service = new TwitterService(consumerKey, consumerSecret);
            service.AuthenticateWith(accessToken, accessSecret);

            tweetCollection = new ObservableCollection<TwitterStatus>();

        }
 */

    }
 /*   public class TwitterExtras
    {
        Tweet twit;
        public ObservableCollection<TweetSharp.TwitterStatus> tweetCollection;  

        private string url = @"http://api.twitter.com/1/jacalata/lists/pax-info/statuses.xml?per_page=20";

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            //    twitterBrowser.Navigate(new Uri(twitterListURL, UriKind.Relative));
        }

        public void GetTweets()
        {
            //http get
            WebClient wc = new WebClient();
            wc.DownloadStringAsync(new Uri(url));
            wc.DownloadStringCompleted +=
                new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
            //start a progress bar here
        }

        public void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs args)
        {
            twit = new Tweet(); //initialise twitter service
            tweetCollection = new ObservableCollection<TweetSharp.TwitterStatus>();
                     
      
            tweetCollection.Clear();
            XDocument doc = XDocument.Parse(args.Result);
            var tweets = from tweet in doc.Descendants("status")
                         let stamp = int.Parse(tweet.Attribute("id").ToString())
                         orderby stamp ascending
                         select new TwitterStatus
                         {
                             Text = tweet.Attribute("text").ToString(),
                             User = new TwitterUser
                             {
                                 Id = int.Parse(tweet.Attribute("id").ToString()),
                                 Name = tweet.Attribute("name").ToString(),
                                 ScreenName = tweet.Attribute("screen_name").ToString()
                             },
                             CreatedDate = DateTime.Parse(tweet.Attribute("created_at").ToString())
                         };

            foreach (TweetSharp.TwitterStatus tweet in tweets)
                tweetCollection.Add(tweet);



        }


    }
  * */
}
