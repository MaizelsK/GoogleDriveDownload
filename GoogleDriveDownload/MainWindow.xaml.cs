using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GoogleDriveDownload
{
    public partial class MainWindow : Window
    {
        UserCredential userCredential;

        public MainWindow()
        {
            InitializeComponent();

            DriveAuthenticate();
        }

        // Click 
        private async void DownloadBtnClick(object sender, RoutedEventArgs e)
        {
            TextBlock.Text = null;
            TextBlock.Text = await DownloadText();
        }

        private Task<string> DownloadText()
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = "Drive API QuickStart"
            });

            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            string textId;

            textId = files.SingleOrDefault(x => x.Name == "text.txt").Id;

            if (textId != null)
            {
                Stream stream = new MemoryStream();
                service.Files.Get(textId).Download(stream);

                byte[] textBytes = new byte[stream.Length];

                stream.Position = 0;
                stream.ReadAsync(textBytes, 0, (int)stream.Length);

                return Task.Run(() =>
                {
                    return Encoding.Default.GetString(textBytes);
                });
            }
            else
            {
                MessageBox.Show("No text found.");
            }

            return Task.Run(() => { return ""; });
        }

        private void DriveAuthenticate()
        {
            string[] scopes = new string[] { DriveService.Scope.Drive };

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credPath = System.IO.Path.Combine(credPath, ".credentials/", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

                userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets,
                                                                         scopes,
                                                                         "user",
                                                                         CancellationToken.None,
                                                                         new FileDataStore(credPath, true)).Result;
            }
        }
    }
}