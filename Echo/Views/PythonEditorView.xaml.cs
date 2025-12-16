using ICSharpCode.AvalonEdit.CodeCompletion; // 若使用补全
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Windows.Threading;
using System.Diagnostics;
using Echo.ViewModels;
using Microsoft.Win32;

namespace Echo.Views
{
	/// <summary>
	/// PythonEditorView.xaml 的交互逻辑
	/// </summary>
	public partial class PythonEditorView : UserControl
	{
		public PythonEditorView()
		{
			InitializeComponent();
		}
	}
}