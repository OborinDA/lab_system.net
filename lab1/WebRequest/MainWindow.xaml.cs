using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Threading;

namespace WebRequest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void request_Click(object sender, RoutedEventArgs e)
        {
            System.Net.WebRequest request = System.Net.WebRequest.Create(txb_url.Text);
            request.BeginGetResponse(new AsyncCallback(OnResponse), request);

        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            string filename = txb_fileuri.Text;
            FileWebRequest request =
                   (FileWebRequest)System.Net.WebRequest.Create(filename);

            using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                txb_fileContent.Text = sr.ReadToEnd();
            }
        }

        protected void OnResponse(IAsyncResult ar)
        {
            System.Net.WebRequest request = (System.Net.WebRequest)ar.AsyncState;
            WebResponse response = request.EndGetResponse(ar);
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                // Выводим исходный код страницы
                string line;
                while ((line = stream.ReadLine()) != null)
                    Dispatcher.Invoke(() => txb_sourceCode.Text += line + "\n");
            }

            // Получаем некоторые данные о сервере
            string messageServer = "Целевой URL: \t" + request.RequestUri + "\nМетод запроса: \t" + request.Method +
                 "\nТип полученных данных: \t" + response.ContentType + "\nДлина ответа: \t" + response.ContentLength + "\nЗаголовки";

            // Получаем заголовки, используем LINQ
            WebHeaderCollection whc = response.Headers;
            var headers = Enumerable.Range(0, whc.Count)
                                    .Select(p =>
                                    {
                                        return new
                                        {
                                            Key = whc.GetKey(p),
                                            Names = whc.GetValues(p)
                                        };
                                    });

            foreach (var item in headers)
            {
                messageServer += "\n  " + item.Key + ":";
                foreach (var n in item.Names)
                    messageServer += "\t" + n;
            }

            Dispatcher.Invoke(() => txb_serverInfo.Text = messageServer);

        }

        private void WriteFile_Click(object sender, RoutedEventArgs e)
        {
            System.Net.WebRequest request = System.Net.WebRequest.Create(txb_fileuri.Text);
            request.Method = "PUT";
            using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(txb_writefile.Text);
            }
        }
    }
}
