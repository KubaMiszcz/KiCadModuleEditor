using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using KiCadModuleEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace KiCadModuleEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		List<KicadModule> ModulesList = new List<KicadModule>();
		KicadModule CurrentKicadModule;
		public MainWindow()
		{
			InitializeComponent();
			statuslabel.Content = "";
			ModuleFilenametb.Text = "";
		}

		private void OpenFolderDialog_OnClick(object sender, RoutedEventArgs e)
		{
			var dialog = new CommonOpenFileDialog();
			dialog.IsFolderPicker = false;
			dialog.Multiselect = true;
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				var filenamesList = dialog.FileNames.ToList();
				ModulesList.Clear();
				PopulateModulesList(filenamesList);
				ModuleslistBox.Items.Clear();
				PupulateListBoxWithFileNames(ModulesList);
				ModuleslistBox.SelectedIndex = 0;
			}
		}

		private void PupulateListBoxWithFileNames(List<KicadModule> modulesList)
		{
			foreach (var item in ModulesList)
			{
				ModuleslistBox.Items.Add(item.FileName);
			}
		}

		private void PopulateModulesList(List<string> filenamesList)
		{
			foreach (var item in filenamesList)
			{
				KicadModule module = new KicadModule();
				using (StreamReader sr = new StreamReader(item))
				{
					var content = sr.ReadToEnd();
					module.Content = content;
					try
					{
						module.Path = item;
						module.FileName = System.IO.Path.GetFileName(item);
						module.Name = new Regex(@"\(module\s(.*)\s\(layer").Match(content).Groups[1].Value;
						module.Value = new Regex(@"\(fp_text\svalue\s(.*)\s\(at").Match(content).Groups[1].Value;
						module.LinkToDatasheet = new Regex(@"\(descr\s""(.*)""").Match(content).Groups[1].Value;
						module.KeywordsList = new Regex(@"\(tags\s""(.*)""").Match(content).Groups[1].Value.Split(' ').ToList();
						module.LinkTo3DModel = new Regex(@"\(model\s(.*)\r\n").Match(content).Groups[1].Value;
						ModulesList.Add(module);
					}
					catch (Exception)
					{
						MessageBox.Show("something wrong with regexps", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}
		}

		private void OnItemSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ListBox lstbox = sender as ListBox;
			if (lstbox.Items.Count > 0)
			{

				CurrentKicadModule = GetModuleByFileName(lstbox.SelectedItem.ToString());
				ModuleFilenametb.Text = CurrentKicadModule.FileName;
				ModuleNametb.Text = CurrentKicadModule.Name;
				ModuleValuetb.Text = CurrentKicadModule.Value;
				ModuleDatasheetLinktb.Text = CurrentKicadModule.LinkToDatasheet;
				ModuleKeywordsTagstb.Text = String.Join(" ", CurrentKicadModule.KeywordsList.ToArray());
				Module3DModeLinkdtb.Text = CurrentKicadModule.LinkTo3DModel;
			}
		}

		private KicadModule GetModuleByFileName(string filename)
		{
			return ModulesList.Find(x => x.FileName == filename);
		}

		private void CheckFilenameAndName_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			if (ModuleFilenametb != null)
			{

				if (System.IO.Path.GetFileNameWithoutExtension(ModuleFilenametb.Text) == ModuleNametb.Text)
				{
					statuslabel.Content = "OK";
					statuslabel.Foreground = new SolidColorBrush(Colors.White);
					statuslabel.Background = new SolidColorBrush(Colors.Green);
				}
				else
				{
					statuslabel.Content = "ERROR!!";
					statuslabel.Foreground = new SolidColorBrush(Colors.Red);
					statuslabel.Background = new SolidColorBrush(Colors.Yellow);
				}
			}
		}



		private void UpdateNamesByFilenamesbutton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var i = 0;
				foreach (var item in ModulesList)
				{
					var filename = System.IO.Path.GetFileNameWithoutExtension(item.FileName);
					if (filename != item.Name)
					{
						item.Content = Regex.Replace(item.Content, @"(\(module\s*)(.*)(\s+\(layer)", "$1" + filename + "$3");
						item.Content = Regex.Replace(item.Content, @"(\(fp_text value )(.*)(\s+\(at)", "$1" + filename + "$3");

						//var keywordsTtoAppend

						FileInfo file = new System.IO.FileInfo(System.IO.Path.GetDirectoryName(item.Path)+"\\xxx\\");
						file.Directory.Create(); // If the directory already exists, this method does nothing.
						File.WriteAllText(file.FullName + item.FileName + "xxx", item.Content, Encoding.UTF8);
						i++;
					}
				}
				MessageBox.Show(i + " files updated!", "message", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception)
			{
				throw;
			}
		}

		private void Savebutton_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
