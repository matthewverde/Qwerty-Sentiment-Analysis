using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Timers;

// REST API
using Tweetinvi;
using Tweetinvi.Core.Credentials;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

// STREAM API
using Tweetinvi.Streaming;
using Stream = Tweetinvi.Stream;

namespace TweetStream
{
    /// <summary>
    /// Simple class for live streaming tweets.
    /// </summary>
    public class Streamer
    {
        /// <summary>
        /// From TweetInvi library. Provides methods for accessing the twitter API.
        /// </summary>
        private IFilteredStream _Stream;

        private bool _Streaming;
        private int _Duration;
        private List<string> _SearchTerms;
        private Timer _Timer;
        private StreamWriter _FileOutput = null;

        // Keys from twitter, required for using their api:
        private string _ConsumerKey = "mlr8uESuVyYdWcbt9d8NHCD1W";
        private string _ConsumerSecret = "RUR3RAcHGC9PWd5siWzdOcYMWYe7A56P34fYok2EjzDgGz8aj5";
        private string _AccessToken = "563989979-tVy1wZXVtgIHbP5oyS70na0DssYtfUPyTvh9syE4";
        private string _AccessTokenSecret = "4B4yT7pzJkWmtb16HAJHkpKgEC1n1nXVrYXKJNC4h9akp";

        /// <summary>
        /// True if this is currently streaming tweets.
        /// </summary>
        public bool StreamingInProgress
        {
            get { return _Streaming; }
        }

        /// <summary>
        /// Get or set the length in seconds to stream tweets.
        /// </summary>
        public int StreamDuration
        {
            get { return _Duration; }
            set
            {
                if (!_Streaming)
                {
                    _Duration = value;
                    _Timer.Interval = 1000 * value;
                }
            }
        }

        /// <summary>
        /// List of search term for incoming tweets.
        /// </summary>
        public List<string> SearchTerms
        {
            get { return _SearchTerms; }
            set
            {
                if (!_Streaming)
                {
                    _SearchTerms = value;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="searchTerms"></param>
        /// <param name="streamDuration"></param>
        public Streamer(IEnumerable<string> searchTerms, int streamDuration)
        {
            _Timer = new Timer(1000 * streamDuration);

            _SearchTerms = new List<string>();

            _Streaming = false;
            _Duration = streamDuration;
            _SearchTerms = searchTerms.ToList();
            
            Auth.SetUserCredentials(_ConsumerKey, _ConsumerSecret, _AccessToken, _AccessTokenSecret);
        }

        /// <summary>
        /// Starts the twitter streamer. If fileOutput is given then the tweets will be output there. Otherwise, they will be output to the console window.
        /// </summary>
        /// <param name="fileOutput"></param>
        public void StartStream(StreamWriter fileOutput = null)
        {
            _Streaming = true;
            _FileOutput = fileOutput;

            _Stream = Stream.CreateFilteredStream();
            _Stream.MatchingTweetReceived += TweetReceived;
            _Stream.AddTweetLanguageFilter(LanguageFilter.English);
            foreach (var term in _SearchTerms)
            {
                _Stream.AddTrack(term);
            }

            _Timer.Elapsed += StopStream;
            _Timer.Start();

            _Stream.StartStreamMatchingAnyCondition();
        }

        /// <summary>
        /// Called when the twitter stream recieves a tweet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TweetReceived(object sender, Tweetinvi.Events.MatchedTweetReceivedEventArgs e)
        {
            string outputText = e.Tweet.Id + "\t" + e.Tweet.TweetLocalCreationDate + "\t" + e.Tweet.CreatedBy + "\t" + e.Tweet.Text;

            Console.WriteLine(outputText);

            if (_FileOutput != null)
            {
                _FileOutput.WriteLine(outputText);
            }
        }

        /// <summary>
        /// Stops the twitter stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopStream(object sender, ElapsedEventArgs e)
        {
            _Timer.Stop();
            _Timer.Elapsed -= StopStream;
            _Streaming = false;

            if (_Stream == null) return;

            _Stream.MatchingTweetReceived -= TweetReceived;
            _Stream.StopStream();
            _Stream = null;

            _FileOutput = null;
        }
    }
}
