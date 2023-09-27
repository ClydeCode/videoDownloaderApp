using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using Xamarin.Essentials;
using System;
using System.IO;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.Linq;
using System.Threading.Tasks;

namespace MusicDownloader
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            EnsurePermissions();
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            ImageView thumbnail = FindViewById<ImageView>(Resource.Id.thumbnail);
            Button downloadBtn = FindViewById<Button>(Resource.Id.downloadBtn);

            downloadBtn.Click += OnClickEvent;     
        }

        async void OnClickEvent(object sender, EventArgs e)
        {
            string url = await Clipboard.GetTextAsync();

            if (url != null)
            {
                await Task.Run(async () =>
                {
                    var downloadedFilePath = Android.OS.Environment.ExternalStorageDirectory.Path + "/Download/MusicDownloader"; ;

                    Directory.CreateDirectory(downloadedFilePath);

                    var youtube = new YoutubeClient();

                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);

                    // highest bitrate audio stream
                    var streamInfo = streamManifest.GetAudioOnlyStreams()
                                                    .Where(s => s.Container == Container.Mp4)
                                                    .GetWithHighestBitrate();

                    var video = await youtube.Videos.GetAsync(url);
                    string fileName = video.Title.Replace('?', ' ');

                    await youtube.Videos.Streams.DownloadAsync(streamInfo, downloadedFilePath + $"/{fileName}.mp3");
                });
            }
        }

        async void EnsurePermissions()
        {
            var readPerm = Permissions.CheckStatusAsync<Permissions.StorageRead>().Result;
            var writePerm = Permissions.CheckStatusAsync<Permissions.StorageWrite>().Result;

            if (readPerm == PermissionStatus.Denied) 
                await Permissions.RequestAsync<Permissions.StorageRead>();
            if (writePerm == PermissionStatus.Denied) 
                await Permissions.RequestAsync<Permissions.StorageWrite>();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}