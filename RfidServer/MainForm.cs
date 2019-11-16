using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using SimpleHttp;

namespace RfidServer
{
    public partial class MainForm : Form
    {
        public static int Port = 1337;

        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            lblAddress.Text = $"{LocalIPAddress()}:{Port}";
            Shown += async (o, e) => await InitializeServer();
        }

        async Task InitializeServer()
        {
            Route.Add("/log", (req, res, props) =>
            {
                using (var reader = new StreamReader(req.InputStream, Encoding.UTF8))
                {
                    try
                    {
                        var json = reader.ReadToEnd();
                        var logRequest = JsonConvert.DeserializeObject<LogRequest>(json);

                        richTextBox.AppendText(logRequest.Tag);
                        richTextBox.AppendText(Environment.NewLine);
                    }
                    catch { /* Ignore on invalid tag request */ }
                }
                
                res.AsText("OK");
            }, "POST");

            await HttpServer.ListenAsync(
                    Port,
                    CancellationToken.None,
                    Route.OnHttpRequestAsync
                );
        }

        public static string LocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
