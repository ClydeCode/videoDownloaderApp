using System;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;

namespace MusicDownloader
{
	public class YoutubeConverterService
	{
		public YoutubeConverterService()
		{
		}

        public async Task<Video> GetVideoInfoAsync(string url)
        {
            var youtube = new YoutubeClient();

            var video = await youtube.Videos.GetAsync(url);

            return video;
        }

		public async Task<bool> DownloadFile(string url, string downloadFilePath, IProgress<double> progress)
		{
            try
            {
                var youtube = new YoutubeClient();

                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);

                // highest bitrate audio stream
                var streamInfo = streamManifest.GetAudioOnlyStreams()
                                                .Where(s => s.Container == Container.Mp4)
                                                .GetWithHighestBitrate();

                var video = await youtube.Videos.GetAsync(url);

                string fileName = video.Title.Replace('?', ' ');

                await youtube.Videos.Streams.DownloadAsync(streamInfo, downloadFilePath + $"/{fileName}.mp3", progress);

                return true;
            }
            catch
            {
                return false;
            }
        }
	}
}

