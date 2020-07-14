using FunKiiUNETThingy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace nusdm
{
	public class MainWindowViewModel : ViewModelBase
	{
		#region Private Fields

		private readonly object __lockObj = new object();
		private readonly string COMMONKEY = SettingsProvider.Settings.CommonKey.ToString();
		private readonly string commonKeyHash = "7835902d8f70f315305f34edf2f05923";
		private string filter;
		private bool isIdle = true;
		private string logEntry;
		private ICollectionView titlesView;

		#endregion Private Fields

		#region Public Constructors

		public MainWindowViewModel()
		{
			if (Decryptor.GetHashMD5(Encoding.ASCII.GetBytes(COMMONKEY.ToLower())).ToLower() != commonKeyHash)
			{
				MessageBox.Show("Please specify the correct WII U common key in config file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				Process.Start("notepad.exe", SettingsProvider.Savefile).WaitForExit();
				//Process.Start(Process.GetCurrentProcess().MainModule.FileName);
				Application.Current.Shutdown();
			}

			InitializeCommands();

			InitializeList();

			Decryptor.DownloadCDecrypt();

		}

		#endregion Public Constructors

		#region Public Properties

		private string windowTitle;
		public RelayCommand CmdClearLog { get; private set; }
		public RelayCommand CmdCopyLog { get; private set; }
		public RelayCommand CmdCopyName { get; private set; }
		public RelayCommand CmdCopyTitleId { get; private set; }
		public RelayCommand CmdCopyTitleKey { get; private set; }
		public RelayCommand CmdDownloadDlc { get; private set; }
		public RelayCommand CmdDownloadTitle { get; private set; }
		public RelayCommand CmdDownloadUpdate { get; private set; }
		public string Filter { get { return filter; } set { if (value != filter) { filter = value; titlesView.Refresh(); OnPropertyChanged(); } } }
		public bool IsIdle { get { return isIdle; } set { isIdle = value; OnPropertyChanged(); } }
		public string Log { get { return logEntry; } set { logEntry = value; OnPropertyChanged(); } }
		public Title SelectedItem { get; set; }
		public bool SelectedItemDlcAvailable
		{
			get
			{
				if (SelectedItem == null)
				{
					return false;
				}
				if (SelectedItem.TitleType.Contains("game", StringComparison.OrdinalIgnoreCase) && !SelectedItem.TitleType.Contains("update", StringComparison.OrdinalIgnoreCase))
				{
					return Titles.FirstOrDefault(o => o.TitleId == "0005000C" + SelectedItem.TitleId.Substring(8, 8)) != null; ;
				}
				return false;
			}
		}

		public bool SelectedItemUpdateAvailable
		{
			get
			{
				if (SelectedItem == null)
				{
					return false;
				}
				if (SelectedItem.TitleType.Contains("game", StringComparison.OrdinalIgnoreCase) && !SelectedItem.TitleType.Contains("dlc", StringComparison.OrdinalIgnoreCase))
				{
					return Titles.FirstOrDefault(o => o.TitleId == "0005000E" + SelectedItem.TitleId.Substring(8, 8)) != null; ;
				}
				return false;
			}
		}

		//public List<Title> Titles { get; set; }
		private ObservableCollection<Title> titles;

		public ObservableCollection<Title> Titles
		{
			get { return titles; }
			set { titles = value; OnPropertyChanged(); }
		}

		public string WindowTitle { get { return windowTitle; } set { windowTitle = value; OnPropertyChanged(); } }
		#endregion Public Properties


		#region Private Methods

		// https://stackoverflow.com/a/2082893
		private static string FormatBytes(ulong bytes)
		{
			string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
			int i;
			double dblSByte = bytes;
			for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
			{
				dblSByte = bytes / 1024.0;
			}

			return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
		}

		private void AddLogEntry(string line)
		{
			lock (__lockObj)
			{
				Log += DateTime.Now.ToString("HH:mm:ss") + " | " + line + Environment.NewLine;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0063:Einfache using-Anweisung verwenden", Justification = "<Ausstehend>")]
		private void DownloadTitle(string t)
		{
			IsIdle = false;

			//Title title = Titles.Find(o => o.TitleId == t);
			Title title = Titles.First(o => o.TitleId == t);

			WindowTitle = $"[{title.Region}] [{title.TitleType}] {(String.IsNullOrEmpty(title.Name) ? title.TitleId : title.Name)}";

			AddLogEntry("Prepare download...");

			string baseUrl = SettingsProvider.Settings.NintendoBaseUrl + title.TitleId + "/";
			string saveDir = SettingsProvider.Settings.SavePath;

			if (String.IsNullOrEmpty(title.Name))
			{
				saveDir = @$"{saveDir}{title.TitleType}\\{title.TitleId.Replace(" ", "")}";
			}
			else
			{
				saveDir = @$"{saveDir}{title.TitleType}\\[{title.Region}] {title.NameSanitized}";
			}

			if (!Directory.Exists(saveDir))
			{
				Directory.CreateDirectory(saveDir);
			}

			Task.Run(() =>
			{
				UInt64 titleSize = 0;
				Ticket ticket;
				Tmd tmd;

				try
				{
					using (WebClient wc = new WebClient())
					{
						try
						{
							AddLogEntry(" + Downloading TMD from Nintendo CDN...");
							tmd = new Tmd(wc.DownloadData(baseUrl + "tmd"));

							AddLogEntry(" + Saving TMD - title.tmd");
							File.WriteAllBytes(saveDir + "\\title.tmd", tmd.ExportTmdData());
						}
						catch (WebException e)
						{
							AddLogEntry(" + ERROR! Could not download TMD!");
							Debug.WriteLine(e.Message);
							throw e;
						}
						catch (IOException e)
						{
							AddLogEntry(" + ERROR! Could not save title.tmd!");
							Debug.WriteLine(e.Message);
							throw e;
						}

						for (int i = 0; i < tmd.GetContentCount(); i++)
							titleSize += tmd.GetContentSize((uint)i);

						AddLogEntry($" + Estimated Content Size: {FormatBytes(titleSize)}");

						WindowTitle += " " + FormatBytes(titleSize);

						string titleType = title.TitleType; ;

						if (!(titleType.Contains("game", StringComparison.OrdinalIgnoreCase) || titleType.Contains("demo", StringComparison.OrdinalIgnoreCase) || titleType.Contains("dlc", StringComparison.OrdinalIgnoreCase)))
						{
							AddLogEntry(" + Downloading Ticket from Nintendo CDN...");
							try
							{
								byte[] cetk = wc.DownloadData(baseUrl + "cetk");
								byte[] tik = new byte[0x350];

								for (int i = 0; i < tik.Length; i++)
									tik[i] = cetk[i];

								ticket = new Ticket(tik);

								AddLogEntry(" + Saving Ticket: title.tik");
								try { File.WriteAllBytes(saveDir + "\\title.tik", ticket.ExportTicketData()); }
								catch (IOException e) { AddLogEntry(" + ERROR! Could not save title.tik!"); Debug.WriteLine(e.Message); throw e; }
							}
							catch (WebException e)
							{
								AddLogEntry(" + ERROR! Could not download Ticket from Nintendo CDN!");
								Debug.WriteLine(e.Message);
								throw e;
							}
						}
						else
						{
							AddLogEntry(" + Generating Fake Ticket...");
							ticket = new Ticket();
							ticket.PatchTitleID(title.TitleId);
							ticket.PatchTitleKey(title.TitleKey);
							ticket.PatchTitleVersion(tmd.GetTitleVersion());

							if (titleType.Contains("dlc", StringComparison.OrdinalIgnoreCase))
							{
								AddLogEntry(" + Patching Ticket: Applying \"DLC Unlock All\" Patch");
								ticket.PatchDLCUnlockAll();
							}

							if (titleType.Contains("demo", StringComparison.OrdinalIgnoreCase))
							{
								AddLogEntry(" + Patching Ticket: Applying \"Demo Remove Time Limit\" Patch");
								ticket.PatchDemoKillTimeLimit();
							}

							AddLogEntry(" + Saving Ticket: title.tik");
							try { File.WriteAllBytes(saveDir + "\\title.tik", ticket.ExportTicketData()); }
							catch (IOException e) { AddLogEntry(" + ERROR! Could not save title.tik!"); Debug.WriteLine(e.Message); throw e; }
						}

						AddLogEntry(" + Saving title.cert...");
						try { File.WriteAllBytes(saveDir + "\\title.cert", Utils.GetByteArrayFromHexString("00010003704138EFBBBDA16A987DD901326D1C9459484C88A2861B91A312587AE70EF6237EC50E1032DC39DDE89A96A8E859D76A98A6E7E36A0CFE352CA893058234FF833FCB3B03811E9F0DC0D9A52F8045B4B2F9411B67A51C44B5EF8CE77BD6D56BA75734A1856DE6D4BED6D3A242C7C8791B3422375E5C779ABF072F7695EFA0F75BCB83789FC30E3FE4CC8392207840638949C7F688565F649B74D63D8D58FFADDA571E9554426B1318FC468983D4C8A5628B06B6FC5D507C13E7A18AC1511EB6D62EA5448F83501447A9AFB3ECC2903C9DD52F922AC9ACDBEF58C6021848D96E208732D3D1D9D9EA440D91621C7A99DB8843C59C1F2E2C7D9B577D512C166D6F7E1AAD4A774A37447E78FE2021E14A95D112A068ADA019F463C7A55685AABB6888B9246483D18B9C806F474918331782344A4B8531334B26303263D9D2EB4F4BB99602B352F6AE4046C69A5E7E8E4A18EF9BC0A2DED61310417012FD824CC116CFB7C4C1F7EC7177A17446CBDE96F3EDD88FCD052F0B888A45FDAF2B631354F40D16E5FA9C2C4EDA98E798D15E6046DC5363F3096B2C607A9D8DD55B1502A6AC7D3CC8D8C575998E7D796910C804C495235057E91ECD2637C9C1845151AC6B9A0490AE3EC6F47740A0DB0BA36D075956CEE7354EA3E9A4F2720B26550C7D394324BC0CB7E9317D8A8661F42191FF10B08256CE3FD25B745E5194906B4D61CB4C2E000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000526F6F7400000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001434130303030303030330000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000007BE8EF6CB279C9E2EEE121C6EAF44FF639F88F078B4B77ED9F9560B0358281B50E55AB721115A177703C7A30FE3AE9EF1C60BC1D974676B23A68CC04B198525BC968F11DE2DB50E4D9E7F071E562DAE2092233E9D363F61DD7C19FF3A4A91E8F6553D471DD7B84B9F1B8CE7335F0F5540563A1EAB83963E09BE901011F99546361287020E9CC0DAB487F140D6626A1836D27111F2068DE4772149151CF69C61BA60EF9D949A0F71F5499F2D39AD28C7005348293C431FFBD33F6BCA60DC7195EA2BCC56D200BAF6D06D09C41DB8DE9C720154CA4832B69C08C69CD3B073A0063602F462D338061A5EA6C915CD5623579C3EB64CE44EF586D14BAAA8834019B3EEBEED3790001000100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100042EA66C66CFF335797D0497B77A197F9FE51AB5A41375DC73FD9E0B10669B1B9A5B7E8AB28F01B67B6254C14AA1331418F25BA549004C378DD72F0CE63B1F7091AAFE3809B7AC6C2876A61D60516C43A63729162D280BE21BE8E2FE057D8EB6E204242245731AB6FEE30E5335373EEBA970D531BBA2CB222D9684387D5F2A1BF75200CE0656E390CE19135B59E14F0FA5C1281A7386CCD1C8EC3FAD70FBCE74DEEE1FD05F46330B51F9B79E1DDBF4E33F14889D05282924C5F5DC2766EF0627D7EEDC736E67C2E5B93834668072216D1C78B823A072D34FF3ECF9BD11A29AF16C33BD09AFB2D74D534E027C19240D595A68EBB305ACC44AB38AB820C6D426560C000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000526F6F742D43413030303030303033000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000143503030303030303062000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000137A080BA689C590FD0B2F0D4F56B632FB934ED0739517B33A79DE040EE92DC31D37C7F73BF04BD3E44E20AB5A6FEAF5984CC1F6062E9A9FE56C3285DC6F25DDD5D0BF9FE2EFE835DF2634ED937FAB0214D104809CF74B860E6B0483F4CD2DAB2A9602BC56F0D6BD946AED6E0BE4F08F26686BD09EF7DB325F82B18F6AF2ED525BFD828B653FEE6ECE400D5A48FFE22D538BB5335B4153342D4335ACF590D0D30AE2043C7F5AD214FC9C0FE6FA40A5C86506CA6369BCEE44A32D9E695CF00B4FD79ADB568D149C2028A14C9D71B850CA365B37F70B657791FC5D728C4E18FD22557C4062D74771533C70179D3DAE8F92B117E45CB332F3B3C2A22E705CFEC66F6DA3772B000100010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000010004919EBE464AD0F552CD1B72E7884910CF55A9F02E50789641D896683DC005BD0AEA87079D8AC284C675065F74C8BF37C88044409502A022980BB8AD48383F6D28A79DE39626CCB2B22A0F19E41032F094B39FF0133146DEC8F6C1A9D55CD28D9E1C47B3D11F4F5426C2C780135A2775D3CA679BC7E834F0E0FB58E68860A71330FC95791793C8FBA935A7A6908F229DEE2A0CA6B9B23B12D495A6FE19D0D72648216878605A66538DBF376899905D3445FC5C727A0E13E0E2C8971C9CFA6C60678875732A4E75523D2F562F12AABD1573BF06C94054AEFA81A71417AF9A4A066D0FFC5AD64BAB28B1FF60661F4437D49E1E0D9412EB4BCACF4CFD6A3408847982000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000526F6F742D43413030303030303033000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000158533030303030303063000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000137A0894AD505BB6C67E2E5BDD6A3BEC43D910C772E9CC290DA58588B77DCC11680BB3E29F4EABBB26E98C2601985C041BB14378E689181AAD770568E928A2B98167EE3E10D072BEEF1FA22FA2AA3E13F11E1836A92A4281EF70AAF4E462998221C6FBB9BDD017E6AC590494E9CEA9859CEB2D2A4C1766F2C33912C58F14A803E36FCCDCCCDC13FD7AE77C7A78D997E6ACC35557E0D3E9EB64B43C92F4C50D67A602DEB391B06661CD32880BD64912AF1CBCB7162A06F02565D3B0ECE4FCECDDAE8A4934DB8EE67F3017986221155D131C6C3F09AB1945C206AC70C942B36F49A1183BCD78B6E4B47C6C5CAC0F8D62F897C6953DD12F28B70C5B7DF751819A98346526250001000100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000")); }
						catch (IOException e) { AddLogEntry(" + ERROR! Could not save title.cert!"); Debug.WriteLine(e.Message); throw e; }
					}

					// --------------------------------
					//
					// PARALLEL DOWNLOAD
					//
					// --------------------------------
					if (SettingsProvider.Settings.DownloadParallel)
					{
						List<string> contentIds = new List<string>();

						// Download Content .app and .h3 files
						for (uint i = 0; i < tmd.GetContentCount(); i++)
						{
							contentIds.Add(tmd.GetContentIDString(i));
						}

						Parallel.ForEach(contentIds, new ParallelOptions { MaxDegreeOfParallelism = SettingsProvider.Settings.MaxDegreeOfParallelism }, (content, state, i) =>
						{
							using (WebClient wc = new WebClient())
							{
								string cidStr = content;

								// .app files
								try
								{
									string contentFilePath = saveDir + "\\" + cidStr + ".app";

									if (SettingsProvider.Settings.SkipExistingFiles && File.Exists(contentFilePath) && (ulong)contentFilePath.GetFileLength() == tmd.GetContentSize((uint)i))
									{
										AddLogEntry(" + File: " + cidStr + ".app exists with correct size, skipping download...");
									}
									else
									{
										int pad = contentIds.Count.ToString().Length;
										AddLogEntry(String.Format(" + Downloading Content No. {0} / {1} - {2}.app ({3} bytes)",
											(i + 1).ToString().PadLeft(pad),
											contentIds.Count.ToString().PadLeft(pad),
											cidStr,
											string.Format("{0:N0}", tmd.GetContentSize((uint)i))));
										wc.DownloadFile(baseUrl + cidStr, saveDir + "\\" + cidStr + ".app");
									}
								}
								catch (WebException e)
								{
									AddLogEntry(" + ERROR! Could not download " + cidStr + ".app");
									throw e;
								}
								catch (IOException e)
								{
									AddLogEntry(" + ERROR! Could not save " + cidStr + ".app");
									throw e;
								}

								if (SettingsProvider.Settings.DownloadH3Files)
								{
									// .h3 files
									try
									{
										AddLogEntry(String.Format(" + Downloading H3 for Content No.{0} from Nintendo CDN - {1}.h3", i + 1, cidStr));
										wc.DownloadFile(baseUrl + cidStr + ".h3", saveDir + "\\" + cidStr + ".h3");
									}
									catch (WebException e)
									{
										if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
											AddLogEntry(String.Format(" + File {0}.h3 not found, ignoring...", cidStr));
										else
										{
											AddLogEntry(" + ERROR! Could not download " + cidStr + ".h3");
											throw e;
										}
									}
									catch (IOException e)
									{
										AddLogEntry(" + ERROR! Could not save " + cidStr + ".h3");
										throw e;
									}
								}
							}
						});
					}

					// --------------------------------
					//
					// SERIAL DOWNLOAD
					//
					// --------------------------------
					else
					{
						using (WebClient wc = new WebClient())
						{
							// Download Content .app and .h3 files
							uint contentCount = tmd.GetContentCount();
							for (uint i = 0; i < contentCount; i++)
							{
								string cidStr = tmd.GetContentIDString(i);

								// .app files
								try
								{
									string contentFilePath = saveDir + "\\" + cidStr + ".app";

									if (SettingsProvider.Settings.SkipExistingFiles && File.Exists(contentFilePath) && (ulong)contentFilePath.GetFileLength() == tmd.GetContentSize(i))
									{
										ulong localFileSize = contentFilePath.GetFileLength();

										AddLogEntry(" + File: " + cidStr + ".app exists with correct size, skipping download...");
									}
									else
									{
										AddLogEntry(String.Format(" + Downloading Content No. {0,3} / {1,3} - {2}.app ({3} bytes)", i + 1, contentCount, cidStr, tmd.GetContentSize(i)));
										wc.DownloadFile(baseUrl + cidStr, saveDir + "\\" + cidStr + ".app");
									}
								}
								catch (WebException e)
								{
									AddLogEntry(" + ERROR! Could not download " + cidStr + ".app");
									Debug.WriteLine(e.Message);
									throw e;
								}
								catch (IOException e)
								{
									AddLogEntry(" + ERROR! Could not save " + cidStr + ".app");
									Debug.WriteLine(e.Message);
									throw e;
								}

								if (SettingsProvider.Settings.DownloadH3Files)
								{
									// .h3 files
									try
									{
										AddLogEntry(String.Format(" + Downloading H3 for Content No.{0} from Nintendo CDN - {1}.h3", i + 1, cidStr));
										wc.DownloadFile(baseUrl + cidStr + ".h3", saveDir + "\\" + cidStr + ".h3");
									}
									catch (WebException e)
									{
										if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
											AddLogEntry(String.Format(" + WARNING: {0}.h3 not found, ignoring...", cidStr));
										else
										{
											AddLogEntry(" + ERROR! Could not download " + cidStr + ".h3");
											Debug.WriteLine(e.Message);
											throw e;
										}
									}
									catch (IOException e)
									{
										AddLogEntry(" + ERROR! Could not save " + cidStr + ".h3");
										Debug.WriteLine(e.Message);

										throw e;
									}
								}
							}
						}
					}

					AddLogEntry(String.Format(" + {0}: {1} downloaded!", title.TitleType, Path.GetFileName(saveDir)));

					if (SettingsProvider.Settings.Decrypt)
					{
						int rc = Decryptor.Decrypt(saveDir);
						if (rc == 0)
						{
							AddLogEntry(String.Format(" + {0}: {1} decrypted!", title.TitleType, Path.GetFileName(saveDir)));
						}
						else
						{
							AddLogEntry(" + DECRYPTION FAILED");
						}
						if (SettingsProvider.Settings.DeleteAppFiles)
						{
							foreach (string f in Directory.EnumerateFiles(saveDir, "*.app")) { File.Delete(f); }
							foreach (string f in Directory.EnumerateFiles(saveDir, "*.h3")) { File.Delete(f); }
							foreach (string f in Directory.EnumerateFiles(saveDir, "title.*")) { File.Delete(f); }
						}
					}
				}
				catch (Exception)
				{
					AddLogEntry(" + ABORTING DOWNLOAD OF THIS TITLE");
				}
				finally
				{
					AddLogEntry(String.Format(" + -------------------------------------------------------------------", Path.GetFileName(saveDir)));
					IsIdle = true;
				}
			});
		}

		private void InitializeCommands()
		{
			CmdDownloadTitle = new RelayCommand(() => DownloadTitle(SelectedItem.TitleId));
			CmdDownloadUpdate = new RelayCommand(() => DownloadTitle("0005000E" + SelectedItem.TitleId.Substring(8, 8)));
			CmdDownloadDlc = new RelayCommand(() => DownloadTitle("0005000C" + SelectedItem.TitleId.Substring(8, 8)));

			CmdCopyName = new RelayCommand(() =>
							{
								if (SelectedItem.Name != null)
								{
									Clipboard.SetText(SelectedItem.Name);
									AddLogEntry("Name copied");
								}
							});

			CmdCopyTitleId = new RelayCommand(() =>
							{
								if (SelectedItem.TitleId != null)
								{
									Clipboard.SetText(SelectedItem.TitleId);
									AddLogEntry("Title-ID copied");
								}
							});
			CmdCopyTitleKey = new RelayCommand(() =>
							{
								if (SelectedItem.TitleKey != null)
								{
									Clipboard.SetText(SelectedItem.TitleKey);
									AddLogEntry("Title-Key copied");
								}
							});

			CmdCopyLog = new RelayCommand(() => Clipboard.SetText(Log));
			CmdClearLog = new RelayCommand(() => Log = String.Empty);
		}

		private void InitializeList()
		{
			Task.Run(() =>
			{

				string fileName = SettingsProvider.Settings.TitleFile;

				string replaceStr = " ";

				AddLogEntry("Reading " + new FileInfo(SettingsProvider.Settings.TitleFile).Name);

				string text = File.ReadAllText(fileName).Replace(@"\r\n", replaceStr).Replace(@"\n", replaceStr).Replace(@"\r", replaceStr);
				text = Regex.Replace(text, @"\s+", " ");

				Titles = JsonConvert.DeserializeObject<ObservableCollection<Title>>(text);

				AddLogEntry($"{Titles.Count} titles parsed");

				titlesView = CollectionViewSource.GetDefaultView(Titles);

				titlesView.Filter = UserFilter;

			});

		}
		private bool UserFilter(object item)
		{
			if (String.IsNullOrEmpty(Filter))
			{
				return true;
			}
			else
			{
				string[] filterWords = Filter.Split();

				return filterWords.All(s => (item as Title).ToString().Contains(s, StringComparison.OrdinalIgnoreCase));
			}
		}

		#endregion Private Methods
	}
}