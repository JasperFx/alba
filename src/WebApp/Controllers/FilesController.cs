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
        public async Task<ActionResult<List<UploadResponse>>> UploadTextFile([FromForm] UploadRequest request)
        {
            if (request.Files.Count > 0)
            {
                var res = new List<UploadResponse>();
                foreach (var formFile in request.Files)
                {
                    if (formFile.ContentType == MediaTypeNames.Text.Plain)
                    {
                        var content = await ReadAsStringAsync(formFile);
                        res.Add(new UploadResponse(formFile.FileName, formFile.Length, content.TrimEnd(), request.AdditionalContent));
                    }

                    if (formFile.ContentType == MediaTypeNames.Image.Jpeg)
                    {
                        res.Add(new UploadResponse(formFile.FileName, formFile.Length, "image", request.AdditionalContent));
                    }
                }
                    

                return res;
            }

            return BadRequest();
        }

        public record UploadRequest(string AdditionalContent, List<IFormFile> Files);

        public record UploadResponse(string Name, long Length, string Content, string AdditionalContent);

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
