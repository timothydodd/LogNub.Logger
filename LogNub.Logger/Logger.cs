using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace LogNub.Logger
{
    public class Logger
    {
        public string ApiKey { get; }
        public string Machine { get; }
        public string Application { get; }
        public string Category { get; }
        private BackgroundWorker _worker = new BackgroundWorker();
        private string _webServiceURL =  "http://www.lognub.com";
        public string WebServiceURL { get { return _webServiceURL; } set { _webServiceURL = value; } }
        public bool IsAsync { get { return _isAsync; } set { _isAsync = value; } }
        private bool _isAsync = true;
        public Logger(string apiKey,string machine,string application, string category)
        {
            ApiKey = apiKey;
            this.Machine = machine;
            this.Application = application;
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

        public void Write(string type, string machine,string app, string category, string message)
        {
            var logEntry = GetLogEntry();
            logEntry.Type = type;
            logEntry.Application = app;
            logEntry.Category = category;
            logEntry.Date = DateTime.UtcNow;
            logEntry.Message = message;
            logEntry.Machine = machine;
            logEntry.ApiKey = ApiKey;
            if (IsAsync)
            {
                lock (_queue)
                {
                    _queue.Enqueue(logEntry);

                }
                if (!_worker.IsBusy)
                {
                    _worker.RunWorkerAsync();
                }
            }
            else
            {
                SendLogEntry(logEntry);

            }
        }
        public void Write(string type, string message)
        {
            Write(type, Machine,Application, Category, message);
        }
        public void Write( string message)
        {
            Write(LogEntryType.Event.ToString(), Machine, Application, Category, message);
        }
        public void Write(Exception exception)
        {
            Write(LogEntryType.Error.ToString(), Machine, Application, Category, exception.ToString());
        }

        private void SendLogEntry(LogEntry entry)
        {
            try
            {

                string baseAddress = WebServiceURL;

                string serviceURL = string.Format("{0}/Log/Write", baseAddress);
                HttpWebRequest req = (HttpWebRequest) WebRequest.Create(serviceURL);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Headers.Add("Content-Encoding", "gzip");

                var body = SmallSerializer.Wrap("value", SmallSerializer.SerializeObject(entry, false));
                byte[] reqBodyBytes = Encoding.UTF8.GetBytes(body);
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
                       var  txt =new StreamReader(resp.GetResponseStream()).ReadToEnd();
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
