using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<ActionResult<List<UploadResponse>>> UploadTextFile(List<IFormFile> files)
        {
            if (files.Count > 0)
            {
                var res = new List<UploadResponse>();
                foreach (var formFile in files)
                {
                    if (formFile.ContentType == MediaTypeNames.Text.Plain)
                    {
                        var content = await ReadAsStringAsync(formFile);
                        res.Add(new UploadResponse(formFile.FileName, formFile.Length, content.TrimEnd()));
                    }

                    if (formFile.ContentType == MediaTypeNames.Image.Jpeg)
                    {
                        res.Add(new UploadResponse(formFile.FileName, formFile.Length, "image"));
                    }
                }
                    

                return res;
            }

            return BadRequest();
        }

        public record UploadResponse(string Name, long Length, string Content);

        public static async Task<string> ReadAsStringAsync(IFormFile file)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    result.AppendLine(await reader.ReadLineAsync());
            }
            return result.ToString();
        }
    }
}
