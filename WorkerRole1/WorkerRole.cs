using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private int ms = 10000;
        private string dest;
        private string url;

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");
            while (true)
            {
                Thread.Sleep(ms);
                var c = new Correo()
                {
                    Asunto = "Mensaje worker",
                    Destino = dest,
                    Origen = "abc@def.com",
                    Contenido = "Mensaje de prueba"
                };
                Enviar(c);
            }
            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }

        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            int.TryParse(ConfigurationManager.AppSettings["tiempo"], out ms);
            url = ConfigurationManager.AppSettings["url"];
            dest = ConfigurationManager.AppSettings["destino"];
            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }

        protected async Task Enviar(Correo c)
        {
            string postBody = JsonConvert.SerializeObject(c);
            HttpClient cl = new HttpClient();
            cl.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage wcfResponse = await cl.PostAsync(url, new StringContent(postBody, Encoding.UTF8, "application/json"));
        }
    }
}
