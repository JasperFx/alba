using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using WebApp.Controllers;

namespace Alba.Testing.Acceptance;

public record UploadedFile(string Name, string FileName, string ContentType, long Length, string Tag);

public class multipart_form_data_builder
{
    private static async Task<IAlbaHost> startUploadApp()
    {
        return await AlbaHost.For(WebApplication.CreateBuilder(), app =>
        {
            app.MapPost("/upload", (IFormFile file, [FromForm] string tag) =>
                    new UploadedFile(file.Name, file.FileName, file.ContentType, file.Length, tag))
                .DisableAntiforgery();
        });
    }

    [Fact]
    public async Task files_and_fields_bind_to_a_minimal_api_endpoint()
    {
        await using var host = await startUploadApp();

        var result = await host.Scenario(x =>
        {
            x.Post.MultipartFormData(m => m
                    .File("file", "a,b\n1,2"u8.ToArray(), "report.csv")
                    .Field("tag", "quarterly"))
                .ToUrl("/upload");
        });

        var uploaded = await result.ReadAsJsonAsync<UploadedFile>();
        uploaded.ShouldBe(new UploadedFile("file", "report.csv", "text/csv", 7, "quarterly"));
    }

    [Theory]
    [InlineData("photo.jpg", null, "image/jpeg")]
    [InlineData("data.unknown-extension", null, "application/octet-stream")]
    [InlineData("data.bin", "application/x-custom", "application/x-custom")]
    public async Task file_content_types(string fileName, string contentType, string expected)
    {
        await using var host = await startUploadApp();

        var result = await host.Scenario(x =>
        {
            x.Post.MultipartFormData(m => m.File("file", [1, 2, 3], fileName, contentType).Field("tag", "t"))
                .ToUrl("/upload");
        });

        (await result.ReadAsJsonAsync<UploadedFile>()).ContentType.ShouldBe(expected);
    }

    [Fact]
    public async Task several_files_bind_to_an_mvc_collection()
    {
        await using var host = await AlbaHost.For<WebApp.Program>();
        await using var image = File.OpenRead("TestImage.jpg");

        #region sample_multipart_form_data_builder
        var result = await host.Scenario(x =>
        {
            x.Post.MultipartFormData(form => form
                    // Files with the same name bind to a collection; the content type
                    // comes from the file name extension unless one is passed
                    .File("files", Encoding.UTF8.GetBytes("Hello there!"), "greeting.txt")
                    .File("files", image, "TestImage.jpg")
                    .Field("additionalContent", "My additional string of fun times"))
                .ToUrl("/api/files/upload");

            x.StatusCodeShouldBeOk();
        });
        #endregion

        var uploads = await result.ReadAsJsonAsync<List<FilesController.UploadResponse>>();
        uploads.ShouldBe([
            new FilesController.UploadResponse("greeting.txt", 12, "Hello there!", "My additional string of fun times"),
            new FilesController.UploadResponse("TestImage.jpg", image.Length, "image", "My additional string of fun times")
        ]);
    }
}
