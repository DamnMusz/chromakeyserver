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
using System.Text;
using System.Web;
using System.Web.Http;

namespace ChromaKeyServer.Controllers
{
    public class ChromaKeyController : ApiController
    {
        // POST: api/ChromaKey
        [HttpPost]
        public string Post(HttpRequestMessage request, [FromUri] int background_id)
        {
            var context = new HttpContextWrapper(HttpContext.Current);

            // Leo los parametros con los que trabajar desde el init.json
            Init init;
            using (StreamReader sr = new StreamReader(HttpContext.Current.Server.MapPath("~") + "/init.json"))
            {
                init = JsonConvert.DeserializeObject<Init>(sr.ReadToEnd());
            }

            string thumbName = "";

            for (int i = 0; i < context.Request.Files.Count; ++i)
            {
                HttpPostedFileBase file = context.Request.Files[i];
                string vidFileName = Path.GetFullPath(init.ORIGINAL_VID_PATH + file.FileName);
                string imgFileName = Path.GetFullPath(init.BACKGROUNDS_PATH + background_id+ init.BACKGROUNDS_EXTENSION);
                string vidName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + background_id + init.OUTPUT_VID_EXTENSION;
                string outFileName_1 = Path.GetFullPath(init.OUTPUT_VID_PATH + vidName);
                string outFileName_2 = Path.GetFullPath(init.OUTPUT_VID_PATH + Path.GetFileNameWithoutExtension(outFileName_1) + init.OUTPUT_VID_EXTENSION_2);
                thumbName = init.OUTPUT_THUMBNAIL_RELATIVE_PATH + vidName;

                // Si el nombre de video de salida ya existe voy probando agregandole (2), (3).. (n)
                int outFile_no = 2;
                while (File.Exists(outFileName_1))
                {
                    vidName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + background_id + " (" + (outFile_no++) + ")"+ init.OUTPUT_VID_EXTENSION;
                    outFileName_1 = Path.GetFullPath(init.OUTPUT_VID_PATH + vidName);
                    thumbName = init.OUTPUT_THUMBNAIL_RELATIVE_PATH + vidName;
                    outFileName_2 = Path.GetFullPath(init.OUTPUT_VID_PATH + Path.GetFileNameWithoutExtension(outFileName_1) + init.OUTPUT_VID_EXTENSION_2);
                }
                
                // Si el nombre de video original ya existe voy probando agregandole (2), (3).. (n)
                int vidFile_no = 2;
                while (File.Exists(vidFileName))
                {
                    string vidFile_first = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    vidFileName = Path.GetFullPath(init.ORIGINAL_VID_PATH + vidFile_first + " (" + (vidFile_no++) + ")"+ extension);
                }

                // Guardo el video original con fondo verde, tal cual lo recibi
                file.SaveAs(vidFileName);

                string prog  = init.FFMPEG_BIN_PATH + "ffmpeg";
                string param = "-i \"" + imgFileName + "\" "
                                        + "-i \"" + vidFileName + "\" "
                                        + "-filter_complex \"[1:v]colorkey=0x"+ init.COLORKEY+":"+ init.SIMILARITY+":"+ init.BLEND+"[ckout];[0:v][ckout]overlay[out];[out]setpts="+init.FRAME_RATE_RELATION+"*PTS[final]\" "
                                        + "-vcodec libx264 -map \"[final]\" \"" + outFileName_1 + "\" ";

                // Ejecuto el proceso de chroma key
                var process = Process.Start(prog, param);

                string param2 = "-i \"" + imgFileName + "\" "
                                        + "-i \"" + vidFileName + "\" "
                                        + "-filter_complex \"[1:v]colorkey=0x" + init.COLORKEY + ":" + init.SIMILARITY + ":" + init.BLEND + "[ckout];[0:v][ckout]overlay[out];[out]setpts=" + init.FRAME_RATE_RELATION + "*PTS[final]\" "
                                        + "-vcodec libx264 -map \"[final]\" \"" + outFileName_2 + "\" ";

                // Ejecuto el proceso de chroma key
                var process2 = Process.Start(prog, param2);


                Debug.WriteLine(param);
                Debug.WriteLine(param2);

                process.WaitForExit();
                process2.WaitForExit();

                string thumbOutFile = Path.GetFullPath(HttpContext.Current.Server.MapPath("~") + init.OUTPUT_THUMBNAIL_RELATIVE_PATH + Path.GetFileNameWithoutExtension(thumbName) + ".png");
                string thumbParam = " -i \"" + outFileName_1 + "\" -ss 00:00:05.435 -vframes 1 -filter:v scale=\""+init.THUMB_SCALE + ":-1\" \"" + thumbOutFile + "\"";

                // No deberia existir otro thumb con el nombre del video (el cual ya verifica que sea siempre distinto). Asi que si existe borra el anterior.
                if(File.Exists(thumbOutFile))
                    File.Delete(thumbOutFile);

                Debug.WriteLine(prog + " " + thumbParam);

                // Ejecuto el proceso de creacion del thumbnail
                var process3 = Process.Start(prog, thumbParam);
                process3.WaitForExit();
            }

            return (init.OUTPUT_THUMBNAIL_RELATIVE_PATH + Path.GetFileNameWithoutExtension(thumbName) + ".png").Replace("\\\\","/");
        }
    }
}
