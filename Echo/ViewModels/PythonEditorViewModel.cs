using Echo.IServices;
using Echo.Services.IServices;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Echo.ViewModels
{
	public class PythonDocumentViewModel : BindableBase
	{
		private string _name;
		private string _text;
		private string _filePath;
		private bool _isDirty;

		public string Name { get => _name; set => SetProperty(ref _name, value); }
		public string Text { get => _text; set { if (SetProperty(ref _text, value)) IsDirty = true; } }
		public string FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
		public bool IsDirty { get => _isDirty; set => SetProperty(ref _isDirty, value); }

		public PythonDocumentViewModel(string name, string text = "")
		{
			Name = name;
			Text = text;
			IsDirty = false;
		}
	}

	internal class PythonEditorViewModel : ViewModelBase
	{
		public DelegateCommand SaveCommand { get; }
		public DelegateCommand OpenFolderCommand { get; }
		public DelegateCommand NewDocumentCommand { get; }
		public DelegateCommand SaveAllCommand { get; }
		public DelegateCommand ExecuteCommand { get; }
		public DelegateCommand<PythonDocumentViewModel> CloseDocumentCommand { get; }

		private ObservableCollection<PythonDocumentViewModel> _documents = new ObservableCollection<PythonDocumentViewModel>();
		public ObservableCollection<PythonDocumentViewModel> Documents { get => _documents; set => SetProperty(ref _documents, value); }

		private PythonDocumentViewModel _selectedDocument;
		public PythonDocumentViewModel SelectedDocument { get => _selectedDocument; set => SetProperty(ref _selectedDocument, value); }

		private string _currentFolder;
		public string CurrentFolder { get => _currentFolder; set => SetProperty(ref _currentFolder, value); }

		private int untitledCount = 1;

		public PythonEditorViewModel(IRegionManager manager, INotificationService notification, IAppConfig appConfig) : base(manager, notification, appConfig)
		{
			SaveCommand = new DelegateCommand(OnSave, CanSave).ObservesProperty(() => SelectedDocument);
			OpenFolderCommand = new DelegateCommand(OnOpenFolder);
			NewDocumentCommand = new DelegateCommand(OnNewDocument);
			SaveAllCommand = new DelegateCommand(OnSaveAll);
			ExecuteCommand = new DelegateCommand(OnExecute, CanExecute).ObservesProperty(() => SelectedDocument);
			CloseDocumentCommand = new DelegateCommand<PythonDocumentViewModel>(OnCloseDocument);

			// add a default document
			OnNewDocument();
		}

		private bool CanSave()
		{
			return SelectedDocument != null;
		}

		private void OnSave()
		{
			if (SelectedDocument == null)
			{
				ShowInformation("提示", "没有选中文件");
				return;
			}

			if (!string.IsNullOrEmpty(SelectedDocument.FilePath))
			{
				try
				{
					File.WriteAllText(SelectedDocument.FilePath, SelectedDocument.Text, Encoding.UTF8);
					SelectedDocument.IsDirty = false;
					ShowSuccess("保存", "保存成功: " + SelectedDocument.Name);
				}
				catch (Exception ex)
				{
					ShowError("保存失败", ex.Message);
				}
			}
			else
			{
				SaveAs(SelectedDocument);
			}
		}

		private void SaveAs(PythonDocumentViewModel doc)
		{
			var dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.Filter = "Python Files (*.py)|*.py|All Files (*.*)|*.*";
			if (!string.IsNullOrEmpty(CurrentFolder) && Directory.Exists(CurrentFolder))
			{
				dlg.InitialDirectory = CurrentFolder;
			}
			if (dlg.ShowDialog() == true)
			{
				try
				{
					File.WriteAllText(dlg.FileName, doc.Text, Encoding.UTF8);
					doc.FilePath = dlg.FileName;
					doc.Name = Path.GetFileName(dlg.FileName);
					doc.IsDirty = false;
					ShowSuccess("保存", "保存成功: " + doc.Name);
				}
				catch (Exception ex)
				{
					ShowError("保存失败", ex.Message);
				}
			}
		}

		private void OnOpenFolder()
		{
			using (var dlg = new FolderBrowserDialog())
			{
				dlg.Description = "选择包含 Python 文件的文件夹";
				dlg.ShowNewFolderButton = false;
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					CurrentFolder = dlg.SelectedPath;
					LoadFolder(CurrentFolder);
				}
			}
		}

		private void LoadFolder(string folder)
		{
			try
			{
				var files = Directory.GetFiles(folder, "*.py");
				if (files.Length == 0)
				{
					ShowInformation("打开文件夹", "未找到 .py 文件");
					return;
				}
				Documents.Clear();
				foreach (var f in files)
				{
					var text = File.ReadAllText(f, Encoding.UTF8);
					var doc = new PythonDocumentViewModel(Path.GetFileName(f), text) { FilePath = f };
					Documents.Add(doc);
				}
				SelectedDocument = Documents.FirstOrDefault();
				ShowInformation("打开文件夹", $"已加载 {Documents.Count} 个文件");
			}
			catch (Exception ex)
			{
				ShowError("加载文件夹失败", ex.Message);
			}
		}

		private void OnNewDocument()
		{
			var name = $"Untitled{untitledCount++}.py";
			var doc = new PythonDocumentViewModel(name, "");
			Documents.Add(doc);
			SelectedDocument = doc;
		}

		private void OnSaveAll()
		{
			int saved = 0;
			foreach (var doc in Documents.ToList())
			{
				if (!string.IsNullOrEmpty(doc.FilePath))
				{
					try
					{
						File.WriteAllText(doc.FilePath, doc.Text, Encoding.UTF8);
						doc.IsDirty = false;
						saved++;
					}
					catch { }
				}
			}
			ShowInformation("保存全部", $"已保存 {saved} 个文件 (未指定路径的文件请单独保存)");
		}

		private bool CanExecute()
		{
			return SelectedDocument != null;
		}

		private void OnExecute()
		{
			if (SelectedDocument == null)
			{
				ShowInformation("执行", "没有选中的文件");
				return;
			}
			// UI 层：仅提示执行
			ShowInformation("执行", $"执行: {SelectedDocument.Name}");
		}

		private void OnCloseDocument(PythonDocumentViewModel doc)
		{
			if (doc == null) return;
			if (doc.IsDirty)
			{
				// 简单提示，实际可弹出保存确认
				ShowWarning("未保存", $"文件 {doc.Name} 有未保存的更改");
			}
			Documents.Remove(doc);
			if (SelectedDocument == doc)
			{
				SelectedDocument = Documents.FirstOrDefault();
			}
		}
	}
}