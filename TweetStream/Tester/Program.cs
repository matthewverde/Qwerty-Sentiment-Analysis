using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using TweetStream;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WindowWidth = 160;

            var searchTerms = new List<string>() { "superbowl" };

            // Create new tweet streamer with search term and a durration of 10 seconds
            Streamer tweetStreamer = new Streamer(searchTerms, 10);

            // will output recieved tweets to console window:
            //tweetStreamer.StartStream();

            // will output recieved tweets to stream writer (.txt file):
            using (StreamWriter sw = new StreamWriter(@"..\..\..\tweets.txt", false))
            {
                // start the stream with the given output file
                tweetStreamer.StartStream(sw);
            }
        }
    }
}
