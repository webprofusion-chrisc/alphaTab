using AlphaTab.Importer;
using AlphaTab.Model;
using AlphaTab.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AlphaTab.UniversalApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Score _score;
        private List<Canvas> _diagramCanvasList = new List<Canvas>();

        private int _currentTrackIndex;

        public Score Score
        {
            get { return _score; }
            set
            {
                _score = value;
                //showScoreInfo.Enabled = value != null;
                //Text = "AlphaTab - " + (value == null ? "No File Opened" : value.Title);
                CurrentTrackIndex = 0;
            }
        }

        public int CurrentTrackIndex
        {
            get { return _currentTrackIndex; }
            set
            {
                _currentTrackIndex = value;
                //UpdateSelectedTrack();
                var track = CurrentTrack;
                if (track != null)
                {
                    //alphaTabControl1.Track = track;
                }
            }
        }

        public Track CurrentTrack
        {
            get
            {
                if (Score == null || CurrentTrackIndex < 0 || CurrentTrackIndex >= _score.Tracks.Count) return null;
                return _score.Tracks[_currentTrackIndex];
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        public void Log(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        private async Task<Windows.Storage.StorageFile> BeginOpenFile()
        {
            //if (rootPage.EnsureUnsnapped())
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                openPicker.FileTypeFilter.Add(".gp3");
                openPicker.FileTypeFilter.Add(".gp4");
                openPicker.FileTypeFilter.Add(".gp5");
                openPicker.FileTypeFilter.Add(".gpx");

                StorageFile file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    await file.CopyAndReplaceAsync(await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(file.Name, CreationCollisionOption.ReplaceExisting));
                    LoadScore(file.Name);
                }

                return file;
            }
        }

        public void LoadScore(string fileName = null)
        {
            //test
            if (fileName == null) fileName = @"C:\Work\SVN\webprofusion\scalex\trunk\FileFormats\Testing\Fade To Black.gp4";
            try
            {
                // load the score from the filesystem
                _score = ScoreLoader.LoadScore(fileName);
                Log(_score.Title);
                CurrentTrackIndex = 0;
                AlphaTab.Platform.CSharp.WpfCanvas canvas = new Platform.CSharp.WpfCanvas();
                var settings = Settings.Defaults;
                settings.Engine = "wpf";
                _diagramCanvasList = new List<Canvas>();

                var _renderer = new ScoreRenderer(settings, this);
                _renderer.PartialRenderFinished += _renderer_PartialRenderFinished;
                _renderer.RenderFinished += _renderer_RenderFinished; ;

                _renderer.Render(CurrentTrack);

                /*_renderer.PreRender += () =>
                {
                    lock (this)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            PartialResults.Clear();
                        }));
                    }
                };*/
            }
            catch (Exception exp)
            {
                Log("Failed to load score: " + exp.ToString());
            }
        }

        private void _renderer_RenderFinished(RenderFinishedEventArgs obj)
        {
            this.diagramContainer.Children.Clear();
            int canvasCount = 1;
            double lastY = 0;
            foreach (var canvas in _diagramCanvasList)
            {
                this.diagramContainer.Children.Add(canvas);

                canvasCount--;
                //if (canvasCount == 0) break; //TEMP
                lastY += canvas.Height;
            }
        }

        private void _renderer_PartialRenderFinished(RenderFinishedEventArgs obj)
        {
            _diagramCanvasList.Add((Windows.UI.Xaml.Controls.Canvas)obj.RenderResult);
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            await BeginOpenFile();
            //LoadScore();
        }
    }
}