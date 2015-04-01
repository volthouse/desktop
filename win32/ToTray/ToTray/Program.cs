using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ToTray {
	static class Program {


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			IntPtr handle = IntPtr.Zero;

			if (!string.IsNullOrEmpty(Environment.CommandLine)) {
				List<string> parameters = new List<string>();				
				foreach (var item in Environment.CommandLine.Split((char)34)) {
					if (item.Length > 1) {
						parameters.Add(item);
					}
				}
				
				if (parameters.Count > 1) {
					string appPath = parameters[1];
					var app = appPath.Split((char)39);
					Process process;
					if(app.Length >= 2) {
						process = Process.Start(app[0], app[1]);
					} else {
						process = Process.Start(appPath);
					}
					process.WaitForInputIdle();
					
					handle = process.MainWindowHandle;
					if (parameters.Count > 3) {
						System.Threading.Thread.Sleep(Int32.Parse(parameters[2].Trim()));
						for(int i = 0; i < 10; i++) {
							IntPtr p = UnsaveNativeMethods.FindWindow(parameters[3], null);
							if (p != IntPtr.Zero) {
								handle = p;
								break;
							}
							System.Threading.Thread.Sleep(500);
						}
					}
				}
			}

			Application.Run(new ToTryHandler(handle));
		}
	}

	public class ToTryHandler : ApplicationContext {

		public ToTryHandler(IntPtr handle) {
			this.notifyIcon = new NotifyIcon();
			this.notifyIcon.Icon = Properties.Resources.Icon;
			this.notifyIcon.ContextMenuStrip = new ContextMenuStrip();
			this.notifyIcon.ContextMenuStrip.ShowCheckMargin = false;
			this.notifyIcon.ContextMenuStrip.ShowImageMargin = false;
			this.notifyIcon.Visible = true;
			this.notifyIcon.MouseClick += handleNotifyIconMouseClick;
			this.notifyIcon.DoubleClick += handleNotifyIconDoubleClick;

			this.localWindowsHook = new LocalWindowsHook(HookType.WH_KEYBOARD_LL);
			this.localWindowsHook.HookInvoked += handleHookInvoked;
			this.localWindowsHook.Install();

			populateContextMenu();

			if (!(handle == IntPtr.Zero)) {
				this.processWrapper = new ProcessWrapper();
				this.processWrapper.Handle = handle;				
				this.processWrapper.ToggleTaskbarButton();
				this.processWrapper.ToggleVisibility();
				setIcon(true);
				this.trackBarHost.Visible = true;
			}
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
		}

		void handleCloseButtonClick(object sender, EventArgs e) {
			this.notifyIcon.Visible = false;
			if (this.processWrapper != null) {
				this.processWrapper.Show();
			}
			Application.Exit();
		}

		void handleHookInvoked(object sender, HookEventArgs e) {
			int vkCode = Marshal.ReadInt32(e.lParam);
			if ((Keys)vkCode == Keys.Pause && e.wParam == (IntPtr)UnsaveNativeMethods.WM_KEYDOWN) {
				toggleCurrentProcessVisibility();
			}
		}

		void handleNotifyIconDoubleClick(object sender, EventArgs e) {
			toggleCurrentProcessVisibility();
		}

		void handleNotifyIconMouseClick(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Right) {
				if (this.notifyIcon.ContextMenuStrip.Items.Count == 0) {					
					populateContextMenu();					
				}
				populateCombo(this.combo);
				this.notifyIcon.ContextMenuStrip.Show();
			}
		}

		void handleProcessComboSelectedIndexChanged(object sender, EventArgs e) {
			this.notifyIcon.ContextMenuStrip.Hide();
			var combo = (ToolStripComboBox)sender;
			this.processWrapper = (ProcessWrapper)combo.SelectedItem;
			if (this.processWrapper.Windowname.Length > 64) {
				this.notifyIcon.Text = this.processWrapper.Windowname.Substring(1, 63);
			} else {
				this.notifyIcon.Text = this.processWrapper.Windowname;
			}
			combo = null;

			this.processWrapper.ToggleTaskbarButton();

			setIcon(true);
			this.trackBarHost.Visible = true;			
		}

		void handleRefreshButtonClick(object sender, EventArgs e) {
			populateCombo(this.combo);
		}

		void handleTransparencyValueChanged(object sender, EventArgs e) {
			var pw = this.processWrapper; //this.combo.SelectedItem as ProcessWrapper;
			if (pw != null) {
				UnsaveNativeMethods.SetWindowLong(
					pw.Handle,
					UnsaveNativeMethods.GWL_EXSTYLE,
					UnsaveNativeMethods.GetWindowLong(
						pw.Handle, UnsaveNativeMethods.GWL_EXSTYLE
					) | UnsaveNativeMethods.WS_EX_LAYERED
				);

				UnsaveNativeMethods.SetLayeredWindowAttributes(
					pw.Handle,
					0,
					(byte)((TrackBar)sender).Value,
					2
				);
			}
		}

		void handleShowInTaskBarButtonClick(object sender, EventArgs e) {
			this.processWrapper.ToggleTaskbarButton();
		}

		private void populateCombo(ToolStripComboBox combo) {
			combo.Items.Clear();
			foreach (var p in System.Diagnostics.Process.GetProcesses()) {
				if (p.MainWindowTitle == string.Empty) {
					continue;
				}
				var pw = new ProcessWrapper()
				{
					Handle = p.MainWindowHandle,
					Windowname = p.MainWindowTitle
				};
				combo.Items.Add(pw);
			}
		}

		private void populateContextMenu() {
			this.combo = new ToolStripComboBox();
			populateCombo(this.combo);
			this.combo.SelectedIndexChanged += handleProcessComboSelectedIndexChanged;
			this.notifyIcon.ContextMenuStrip.Items.Add(this.combo);

			this.trackBar = new TrackBar()
			{
				BackColor = System.Drawing.Color.White,
				Minimum = 0,
				Maximum = 255,
				TickStyle = TickStyle.None,
				Value = 255,
				AutoSize = false,
				Height = 20,
				Width = 125
			};
			this.trackBar.ValueChanged += handleTransparencyValueChanged;
			this.trackBarHost = new ToolStripControlHost(this.trackBar) { Visible = false };
			this.notifyIcon.ContextMenuStrip.Items.Add(this.trackBarHost);

			var button = new ToolStripButton("Refresh");
			button.Click += handleRefreshButtonClick;
			this.notifyIcon.ContextMenuStrip.Items.Add(button);

			this.notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			button = new ToolStripButton("Show In Taskbar");
			button.Click += handleShowInTaskBarButtonClick;
			this.notifyIcon.ContextMenuStrip.Items.Add(button);

			this.notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			button = new ToolStripButton("Close");
			button.Click += handleCloseButtonClick;
			this.notifyIcon.ContextMenuStrip.Items.Add(button);			
		}

		private void setIcon(bool full) {
			var b = this.notifyIcon.Icon.ToBitmap();
			var g = System.Drawing.Graphics.FromImage(b);
			g.Clear(System.Drawing.Color.Transparent);

			if (full) {
				g.FillEllipse(System.Drawing.Brushes.Green, 2, 2, 16, 16);
			} else {
				var pen = new System.Drawing.Pen(System.Drawing.Color.Green);
				pen.Width = 2;
				g.DrawEllipse(pen, 5, 5, 12, 12);
			}

			this.notifyIcon.Icon = System.Drawing.Icon.FromHandle(b.GetHicon());
		}

		private void toggleCurrentProcessVisibility() {
			if (this.processWrapper != null) {
				this.processWrapper.ToggleVisibility();				
				setIcon(this.processWrapper.WindowVisible);
			}
		}	

		private ToolStripComboBox combo;
		private LocalWindowsHook localWindowsHook;
		private NotifyIcon notifyIcon;
		private ProcessWrapper processWrapper;
		private TrackBar trackBar;
		private ToolStripControlHost trackBarHost;
	}

	internal class UnsaveNativeMethods {


		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr FindWindow(string className, string windowName);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr SetParent(HandleRef hWnd, HandleRef hWndParent);

		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


		public static int ModifyStyleEx(IntPtr hWnd, int dwRemove, int dwAdd, long nFlags) {
			if (hWnd != null) {				
				var dwStyle = UnsaveNativeMethods.GetWindowLong(hWnd, GWL_EXSTYLE);

				var dwNewStyle = (dwStyle & ~dwRemove) | dwAdd;
				if (dwStyle == dwNewStyle)
					return -1;

				return UnsaveNativeMethods.SetWindowLong(hWnd, GWL_EXSTYLE, dwNewStyle);
			}
			return -1;
		}

		public const int GWL_EXSTYLE = -20;
		public const int WS_EX_LAYERED = 0x80000;
		public const int LWA_ALPHA = 0x2;
		public const int LWA_COLORKEY = 0x1;
		public const int WS_EX_TRANSPARENT = 0x20;
		public const int WM_KEYDOWN = 0x0100;

		public const int WS_EX_APPWINDOW = 0x00040000;
		public const int WS_EX_TOOLWINDOW = 0x00000080;
	}

	internal class ProcessWrapper {


		public void Show() {
			UnsaveNativeMethods.ShowWindow(Handle, 1);
		}

		public void ToggleVisibility() {
			if (Handle != null) {
				UnsaveNativeMethods.ShowWindow(
					Handle, WindowVisible ? 0 : 1
				);
				WindowVisible = !WindowVisible;				
			}
		}

		public void ToggleTaskbarButton() {
			if (Handle != null) {
				UnsaveNativeMethods.ShowWindow(Handle, 0);
				if (ShowInTaskbar) {
					UnsaveNativeMethods.ModifyStyleEx(
						Handle,	UnsaveNativeMethods.WS_EX_APPWINDOW, UnsaveNativeMethods.WS_EX_TOOLWINDOW, 0
					);
				} else {
					UnsaveNativeMethods.ModifyStyleEx(
						Handle, UnsaveNativeMethods.WS_EX_TOOLWINDOW, UnsaveNativeMethods.WS_EX_APPWINDOW, 0
					);
				}				
				UnsaveNativeMethods.ShowWindow(Handle, 1);
				ShowInTaskbar = !ShowInTaskbar;
			}
		}

		public override string ToString() {
			return Windowname;
		}


		public bool WindowVisible = true;
		public bool ShowInTaskbar = true;
		public string Windowname;
		public IntPtr Handle;
	}

	public class HookEventArgs : EventArgs {
		public int HookCode;
		// Hook code
		public IntPtr wParam;
		// WPARAM argument
		public IntPtr lParam;
		// LPARAM argument
	}


	// Hook Types
	public enum HookType : int {
		WH_JOURNALRECORD = 0,
		WH_JOURNALPLAYBACK = 1,
		WH_KEYBOARD = 2,
		WH_GETMESSAGE = 3,
		WH_CALLWNDPROC = 4,
		WH_CBT = 5,
		WH_SYSMSGFILTER = 6,
		WH_MOUSE = 7,
		WH_HARDWARE = 8,
		WH_DEBUG = 9,
		WH_SHELL = 10,
		WH_FOREGROUNDIDLE = 11,
		WH_CALLWNDPROCRET = 12,
		WH_KEYBOARD_LL = 13,
		WH_MOUSE_LL = 14
	}

	public class LocalWindowsHook {


		public delegate void HookEventHandler(object sender,
		   HookEventArgs e);
		public delegate int HookProc(int code, IntPtr wParam,
		   IntPtr lParam);


		public event HookEventHandler HookInvoked;



		public LocalWindowsHook(HookType hook, HookProc func) {
			m_hookType = hook;
			m_filterFunc = func;
		}

		public LocalWindowsHook(HookType hook) {
			m_hookType = hook;
			m_filterFunc = new HookProc(this.CoreHookProc);
		}



		public int CoreHookProc(int code, IntPtr wParam, IntPtr lParam) {
			if (code < 0)
				return CallNextHookEx(m_hhook, code, wParam, lParam);

			// Let clients determine what to do
			HookEventArgs e = new HookEventArgs();
			e.HookCode = code;
			e.wParam = wParam;
			e.lParam = lParam;
			OnHookInvoked(e);

			// Yield to the next hook in the chain
			return CallNextHookEx(m_hhook, code, wParam, lParam);
		}

		public void Install() {
			Process curProcess = Process.GetCurrentProcess();
			ProcessModule curModule = curProcess.MainModule;
			m_hhook = SetWindowsHookEx(
				m_hookType,
				m_filterFunc,
				GetModuleHandle(curModule.ModuleName),
				0
			);
		}

		public void Uninstall() {
			UnhookWindowsHookEx(m_hhook);
		}


		protected void OnHookInvoked(HookEventArgs e) {
			if (HookInvoked != null)
				HookInvoked(this, e);
		}




		#region Win32 Imports

		[DllImport("user32.dll")]
		protected static extern int CallNextHookEx(
			IntPtr hhook, int code, IntPtr wParam, IntPtr lParam
		);


		[DllImport("user32.dll")]
		protected static extern IntPtr SetWindowsHookEx(
			HookType code, HookProc func, IntPtr hInstance, int threadID
		);

		[DllImport("user32.dll")]
		protected static extern int UnhookWindowsHookEx(IntPtr hhook);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		#endregion
		
		protected IntPtr m_hhook = IntPtr.Zero;
		protected HookProc m_filterFunc = null;
		protected HookType m_hookType;
	}
}


