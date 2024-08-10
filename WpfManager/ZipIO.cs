using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.BZip2;

namespace WpfManager;

static class ZipIO
{
	static public void ExtractFile(string archiveFilePath, string fileNameToExtract, string outputFilePath)
	{
		using (FileStream fs = File.OpenRead(archiveFilePath))
		{
			if (IsZipFile(fs))
			{
				ExtractFromZip(fs, fileNameToExtract, outputFilePath);
			}
			else if (IsTarFile(fs))
			{
				ExtractFromTar(fs, fileNameToExtract, outputFilePath);
			}
			else if (IsGZipFile(fs))
			{
				ExtractFromGZip(fs, outputFilePath);
			}
			else if (IsBZip2File(fs))
			{
				ExtractFromBZip2(fs, outputFilePath);
			}
			else
			{
				throw new Exception("Unknown or unsupported archive type");
			}
		}
	}

	static bool IsZipFile(Stream fs)
	{
		byte[] zipSignature = [0x50, 0x4B, 0x03, 0x04]; // "PK"
		fs.Seek(0, SeekOrigin.Begin);
		byte[] buffer = new byte[zipSignature.Length];
		fs.Read(buffer, 0, zipSignature.Length);
		fs.Seek(0, SeekOrigin.Begin);
		for (int i = 0; i < zipSignature.Length; i++)
		{
			if (buffer[i] != zipSignature[i])
			{
				return false;
			}
		}
		return true;
	}

	static bool IsTarFile(Stream fs)
	{
		byte[] tarSignature = [0x75, 0x73, 0x74, 0x61, 0x72]; // "ustar" signature in TAR
		fs.Seek(257, SeekOrigin.Begin);
		byte[] buffer = new byte[tarSignature.Length];
		fs.Read(buffer, 0, tarSignature.Length);
		fs.Seek(0, SeekOrigin.Begin);
		return System.Text.Encoding.ASCII.GetString(buffer) == "ustar";
	}

	static bool IsGZipFile(Stream fs)
	{
		byte[] gzipSignature = [0x1F, 0x8B]; // ␟‹	
		fs.Seek(0, SeekOrigin.Begin);
		byte[] buffer = new byte[gzipSignature.Length];
		fs.Read(buffer, 0, gzipSignature.Length);
		fs.Seek(0, SeekOrigin.Begin);
		return buffer[0] == gzipSignature[0] && buffer[1] == gzipSignature[1];
	}

	static bool IsBZip2File(Stream fs)
	{
		byte[] bzip2Signature = [0x42, 0x5A, 0x68]; // "BZh"
		fs.Seek(0, SeekOrigin.Begin);
		byte[] buffer = new byte[bzip2Signature.Length];
		fs.Read(buffer, 0, bzip2Signature.Length);
		fs.Seek(0, SeekOrigin.Begin);
		return buffer[0] == bzip2Signature[0] && buffer[1] == bzip2Signature[1] && buffer[2] == bzip2Signature[2];
	}

	static void ExtractFromZip(Stream fs, string fileNameToExtract, string outputFilePath)
	{
		using (ZipFile zipFile = new ZipFile(fs))
		{
			ZipEntry entry = zipFile.GetEntry(fileNameToExtract);
			if (entry != null)
			{
				using (Stream zipStream = zipFile.GetInputStream(entry))
				using (FileStream outputFs = File.Create(outputFilePath))
				{
					zipStream.CopyTo(outputFs);
				}
			}
			else
			{
				throw new Exception("Decriptor not found in archive");
			}
		}
	}

	static void ExtractFromTar(Stream fs, string fileNameToExtract, string outputFilePath)
	{
		using (TarInputStream tarStream = new TarInputStream(fs))
		{
			TarEntry entry;
			while ((entry = tarStream.GetNextEntry()) != null)
			{
				if (entry.Name == fileNameToExtract)
				{
					using (FileStream outputFs = File.Create(outputFilePath))
					{
						tarStream.CopyTo(outputFs);
					}
					return;
				}
			}
			throw new Exception("Decriptor not found in archive");
		}
	}

	static void ExtractFromGZip(Stream fs, string outputFilePath)
	{
		using (GZipInputStream gzipStream = new GZipInputStream(fs))
		using (FileStream outputFs = File.Create(outputFilePath))
		{
			gzipStream.CopyTo(outputFs);
		}
	}

	static void ExtractFromBZip2(Stream fs, string outputFilePath)
	{
		using (BZip2InputStream bzip2Stream = new BZip2InputStream(fs))
		using (FileStream outputFs = File.Create(outputFilePath))
		{
			bzip2Stream.CopyTo(outputFs);
		}
	}
}
