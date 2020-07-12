using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace nusdm
{
	public static class Decryptor
	{

		#region Private Fields
		
		static readonly string[] files = { "msvcr120d.dll", "libeay32.dll", "CDecrypt_v2.0b.exe" };
		static readonly string tmp = Path.GetTempPath();
		static string destination = "";
		static Dictionary<string, string> hashes = new Dictionary<string, string>();

		#endregion Private Fields


		#region Public Methods

		public static int Decrypt(string path)
		{
			destination = path;

			CopyFiles();

			int rc = Start();

			DeleteFiles();

			return rc;
		}

		public static void DownloadCDecrypt()
		{
			hashes.Add("CDecrypt_v2.0b.exe", "4505152FC7245383105BD0AB3FBFAD7F");
			hashes.Add("libeay32.dll", "320FD1D9FC94E40CEDCBA3F9CC7AEC43");
			hashes.Add("msvcr120d.dll", "F67CA8D338DFD99E3C540336221F8FA7");

			foreach (var file in files)
			{
				if (!File.Exists(tmp + file))
				{
					Task.Run(() =>
					{
						string dest = tmp + file;
						using WebClient wc = new WebClient();
						byte[] data = wc.DownloadData(@"https://github.com/skylerspark/CDecrypt-Release/raw/master/" + file);
						string dataHash = GetHashMD5(data);
						if (hashes[file] == dataHash)
						{
							File.WriteAllBytes(dest, data);
						}
						else
						{
							throw new Exception("MD5 hash does not match");
						}
					});
				}
			}
		}

		#endregion Public Methods


		#region Private Methods

		public static string GetHashMD5(byte[] stream)
		{
			using (MD5 md5 = new MD5CryptoServiceProvider())
			{
				byte[] retVal = md5.ComputeHash(stream);
				return BitConverter.ToString(retVal).Replace("-", ""); // hex string
			}
		}

		private static void CopyFiles()
		{
			foreach (var file in files)
			{
				try
				{
					File.Copy(tmp + file, destination + "\\" + file, true);
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		private static void DeleteFiles()
		{
			foreach (var file in files)
			{
				if (File.Exists(destination + "\\" + file))
				{
					File.Delete(destination + "\\" + file);
				}
			}
		}
		private static int Start()
		{
			string fileName = destination + "\\" + files[2];
			Process p = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = fileName,
					Arguments = "title.tmd title.tik",
					UseShellExecute = false,
					CreateNoWindow = false,
					WorkingDirectory = destination
				}
			};

			p.Start();
			p.WaitForExit();
			return p.ExitCode;
		}

		#endregion Private Methods
	}
}
