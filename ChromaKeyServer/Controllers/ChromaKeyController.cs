using ChromaKeyServer.Models;
using Newtonsoft.Json;
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
        public void Post(HttpRequestMessage request, [FromUri] int background_id)
        {
            var context = new HttpContextWrapper(HttpContext.Current);

            Init init;
            using (StreamReader sr = new StreamReader(HttpContext.Current.Server.MapPath("~") + "/init.json"))
            {
                init = JsonConvert.DeserializeObject<Init>(sr.ReadToEnd());
            }

            for (int i = 0; i < context.Request.Files.Count; ++i)
            {
                HttpPostedFileBase file = context.Request.Files[i];
                string vidFileName = Path.GetFullPath(init.ORIGINAL_VID_PATH + file.FileName);
                string imgFileName = Path.GetFullPath(init.BACKGROUNDS_PATH + background_id+ init.BACKGROUNDS_EXTENSION);
                string outFileName = Path.GetFullPath(init.OUTPUT_VID_PATH + file.FileName+"_"+background_id + init.OUTPUT_VID_EXTENSION);

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

                string prog  = init.FFMPEG_BIN_PATH;
                string param = "-i \"" + imgFileName + "\" "
                                        + "-i \"" + vidFileName + "\" "
                                        + "-filter_complex \"[1:v]colorkey=0x"+ init.COLORKEY+":"+ init.SIMILARITY+":"+ init.BLEND+"[ckout];[0:v][ckout]overlay[out];[out]setpts="+init.FRAME_RATE_RELATION+"*PTS[final]\" "
                                        + "-map \"[final]\" \"" + outFileName + "\" ";
                var process = Process.Start(prog, param);
                process.WaitForExit();
            }
        }
    }
}
