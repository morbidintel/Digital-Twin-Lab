using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace DotSpatial.Projections
{
    internal static class DeflateStreamReader
    {
        public static Stream DecodeFile(string path)
        {
			using (var s = new FileStream(path, FileMode.Open))
			{
				var msUncompressed = new MemoryStream();
				using (var ds = new DeflateStream(s, CompressionMode.Decompress, true))
				{
					var buffer = new byte[4096];
					int numRead;
					while ((numRead = ds.Read(buffer, 0, buffer.Length)) != 0)
					{
						msUncompressed.Write(buffer, 0, numRead);
					}
				}
				msUncompressed.Seek(0, SeekOrigin.Begin);
				return msUncompressed;
			}
        }
    }
}