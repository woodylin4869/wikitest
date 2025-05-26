using System.IO.Compression;
using System.IO;

namespace H1_ThirdPartyWalletAPI.Helpers;

public class GzipHelper
{
    public static byte[] Compress(byte[] data)
    {
        using var memoryStream = new MemoryStream();
        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress);
        gzipStream.Write(data, 0, data.Length);
        gzipStream.Close();
        return memoryStream.ToArray();
    }

    public static byte[] Decompress(byte[] data)
    {
        using var outMemoryStream = new MemoryStream();
        using var memoryStream = new MemoryStream(data);
        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
        gzipStream.CopyTo(outMemoryStream);
        return outMemoryStream.ToArray();
    }
}