using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newtonsoft.Json;
using RestSharp;
using tagRipper.Helpers;
using TagLib;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

// ReSharper disable once CheckNamespace

namespace mp3Enhance
{
    public partial class MainWindow
    {
        private string _selectedFile;
        private string _selectedTitle;
        private List<Track> _trackList;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void _askfile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                DereferenceLinks = true,
                Multiselect = false
            };
            switch (fileDialog.ShowDialog())
            {
                case System.Windows.Forms.DialogResult.OK:
                {
                    _selectedFile = fileDialog.FileName;
                    _askfile.Visibility = Visibility.Collapsed;
                    _askName.Visibility = Visibility.Visible;
                    _workArea.Visibility = Visibility.Collapsed;
                    _workArea.Opacity = 1;
                    break;
                }
                case System.Windows.Forms.DialogResult.Cancel:
                {
                    MessageBox.Show("Please select a file and try again", "No file selected", MessageBoxButton.OK);
                    break;
                }
                default:
                {
                    goto case System.Windows.Forms.DialogResult.Cancel;
                }
            }
        }

        private void _client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                raagaMeta listSong = null;
                try
                {
                    listSong = JsonConvert.DeserializeObject<raagaMeta>(e.Result);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Deserialisation error (wonder what it is?)", MessageBoxButton.OK);
                    Environment.Exit(-1);
                }
                if ((listSong != null && listSong.count != 0 && listSong.tracks != null))
                {
                    _trackList = null;
                    _trackList = listSong.tracks;
                    Dispatcher.Invoke(() =>
                    {
                        _songdropdown.Items.Clear();
                        foreach (var item in _trackList)
                        {
                            _songdropdown.Items.Add(CreateComboItem(item));
                        }
                        _askfile.Visibility = Visibility.Collapsed;
                        _askName.Visibility = Visibility.Collapsed;
                        _workArea.Visibility = Visibility.Visible;
                    }, DispatcherPriority.Render);
                }
                else
                {
                    MessageBox.Show("No match found. Try out a different keyword maybe?", "Error (you are responsible)",
                        MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show(
                    string.Concat("Its not your fault. Something to do with your internet. Code ", e.Error.HResult),
                    "Data receive error", MessageBoxButton.OK);
                Environment.Exit(-1);
            }
        }

        private void _confirmTag_Click(object sender, RoutedEventArgs e)
        {
            ModTags(_trackList.ElementAt(_songdropdown.SelectedIndex), _selectedFile);
            MessageBox.Show("Song info updated successfully. Thank you for using tagRipper GUI, by Karthik Nishanth",
                "Success", MessageBoxButton.OK);
            _askName.Visibility = Visibility.Collapsed;
            _askfile.Visibility = Visibility.Visible;
            _workArea.Visibility = Visibility.Collapsed;
        }

        private void _fileNameOk_Click(object sender, RoutedEventArgs e)
        {
            _selectedTitle = _titleField.Text;
            ReturnTracksAsync(_selectedTitle);
        }

        private void _songdropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_songdropdown.SelectedIndex != -1)
            {
                var selectedTrack = _trackList.ElementAt(_songdropdown.SelectedIndex);
                _title.Text = selectedTrack.track_title;
                _album.Text = selectedTrack.album_title;
                var artistList = selectedTrack.artist.Select(artist => artist.name).ToList();
                var artistComplete = artistList.Aggregate("",
                    (current, artistlistitem) => string.Concat(current, artistlistitem, " "));
                var genreList = selectedTrack.gener.Select(itemGener => itemGener.name).ToList();
                var genreComplete = genreList.Aggregate("", (current, str) => string.Concat(current, str, " "));
                _artist.Text = artistComplete;
                _genre.Text = genreComplete;
                _picture.Source = new BitmapImage(new Uri(selectedTrack.artwork_web, UriKind.Absolute));
            }
            else
            {
                MessageBox.Show("Please select the appropriate song data from the drop-down list", "Alert",
                    MessageBoxButton.OK);
            }
        }

        private void _titleField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                _fileNameOk_Click(_titleField, new RoutedEventArgs());
            }
        }

        private ComboBoxItem CreateComboItem(Track track)
        {
            var displayTitle = string.Concat(track.track_title.Replace('\n', ' '), " -> ",
                track.album_title.Replace('\n', ' '));
            return new ComboBoxItem
            {
                Content = displayTitle
            };
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
                MessageBox.Show(exception.Message, "Unhandled error", MessageBoxButton.OK);
        }

        private static IPicture DownloadAlbumArt(Track reqTeack)
        {
            IPicture picture = null;
            var client = new RestClient(reqTeack.artwork_large);
            var response = client.DownloadData(new RestRequest(Method.GET));
            if (response == null)
            {
                MessageBox.Show("Album art download failed", "An error occured", MessageBoxButton.OK);
                Environment.Exit(-1);
/*
                picture = null;
*/
            }
            else
            {
                picture = new Picture(new ByteVector(response));
            }
            return picture;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _askName.Visibility = Visibility.Collapsed;
            _askfile.Visibility = Visibility.Visible;
            _workArea.Visibility = Visibility.Collapsed;
        }

        private static void ModTags(Track reqTeack, string file)
        {
            File sourceFile = null;
            try
            {
                sourceFile = File.Create(file);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "An error occured", MessageBoxButton.OK);
                Environment.Exit(-1);
            }
            sourceFile.Tag.Album = reqTeack.album_title;
            sourceFile.Tag.Title = reqTeack.track_title;
            var artistList = reqTeack.artist.Select(item => item.name).ToList();
            sourceFile.Tag.AlbumArtists = artistList.ToArray<string>();
            var genreList = reqTeack.gener.Select(item => item.name).ToList();
            sourceFile.Tag.Genres = genreList.ToArray<string>();
            var albumArt = DownloadAlbumArt(reqTeack);
            sourceFile.Tag.Pictures = new[] {albumArt};
            try
            {
                sourceFile.Save();
            }
            catch (Exception exception1)
            {
                MessageBox.Show(exception1.Message, "An error occured", MessageBoxButton.OK);
                Environment.Exit(-1);
            }
        }

/*
        private List<Track> ReturnTracks(string songName)
        {
            List<Track> tracks;
            RestClient client =
                new RestClient(string.Format("http://api.gaana.com/?type=search&subtype=search_song&key={0}", songName));
            RestResponse response = (RestResponse) client.Execute(new RestRequest(Method.GET));
            raagaMeta listSong = JsonConvert.DeserializeObject<raagaMeta>(response.Content);
            if (listSong != null)
            {
                tracks = listSong.tracks;
            }
            else
            {
                MessageBox.Show("No match found. Try out a different keyword maybe?", "Error", MessageBoxButton.OK);
                tracks = null;
            }
            return tracks;
        }
*/

        private void ReturnTracksAsync(string songName)
        {
            var client = new WebClient();
            client.DownloadStringCompleted += _client_DownloadStringCompleted;
            client.DownloadStringAsync(
                new Uri(string.Format("http://api.gaana.com/?type=search&subtype=search_song&key={0}", songName),
                    UriKind.Absolute));
        }
    }
}