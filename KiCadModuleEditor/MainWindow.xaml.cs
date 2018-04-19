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
         OverallStatuslabel.Content = "";
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
            CheckFilenamesAndNames();
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
                  module.LinkTo3DModel = new Regex(@"\(model\s(.*)\n").Match(content).Groups[1].Value;
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
            ContentRtb.Document.Blocks.Clear();
            ContentRtb.Document.Blocks.Add(new Paragraph(new Run(CurrentKicadModule.Content)));

               //to get RichTextBox text:
               //string richText = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd).Text;

         }
      }

      private KicadModule GetModuleByFileName(string filename)
      {
         return ModulesList.Find(x => x.FileName == filename);
      }

      private bool CheckFilenamesAndNames()
      {
         var result = false;
         foreach (var item in ModulesList)
         {
            if (ModuleFilenametb != null)
            {
               if (System.IO.Path.GetFileNameWithoutExtension(item.FileName) != item.Name)
               {
                  OverallStatuslabel.Content = "All OK";
                  OverallStatuslabel.Foreground = new SolidColorBrush(Colors.White);
                  OverallStatuslabel.Background = new SolidColorBrush(Colors.Green);
               }
               else
               {
                  OverallStatuslabel.Content = "ERROR exists!!";
                  OverallStatuslabel.Foreground = new SolidColorBrush(Colors.Red);
                  OverallStatuslabel.Background = new SolidColorBrush(Colors.Yellow);
                  result = true;
               }

            }
         }
         return result;
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
               if ((filename != item.Name)||DoUpdate3DModelPathcheckBox.IsChecked.Value)  //FIXITTTTT!!!!
               {
                  SaveModuleToDisk(item);
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

      private void SaveModuleToDisk(KicadModule item)
      {
         var filename = System.IO.Path.GetFileNameWithoutExtension(item.FileName);
         var curDate = DateTime.Now.ToString().Replace(':', '-').Replace(' ', '_');
         MakeBackupFile(item, curDate);

         ReplaceModuleNameWithFilename(item, filename);
         ReplaceModuleValueWithFilename(item, filename);
			if (DoUpdate3DModelPathcheckBox.IsChecked.Value) ReplaceModuleLinkTo3DModelWithFilename(item, filename);
			AddNewKeywords(item, filename);
         File.WriteAllText(item.Path, item.Content);
      }

		private void ReplaceModuleLinkTo3DModelWithFilename(KicadModule item, string filename)
		{
			var str = Module3DModePathToAddtb.Text;
			item.Content= Regex.Replace(item.Content, @"(\(model\s)(.*)(\n)", "$1" + str + filename + ".wrl$3");
		}

		private void AddNewKeywords(KicadModule item, string filename)
      {
         var newKeywordsList = System.IO.Path.GetFileNameWithoutExtension(item.FileName).Replace('_', ' ').Split(' ').ToList();
         item.KeywordsList.AddRange(newKeywordsList.Where(p2 =>
                item.KeywordsList.All(p1 => p1 != p2)));
         var newKeywords = String.Join(" ", item.KeywordsList.ToArray());
         //Regex.Match(item.Content, @"(\(tags\s*"")(.*)("".*)").Groups[2].Value;
         item.Content = Regex.Replace(item.Content, @"(\(tags\s*"")(.*)("".*)", "$1" + newKeywords + "$3");
      }

      private void ReplaceModuleValueWithFilename(KicadModule item, string filename)
      {
         item.Content = Regex.Replace(item.Content, @"(\(fp_text value )(.*)(\s+\(at)", "$1" + filename + "$3");
      }

      private void ReplaceModuleNameWithFilename(KicadModule item, string filename)
      {
         item.Content = Regex.Replace(item.Content, @"(\(module\s*)(.*)(\s+\(layer)", "$1" + filename + "$3");
      }

      private void MakeBackupFile(KicadModule item, string curDate)
      {
         //FileInfo file = new FileInfo(System.IO.Path.GetDirectoryName(item.Path) + "\\xxx\\");

         FileInfo file = new FileInfo(System.IO.Path.GetDirectoryName(item.Path) + "\\backup_" + curDate + "\\");
         file.Directory.Create(); // If the directory already exists, this method does nothing.
         File.WriteAllText(file.FullName + item.FileName + "bak", item.Content);
      }

      private void Savebutton_Click(object sender, RoutedEventArgs e)
      {
         SaveModuleToDisk(CurrentKicadModule);
      }
   }
}
