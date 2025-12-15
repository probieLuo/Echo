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

namespace Echo.Views
{
	/// <summary>
	/// PythonEditorView.xaml 的交互逻辑
	/// </summary>
	public partial class PythonEditorView : UserControl
	{
		private FoldingManager? _foldingManager;
		private IndentationFoldingStrategy? _foldingStrategy;
		private DispatcherTimer? _foldingUpdateTimer;
		private EventHandler<DocumentChangeEventArgs>? _docChangedHandler;

		public PythonEditorView()
		{
			InitializeComponent();

			if (Editor == null) return;

			LoadPythonHighlighting();

			// 编辑器选项
			Editor.Options.ConvertTabsToSpaces = true;
			Editor.Options.IndentationSize = 4;

			// 折叠支持（基于缩进）
			_foldingManager = FoldingManager.Install(Editor.TextArea);
			_foldingStrategy = new IndentationFoldingStrategy();

			// 使用定时器对文档改动进行防抖，以降低 UpdateFoldings 调用频率
			_foldingUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
			_foldingUpdateTimer.Tick += (s, e) =>
			{
				_foldingUpdateTimer.Stop();
				TryUpdateFoldings();
			};

			// 初次更新折叠并订阅文档变更
			if (Editor.Document != null)
			{
				// ensure foldings are updated after any initial text is loaded
				Dispatcher.BeginInvoke((Action)(() => TryUpdateFoldings()), DispatcherPriority.ApplicationIdle);
				_docChangedHandler = (s, e) =>
				{
					_foldingUpdateTimer?.Stop();
					_foldingUpdateTimer?.Start();
				};
				Editor.Document.Changed += _docChangedHandler;
			}

			// 简单补全示例：按 Ctrl+Space 弹出固定词汇列表
			Editor.TextArea.KeyDown += TextArea_KeyDown;
			// also subscribe to PreviewKeyDown on the editor control to catch keys earlier
			Editor.PreviewKeyDown += Editor_PreviewKeyDown;

			// 清理
			Unloaded += PythonEditorView_Unloaded;
		}

		private void Editor_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			// mirror the same completion trigger so events handled elsewhere don't block it
			if ((e.Key == Key.Space || e.Key == Key.J) && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
			{
				Debug.WriteLine("PreviewKeyDown: Completion triggered");
				TextArea_KeyDown(sender, e);
			}
		}

		private void LoadPythonHighlighting()
		{
			try
			{
				var asm = typeof(PythonEditorView).Assembly;
				var resourceNames = asm.GetManifestResourceNames();
				Debug.WriteLine($"Assembly resources: {string.Join(", ", resourceNames)}");
				// try safer matching: contains python.xshd
				var resourceName = resourceNames.FirstOrDefault(n => n.EndsWith("python.xshd", StringComparison.OrdinalIgnoreCase)
					|| n.IndexOf("python.xshd", StringComparison.OrdinalIgnoreCase) >= 0);
				IHighlightingDefinition? def = null;
				if (resourceName != null)
				{
					Debug.WriteLine($"Found embedded resource: {resourceName}");
					using var s = asm.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException("Resource stream is null");
					using var xr = XmlReader.Create(s);
					def = HighlightingLoader.Load(xr, HighlightingManager.Instance);
					Debug.WriteLine($"Loaded highlighting from embedded resource, def is null? {def == null}");
				}

				// fallback: try loading from disk (useful during development)
				if (def == null)
				{
					var basePath = AppDomain.CurrentDomain.BaseDirectory;
					var filePath = Path.Combine(basePath, "Resources", "python.xshd");
					if (!File.Exists(filePath))
					{
						// try project relative path
						filePath = Path.Combine(Environment.CurrentDirectory, "Resources", "python.xshd");
					}
					if (File.Exists(filePath))
					{
						using var fs = File.OpenRead(filePath);
						using var xr = XmlReader.Create(fs);
						def = HighlightingLoader.Load(xr, HighlightingManager.Instance);
						Debug.WriteLine($"Loaded python.xshd from file: {filePath}, def is null? {def == null}");
					}
				}

				if (def != null)
				{
					HighlightingManager.Instance.RegisterHighlighting("Python", new[] { ".py" }, def);
					Editor.SyntaxHighlighting = def;
					Debug.WriteLine("Python highlighting loaded and applied.");
					// force redraw of TextView to apply highlighting immediately
					Editor.TextArea.TextView.Redraw();
					// update foldings now that text/higlighting may have been applied
					TryUpdateFoldings();
				}
				else
				{
					Debug.WriteLine("No python highlighting found via embedded resource or fallback file.");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to load python highlighting: {ex}");
			}
		}

		private void TryUpdateFoldings()
		{
			try
			{
				if (_foldingManager == null || _foldingStrategy == null || Editor.Document == null) return;
				_foldingStrategy.UpdateFoldings(_foldingManager, Editor.Document);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"UpdateFoldings failed: {ex}");
			}
		}

