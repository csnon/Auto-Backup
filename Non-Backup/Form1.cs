using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace Non_Backup
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
        //Anonfiles.com upload
        static string CreateDownloadLink(string File)
        {
            string ReturnValue = string.Empty;
            try
            {
                using (WebClient Client = new WebClient())
                {
                    byte[] Response = Client.UploadFile("https://api.anonfiles.com/upload", File); //Upload And Return Response
                    string ResponseBody = Encoding.ASCII.GetString(Response);
                    if (ResponseBody.Contains("\"error\": {")) // Simple Error Handling
                    {
                        ReturnValue += "There was a erorr while uploading the file.\r\n";
                        ReturnValue += "Error message: " + ResponseBody.Split('"')[7] + "\r\n";
                    }
                    else
                    {
                        ReturnValue += ResponseBody.Split('"')[15] + "\r\n"; // 
                    }
                }
            }
            catch (Exception Exception)
            {
                ReturnValue += "Exception Message:\r\n" + Exception.Message + "\r\n"; // Return Error 
            }
            return ReturnValue;
        }
        //Webhooks
        private static string defaultUserAgent = "Elysia#0896";
        private static string defaultAvatar = "https://cdn.discordapp.com/attachments/798782780292333571/886640991308746862/20210826_103405.jpg";

        public static string Send(string mssgBody, string userName, string webhook)
        {
            // Generate post objects
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("username", userName);
            postParameters.Add("content", mssgBody);
            postParameters.Add("avatar_url", defaultAvatar);

            // Create request and receive response
            HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(webhook, defaultUserAgent, postParameters);

            // Process response
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();
            webResponse.Close();


            //return string with response
            return fullResponse;
        }

        public static string SendFile(
            string mssgBody,
            string filename,
            string fileformat,
            string filepath,
            string application,
            string userName,
            string webhooklink)
        {
            // Read file data
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();

            // Generate post objects
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("filename", filename);
            postParameters.Add("fileformat", fileformat);
            postParameters.Add("file", new FormUpload.FileParameter(data, filename, application/*"application/msexcel"*/));

            postParameters.Add("username", userName);
            postParameters.Add("content", mssgBody);
            postParameters.Add("avatar_url", defaultAvatar);

            // Create request and receive response
            HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(webhooklink, defaultUserAgent, postParameters);

            // Process response
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();
            webResponse.Close();

            //return string with response
            return fullResponse;
        }

        public static class FormUpload //formats data as a multi part form to allow for file sharing
        {
            private static readonly Encoding encoding = Encoding.UTF8;
            public static HttpWebResponse MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters)
            {
                string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());

                string contentType = "multipart/form-data; boundary=" + formDataBoundary;

                byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

                return PostForm(postUrl, userAgent, contentType, formData);
            }

            private static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData)
            {
                HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

                if (request == null)
                {

                    throw new NullReferenceException("request is not a http request");
                }

                // Set up the request properties.
                request.Method = "POST";
                request.ContentType = contentType;
                request.UserAgent = userAgent;
                request.CookieContainer = new CookieContainer();
                request.ContentLength = formData.Length;

                // Send the form data to the request.
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(formData, 0, formData.Length);
                    requestStream.Close();
                }

                return request.GetResponse() as HttpWebResponse;
            }

            private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
            {
                Stream formDataStream = new System.IO.MemoryStream();
                bool needsCLRF = false;

                foreach (var param in postParameters)
                {
                    if (needsCLRF)
                        formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                    needsCLRF = true;

                    if (param.Value is FileParameter)
                    {
                        FileParameter fileToUpload = (FileParameter)param.Value;

                        string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                            boundary,
                            param.Key,
                            fileToUpload.FileName ?? param.Key,
                            fileToUpload.ContentType ?? "application/octet-stream");

                        formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                        formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                    }
                    else
                    {
                        string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                            boundary,
                            param.Key,
                            param.Value);
                        formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                    }
                }

                // Add the end of the request.  Start with a newline
                string footer = "\r\n--" + boundary + "--\r\n";
                formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

                // Dump the Stream into a byte[]
                formDataStream.Position = 0;
                byte[] formData = new byte[formDataStream.Length];
                formDataStream.Read(formData, 0, formData.Length);
                formDataStream.Close();

                return formData;
            }

            public class FileParameter
            {
                public byte[] File { get; set; }
                public string FileName { get; set; }
                public string ContentType { get; set; }
                public FileParameter(byte[] file) : this(file, null) { }
                public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
                public FileParameter(byte[] file, string filename, string contenttype)
                {
                    File = file;
                    FileName = filename;
                    ContentType = contenttype;
                }
            }
        }
        //timer scroll bar
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label6.Text = trackBar1.Value.ToString() + " Hour";
        }
        //select your file 1
        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "File 1";
            if (fbd.ShowDialog() == DialogResult.OK)
                textBox2.Text = fbd.SelectedPath;
        }
        //select your file 2
        private void button4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "File 2";
            if (fbd.ShowDialog() == DialogResult.OK)
                textBox3.Text = fbd.SelectedPath;
        }
        //select your file 3
        private void button5_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "File 3";
            if (fbd.ShowDialog() == DialogResult.OK)
                textBox4.Text = fbd.SelectedPath;
        }
        //start button
        private void button1_Click(object sender, EventArgs e)
        {
           
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please Input The Webhook Link", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

            }
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Please Choose the file ", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }// 1 hour = 3600000 ms
            if (trackBar1.Value < 1)
            {
                MessageBox.Show("Please Choose the timer value", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                button1.Enabled = false;
                int BackupTimer = trackBar1.Value * 3600000;
                timer1.Interval = BackupTimer;
                timer1.Start();
                label7.Text = "Auto Backup = ON";
            }

        }
        //stop button
        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            timer1.Stop();
            label7.Text = "Auto Backup = OFF";
            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.CancelAsync();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string desktop = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
            string file1 = @textBox2.Text;
            string file2 = @textBox3.Text;
            string file3 = @textBox4.Text;
            string zipfile1 = desktop + @"\\file1.zip";
            string zipfile2 = desktop + @"\\file2.zip";
            string zipfile3 = desktop + @"\\file3.zip";
            if (File.Exists(zipfile1))
            {
                File.Delete(zipfile1);
            }
            if (File.Exists(zipfile2))
            {
                File.Delete(zipfile2);
            }
            if (File.Exists(zipfile3))
            {
                File.Delete(zipfile3);
            }
            if (!string.IsNullOrWhiteSpace(textBox2.Text))
            {
                ZipFile.CreateFromDirectory(file1, desktop + @"\\file1.zip");
                if (File.Exists(zipfile1))
                {
                    String Link1 = CreateDownloadLink(zipfile1);
                    Send("**File 1 : " + Link1, "Non-Backup | Auto backup", textBox1.Text);
                }
            }
            if (!string.IsNullOrWhiteSpace(textBox3.Text))
            {
                ZipFile.CreateFromDirectory(file2, desktop + @"\\file2.zip");
                if (File.Exists(zipfile2))
                {
                    String Link2 = CreateDownloadLink(zipfile2);
                    Send("**File 2 : " + Link2, "Non-Backup | Auto backup", textBox1.Text);
                }
            }
            if (!string.IsNullOrWhiteSpace(textBox4.Text))
            {
                ZipFile.CreateFromDirectory(file3, desktop + @"\\file3.zip");
                if (File.Exists(zipfile3))
                {
                    String Link3 = CreateDownloadLink(zipfile3);
                    Send("**File 3 : " + Link3, "Non-Backup | Auto backup", textBox1.Text);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
