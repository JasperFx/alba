using System.Net.Http.Headers;
using System.Net.Mime;

namespace Alba.Testing.Samples
{
    public class FormData
    {
        #region sample_write_form_data
        public async Task write_form_data(IAlbaHost system)
        {
            var form1 = new Dictionary<string, string>
            {
                ["a"] = "what?",
                ["b"] = "now?",
                ["c"] = "really?"
            };

            await system.Scenario(_ =>
            {
                // This writes the dictionary values to the HTTP
                // request as form data, and sets the content-length
                // header as well as setting the content-type
                // header to application/x-www-form-urlencoded
                _.WriteFormData(form1);
            });
        }
        #endregion

        #region sample_write_multipart_form_data
        public async Task write_multipart_form_data(IAlbaHost system)
        {
            // Read our file into a stream
            await using var imageFile = File.OpenRead("TestImage.jpg");
            // Extract the name from the path
            var imageFileName = Path.GetFileName(imageFile.Name);
            // Create the stream content
            using var content = new StreamContent(imageFile);
            // Remember to manually set the media type as it won't be done automatically
            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Image.Jpeg);

            // Create the MultiPartForm content & add the file
            using var formData = new MultipartFormDataContent();
            formData.Add(content, "files", imageFileName);

            // If you have other content in the form object, you can add it as well!
            formData.Add(new StringContent("My additional metadata"), "metadata");

            var result = await system.Scenario(_ =>
            {
                // This extension will write the content to the request
                // body and set the required headers
                _.Post.MultipartFormData(formData).ToUrl("/api/files/upload");
                _.StatusCodeShouldBeOk();
            });
        }
#endregion
    }
}