		private void TextArea_KeyDown(object sender, KeyEventArgs e)
		{
			// allow both Ctrl+Space and Ctrl+J as a fallback
			if ((e.Key == Key.Space || e.Key == Key.J) && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
			{
				Debug.WriteLine("Completion triggered");
				CompletionWindow? window = null;
				try
				{
					window = new CompletionWindow(Editor.TextArea);
					var list = window.CompletionList.CompletionData;
					list.Add(new MyCompletionData("print"));
					list.Add(new MyCompletionData("def"));
					list.Add(new MyCompletionData("class"));
					window.Show();
					window.Closed += (s, args) => { /* allow GC */ };
					e.Handled = true;
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Completion failed: {ex}");
					window?.Close();
				}
			}
		}

		private void PythonEditorView_Unloaded(object? sender, System.Windows.RoutedEventArgs e)
		{
			try
			{
				if (Editor?.Document != null && _docChangedHandler != null)
				{
					Editor.Document.Changed -= _docChangedHandler;
				}
				_foldingUpdateTimer?.Stop();
				if (_foldingManager != null)
				{
					FoldingManager.Uninstall(_foldingManager);
					_foldingManager = null;
				}
			}
			catch { }
		}
	}

	public class MyCompletionData : ICompletionData
	{
		public MyCompletionData(string text)
		{ Text = text; }

		public ImageSource? Image => null;
		public string Text { get; }
		public object Content => Text;
		public object Description => $"Insert {Text}";
		public double Priority => 0;

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
		{
			textArea.Document.Replace(completionSegment, Text);
		}
	}

	public class IndentationFoldingStrategy
	{
		public void UpdateFoldings(FoldingManager manager, TextDocument document)
		{
			if (manager == null || document == null) return;
			var newFoldings = CreateNewFoldings(document);
			try { manager.UpdateFoldings(newFoldings, -1); } catch (Exception ex) { Debug.WriteLine($"manager.UpdateFoldings threw: {ex}"); }
		}

		private IEnumerable<NewFolding> CreateNewFoldings(TextDocument document)
		{
			var foldings = new List<NewFolding>();
			if (document == null || document.LineCount == 0) return foldings;

			int docLen = document.TextLength;

			// helper to move offset left past any CR/LF
			int SkipTrailingNewlines(int end, int start)
			{
				if (end >= docLen) end = docLen - 1;
				while (end > start && end >= 0)
				{
					char ch = document.GetCharAt(end);
					if (ch == '\r' || ch == '\n') end--; else break;
				}
				return end;
			}

			// Stack-based approach: track start offset and indent
			var stack = new Stack<(int startOffset, int indent)>();
			for (int i = 1; i <= document.LineCount; i++)
			{
				var line = document.GetLineByNumber(i);
				string text = document.GetText(line);
				if (string.IsNullOrWhiteSpace(text)) continue;
				int indent = GetIndent(text);

				while (stack.Count > 0 && indent <= stack.Peek().indent)
				{
					var item = stack.Pop();
					int start = item.startOffset;
					int end = line.Offset - 1; // end just before current line

					end = SkipTrailingNewlines(end, start);

					// Validate range
					if (!(start >= 0 && end > start && end < docLen))
					{
						Debug.WriteLine($"Skipping invalid folding range: start={start}, end={end}, docLen={docLen}");
						continue;
					}

					// Ensure folding does not end inside a line delimiter by checking the character at end and end+1
					if (end + 1 < docLen)
					{
						char next = document.GetCharAt(end + 1);
						if (next == '\n' || next == '\r')
						{
							// This means end points to last visible char but next is newline; that's okay.
						}
					}

					// Additional safety: ensure start and end are not inside the same line delimiter sequence
					var startLine = document.GetLineByOffset(start);
					var endLine = document.GetLineByOffset(end);
					if (startLine == null || endLine == null)
					{
						Debug.WriteLine($"Skipping folding due to null line: start={start}, end={end}");
						continue;
					}

					// If endOffset lies beyond endLine's content (i.e., in delimiter), adjust
					int lineContentEnd = endLine.Offset + Math.Max(0, endLine.Length - (endLine.DelimiterLength > 0 ? endLine.DelimiterLength : 0)) - 1;
					if (end > lineContentEnd)
					{
						end = lineContentEnd;
					}

					if (!(end > start))
					{
						Debug.WriteLine($"Skipping folding after adjustment because end <= start: start={start}, end={end}");
						continue;
					}

					foldings.Add(new NewFolding(start, end) { Name = "..." });
				}

				// If indent greater than previous, push as a new block
				if (stack.Count == 0 || indent > stack.Peek().indent)
				{
					stack.Push((line.Offset, indent));
				}
			}

			// Close remaining blocks at end of document
			int docEnd = document.TextLength;
			while (stack.Count > 0)
			{
				var item = stack.Pop();
				int start = item.startOffset;
				int end = docEnd - 1;

				end = SkipTrailingNewlines(end, start);

				if (!(start >= 0 && end > start && end < docLen))
				{
					Debug.WriteLine($"Skipping invalid final folding range: start={start}, end={end}, docLen={docLen}");
					continue;
				}

				var endLine = document.GetLineByOffset(end);
				int lineContentEndFinal = endLine.Offset + Math.Max(0, endLine.Length - (endLine.DelimiterLength > 0 ? endLine.DelimiterLength : 0)) - 1;
				if (end > lineContentEndFinal) end = lineContentEndFinal;
				if (end <= start)
				{
					Debug.WriteLine($"Skipping final folding after adjustment because end <= start: start={start}, end={end}");
					continue;
				}

				foldings.Add(new NewFolding(start, end) { Name = "..." });
			}

			return foldings.OrderBy(f => f.StartOffset).ToList();
		}

		private int GetIndent(string line)
		{
			int n = 0;
			foreach (var ch in line)
			{
				if (ch == ' ') n++;
				else if (ch == '\t') n += 4;
				else break;
			}
			return n;
		}
	}
}