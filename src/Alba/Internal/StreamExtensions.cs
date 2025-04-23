namespace Alba.Internal;

internal static class StreamExtensions
{
    public static string ReadAllText(this Stream stream)
    {
        using var sr = new StreamReader(stream, leaveOpen: true);
        return sr.ReadToEnd();
    }

    public static byte[] ReadAllBytes(this Stream stream)
    {
        using var content = new MemoryStream();
        stream.CopyTo(content);
        return content.ToArray();
    }

    public static async Task<string> ReadAllTextAsync(this Stream stream)
    {
        using var sr = new StreamReader(stream, leaveOpen: true);
        return await sr.ReadToEndAsync();
    }

    public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
    {
        using var content = new MemoryStream();
        await stream.CopyToAsync(content);
        return content.ToArray();
    }
}