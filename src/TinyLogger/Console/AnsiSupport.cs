using System.Runtime.InteropServices;

namespace TinyLogger.Console;

public static partial class AnsiSupport
{
	public static bool TryEnable()
	{
		// Try to enable VT-processing on Windows
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var handle = NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE);

			// Fallback to slow Windows API if console mode can't be accessed
			if (!NativeMethods.GetConsoleMode(handle, out var consoleMode))
			{
				return false;
			}

			if ((consoleMode & NativeMethods.ENABLE_VIRTUAL_TERMINAL_PROCESSING) == 0)
			{
				NativeMethods.SetConsoleMode(handle, consoleMode | NativeMethods.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
			}
		}

		return true;
	}

	private static partial class NativeMethods
	{
		public const int STD_OUTPUT_HANDLE = -11;
		public const int STD_ERROR_HANDLE = -12;

		public const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

#if NET
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[LibraryImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[LibraryImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[LibraryImport("kernel32.dll", SetLastError = true)]
		public static partial IntPtr GetStdHandle(int nStdHandle);
#else
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[DllImport("kernel32.dll")]
		public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[DllImport("kernel32.dll")]
		public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetStdHandle(int nStdHandle);
#endif
	}
}
