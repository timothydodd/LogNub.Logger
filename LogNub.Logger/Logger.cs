using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LogNub.Logger.Properties;

namespace LogNub.Logger
{
    public class Logger
    {
        public string ApiKey { get; }
        public string Machine { get; }
        public string Category { get; }
        private BackgroundWorker _worker = new BackgroundWorker();

        public Logger(string apiKey,string machine, string category)
        {
            ApiKey = apiKey;
            this.Machine = machine;
            this.Category = category;
            _worker.DoWork += _worker_DoWork;
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {

            int count = 0;
            lock (_queue)
            {
                count = _queue.Count;

            }
            while (count>0)
            {

                LogEntry entry = null;
                lock (_queue)
                {

                    entry = _queue.Dequeue();
                }
                SendLogEntry(entry);

                lock (_queue)
                {
                    count = _queue.Count;

                }
            }
        }

        public void Write(string type, string machine, string category, string message)
        {
            var logEntry = GetLogEntry();
            logEntry.Type = type;
            logEntry.Category = category;
            logEntry.Date = DateTime.UtcNow;
            logEntry.Message = message;
            logEntry.Machine = machine;
            lock (_queue)
            {
                _queue.Enqueue(logEntry);

            }
            if (!_worker.IsBusy)
            {
                _worker.RunWorkerAsync();
            }
        }
        public void Write(string type, string message)
        {
            Write(type, Machine, Category, message);
        }
        public void Write( string message)
        {
            Write(LogEntryType.Event.ToString(), Machine, Category, message);
        }
        public void Write(Exception exception)
        {
            Write(LogEntryType.Error.ToString(), Machine, Category, exception.ToString());
        }

        private void SendLogEntry(LogEntry entry)
        {
            try
            {

                string baseAddress = Settings.Default.WebServiceURL;

                string serviceURL = string.Format("{0}/Log/Write", baseAddress);
                HttpWebRequest req = (HttpWebRequest) HttpWebRequest.Create(serviceURL);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Headers.Add("Content-Encoding", "gzip");

                var body = SmallSerializer.Wrap("value", SmallSerializer.SerializeObject(entry, false));
                byte[] reqBodyBytes = Encoding.UTF8.GetBytes(body);
                Console.WriteLine("Sent payload:{0} bytes", body.Length);
                using (Stream postStream = req.GetRequestStream())
                {
                    using (var zipStream = new GZipStream(postStream, CompressionMode.Compress))
                    {
                        zipStream.Write(reqBodyBytes, 0, reqBodyBytes.Length);
                    }
                }
                using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse())

                {
                    if (resp.ContentLength > 0)
                    {
                        Console.WriteLine(new StreamReader(resp.GetResponseStream()).ReadToEnd());
                    }
                    resp.Close();
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());

            }
        }

        private LogEntry GetLogEntry()
        {
            lock (_pool)
            {
                if (_pool.Count > 0)
                {
                    return _pool.Pop();
                }
                else
                {
                    return new LogEntry();
                }

            }
           
        }
        private Queue<LogEntry> _queue = new Queue<LogEntry>();
        private Stack<LogEntry> _pool = new Stack<LogEntry>();
    }

    public enum LogEntryType
    {
        Event,
        Information,
        Error
    }
}
