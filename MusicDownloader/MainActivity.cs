using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using Xamarin.Essentials;
using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using System.Net;

namespace MusicDownloader
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private readonly string downloadFilePath = Android.OS.Environment.ExternalStorageDirectory.Path + "/Download/MusicDownloader";

        private YoutubeConverterService _youtubeConverterService;

        private ImageView _thumbnail;
        private ProgressBar _progressBar;
        private Button _downloadBtn;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            _youtubeConverterService = new YoutubeConverterService();

            EnsurePermissions();
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            _thumbnail = FindViewById<ImageView>(Resource.Id.thumbnail);
            _downloadBtn = FindViewById<Button>(Resource.Id.downloadBtn);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            _downloadBtn.Click += OnClickEvent;     
        }

        private async void OnClickEvent(object sender, EventArgs e)
        {
            string url = Clipboard.GetTextAsync().Result;

            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttps;

            if (result == false) return;

            _downloadBtn.Enabled = false;

            var progress = new Progress<double>(p => _progressBar.Progress = (int)(p * 100));

            progress.ProgressChanged += ProgressChangedEvent;

            await Task.Run(async () =>
            {
                Directory.CreateDirectory(downloadFilePath);

                var videoInfo = _youtubeConverterService.GetVideoInfoAsync(url).Result;

                var bitmapImage = GetBitmapFromUrl(videoInfo.Thumbnails[0].Url);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _thumbnail.SetImageBitmap(bitmapImage);
                });

                await _youtubeConverterService.DownloadFile(url, downloadFilePath, progress);
            });
        }

        private void ProgressChangedEvent(object sender, double e)
        {
            int progress = (int)(e * 100);

            if (progress == 100) _downloadBtn.Enabled = true;
        }

        private async void EnsurePermissions()
        {
            var readPerm = Permissions.CheckStatusAsync<Permissions.StorageRead>().Result;
            var writePerm = Permissions.CheckStatusAsync<Permissions.StorageWrite>().Result;

            if (readPerm == PermissionStatus.Denied) 
                await Permissions.RequestAsync<Permissions.StorageRead>();
            if (writePerm == PermissionStatus.Denied) 
                await Permissions.RequestAsync<Permissions.StorageWrite>();
        }

        private Bitmap GetBitmapFromUrl(string url)
        {
            using (WebClient webClient = new WebClient())
            {
                byte[] bytes = webClient.DownloadData(url);

                if (bytes != null && bytes.Length > 0)
                {
                    return BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
                }
            }
            return null;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}