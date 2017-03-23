using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ChromaKeyServer.Controllers
{
    public class ChromaKeyController : ApiController
    {
        // GET: api/ChromaKey
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/ChromaKey/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/ChromaKey
        [HttpPost]
        public string Post(HttpRequestMessage request, [FromUri] int background_id)
        {
            Debug.WriteLine("POST: api/ChromaKey");
            Debug.WriteLine("background_id = " + background_id);
            var context = new HttpContextWrapper(HttpContext.Current);

            for (int i = 0; i < context.Request.Files.Count; ++i)
            {
                HttpPostedFileBase file = context.Request.Files[i];
                string vidFileName = Path.GetFullPath(HttpContext.Current.Server.MapPath("~") + "\\Videos\\Original\\" + file.FileName);
                string imgFileName = Path.GetFullPath(HttpContext.Current.Server.MapPath("~") + "\\Videos\\Fondos\\" + background_id+".jpg");
                string outFileName = Path.GetFullPath(HttpContext.Current.Server.MapPath("~") + "\\Videos\\Out\\" + file.FileName+"_"+background_id + ".mov");

                int outFile_no = 2;
                while(File.Exists(outFileName))
                    outFileName = Path.GetFullPath(HttpContext.Current.Server.MapPath("~") + "\\Videos\\Out\\" + file.FileName + "_" + background_id + " ("+(outFile_no++)+").mov");
                
                int vidFile_no = 2;
                while (File.Exists(vidFileName))
                {
                    string vidFile_first = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    vidFileName = Path.GetFullPath(HttpContext.Current.Server.MapPath("~") + "\\Videos\\Original\\" + vidFile_first + " (" + (vidFile_no++) + ")"+ extension);
                }

                file.SaveAs(vidFileName);
                Debug.WriteLine("Archivo video: " + vidFileName);
                Debug.WriteLine("Archivo fondo: " + imgFileName);
                Debug.WriteLine("Archivo salida: " + outFileName);

                if(File.Exists(outFileName))
                    Debug.WriteLine("YA EXISTE!!");
                else
                    Debug.WriteLine("OK. NO EXISTE.");

                string prog  = "C:\\ffmpeg\\bin\\ffmpeg";
                string param = "-i \"" + imgFileName + "\" "
                                        + "-i \"" + vidFileName + "\" "
                                        + "-filter_complex \"[1:v]colorkey=0x008A00:0.32:0.1[ckout];[0:v][ckout]overlay[out]\" "
                                        + "-map \"[out]\" \"" + outFileName + "\" ";
                Debug.WriteLine("Ejecutando:");
                Debug.WriteLine(prog + " " + param);

                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.DoWork += new DoWorkEventHandler(
                    delegate (object o, DoWorkEventArgs args)
                    {
                        Process.Start(prog, param);
                    });
                bw.RunWorkerAsync();
            }
            Debug.WriteLine("-- Fin POST: api/ChromaKey");
            return "OK";
        }

        // PUT: api/ChromaKey/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ChromaKey/5
        public void Delete(int id)
        {
        }
    }
}
