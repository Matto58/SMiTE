namespace Mattodev.SMiTE
{
	public static class Info
	{
		public static string
			version = "0.11";
	}
	public class BarItem
	{
		public string title;
		public string id;
		public Action<string, string, string[]> callback = (t, i, a) => { };
		public Func<string[], string> status = a => "";

		public BarItem(string title, string id)
		{
			this.title = title;
			this.id = id;
		}
		public BarItem(string title, string id, Action<string, string, string[]> callback) : this(title, id)
		{
			this.callback = callback;
		}
		public BarItem(string title, string id, Func<string[], string> status) : this(title, id)
		{
			this.status = status;
		}
		public BarItem(string title, string id, Action<string, string, string[]> callback, Func<string[], string> status) : this(title, id)
		{
			this.callback = callback;
			this.status = status;
		}

		public BarItem OnActivate(Action<string, string, string[]> callback)
		{
			this.callback = callback;
			return this;
		}
		internal BarItem activated(string[] args)
		{
			callback.Invoke(title, id, args);
			return this;
		}
	}

	internal class Program
	{
		static string[] flCont = { };
		static bool exit = false;
		static Dictionary<string, BarItem[]> items = new()
		{
			{ "File", new BarItem[] {
				new("New", "newfl", (t,i,a) =>
				{
					flCont = new string[] {""};
					cursorX = 0; cursorY = 0;
				}, a => "Made a new file"),
				new("Save", "savefl", (t,i,a) => File.WriteAllLines(a[0], flCont), a => $"Saved to {a[0]}"),
				new("Open", "openfl", (t,i,a) => flCont = File.ReadAllLines(a[0]), a => $"Read from {a[0]}"),
				new("Quit", "quit", (t,i,a) => exit = true, a => ""),
			} },
			{ "Edit", new BarItem[] {
				new("Insert", "ins", (t,i,a) =>
					flCont[cursorY] = flCont[cursorY].Insert(cursorX, string.Join(' ', a)),
					a => $"Inserted \"{string.Join(' ', a)}\" at ({cursorX},{cursorY})"),/*
				new("Insert newline", "insnl", (t,i,a) =>
					flCont.Append(""),
					a => $"Inserted newline at EOF"),*/
				new("Place cursor", "pcur", (t,i,a) =>
				{
					cursorX = int.Parse(a[0]);
					cursorY = int.Parse(a[1]);
				}, a => $"Placed cursor from ({cursorX2},{cursorY2}) to ({cursorX},{cursorY})"),
				new("Delete", "del", (t,i,a) =>
					flCont[cursorY] = flCont[cursorY].Remove(cursorX, int.Parse(a[0])),
				a => $"Deleted {a[0]} chars at ({cursorX},{cursorY})"),
			} },
			{ "Customize", new BarItem[] {
				new("Background color", "bg", (t,i,a) => bgColor = (ConsoleColor)int.Parse(a[0]), a => $"Changed background color from {(int)bgColor2} to {(int)bgColor}"),
				new("Foreground color", "fg", (t,i,a) => fgColor = (ConsoleColor)int.Parse(a[0]), a => $"Changed foreground color from {(int)fgColor2} to {(int)fgColor}"),
				new("Bar background color", "bgbar", (t,i,a) => barBgColor = (ConsoleColor)int.Parse(a[0]), a => $"Changed the bar's background color from {(int)barBgColor2} to {(int)barBgColor}"),
				new("Bar foreground color", "fgbar", (t,i,a) => barFgColor = (ConsoleColor)int.Parse(a[0]), a => $"Changed the bar's foreground color from {(int)barFgColor2} to {(int)barFgColor}"),
			} },
			{ "Help", new BarItem[] {
				new("Toggle bar", "tbar", (t,i,a) => showBar = !showBar, a => $"{(showBar ? "Enabled" : "Disabled")} the bar"),
				new("About SMiTE", "about", (t,i,a) => { }, a => $"SMiTE v{Info.version}, by Matto58, 2023"),
			} }
		};

		static int scrollX = 0;
		static int scrollY = 0;

		static int cursorX = 0;
		static int cursorY = 0;

		static int cursorX2 = cursorX;
		static int cursorY2 = cursorY;

		static bool showBar = true;

		static ConsoleColor
			bgColor = ConsoleColor.Blue,
			fgColor = ConsoleColor.White,
			barBgColor = ConsoleColor.Cyan,
			barFgColor = ConsoleColor.Black;

		static ConsoleColor
			bgColor2 = bgColor,
			fgColor2 = fgColor,
			barBgColor2 = barBgColor,
			barFgColor2 = barFgColor;

		static Dictionary<string, string> properties = new();

		static void resetcolor()
		{
			Console.BackgroundColor = bgColor;
			Console.ForegroundColor = fgColor;
		}
		static void invertcolor()
		{
			ConsoleColor temp = Console.BackgroundColor;
			ConsoleColor temp2 = Console.ForegroundColor;
			Console.ForegroundColor = temp;
			Console.BackgroundColor = temp2;
		}
		static void drawui(int w, int h)
		{
			Console.Clear();
			if (showBar) drawbar(w);
			resetcolor();
			if (scrollY < flCont.Length)
			{
				int y = 0;
				foreach (string ln in flCont[scrollY..])
				{
					resetcolor();
					if (!string.IsNullOrWhiteSpace(ln))
					{
						if (scrollX <= ln.Length)
						{
							int x = 0;
							foreach (char c in ln[scrollX..])
							{
								if (cursorX - scrollX == x && cursorY - scrollY == y)
									invertcolor();

								if (x < w)
									Console.Write(c);

								if (cursorX - scrollX == x && cursorY - scrollY == y)
									invertcolor();
								x++;
							}
						}

						for (int j = ln[scrollX..].Length; j < w; j++)
						{
							if (cursorX - scrollX == j && cursorY - scrollY == y)
								invertcolor();

							Console.Write(' ');

							if (cursorX - scrollX == j && cursorY - scrollY == y)
								invertcolor();
						}
					}
					else
						for (int j = 0; j < w; j++)
						{
							if (cursorX - scrollX == j && cursorY - scrollY == y)
								invertcolor();

							Console.Write(' ');

							if (cursorX - scrollX == j && cursorY - scrollY == y)
								invertcolor();
						}


					Console.WriteLine();
					y++;
				}
			}
			resetcolor();
			for (int i = flCont.Length; i < h; i++)
			{
				for (int j = 0; j < w; j++)
				{
					if (cursorX - scrollX == j && cursorY - scrollY == i)
						invertcolor();

					Console.Write(' ');

					if (cursorX - scrollX == j && cursorY - scrollY == i)
						invertcolor();
				}
				Console.ResetColor();
				Console.WriteLine();
				resetcolor();
			}
		}
		static void drawbar(int w)
		{
			Console.BackgroundColor = barBgColor;
			Console.ForegroundColor = barFgColor;
			int w2 = 0;
			int i = 0;
			while (w2 < w)
			{
				Console.Write(" " + (i < items.Count ? items.Keys.ToArray()[i] + " " : ""));
				w2 += 1 + (i < items.Count ? items.Keys.ToArray()[i] + " " : "").Length;
				i++;
			}
			Console.WriteLine();
		}

		static void Main(string[] args)
		{
			string status = "\n";
			int width = 80, height = 20;

			flCont = new string[height];
			flCont[0] = "Welcome to SMiTE v" + Info.version + "!";
			for (int i = 1; i < height; i++) flCont[i] = "";
			
			while (!exit)
			{
				string[] fl =
					File.Exists("settings.txt")
					? File.ReadAllLines("settings.txt")
					: new string[]
					{
						"Bg=" + (int)bgColor,
						"Fg=" + (int)fgColor,
						"BgBar=" + (int)barBgColor,
						"FgBar=" + (int)barFgColor,
						"ShowBar=" + (showBar ? "1" : "0")
					};

				foreach (string ln in fl)
				{
					string[] strings = ln.Split("=");
					properties[strings[0]] = strings[1];
				}

				bgColor = (ConsoleColor)int.Parse(properties["Bg"]);
				fgColor = (ConsoleColor)int.Parse(properties["Fg"]);
				barBgColor = (ConsoleColor)int.Parse(properties["BgBar"]);
				barFgColor = (ConsoleColor)int.Parse(properties["FgBar"]);

				drawui(width, height);
				Console.ResetColor();
				Console.WriteLine(status);
				Console.Write('>');
				string[]? i = Console.ReadLine()?.Split(' ');
				if (i != null)
					foreach (var pair in items)
						foreach (var item in pair.Value)
							if (item.id == i[0])
							{
								status = $"Opened {pair.Key} > {item.title}\n";
								item.activated(i[1..]);
								status += item.status.Invoke(i[1..]);
							}
				cursorX2 = cursorX;
				cursorY2 = cursorY;

				File.WriteAllLines("settings.txt", new string[]
				{
					"Bg=" + (int)bgColor,
					"Fg=" + (int)fgColor,
					"BgBar=" + (int)barBgColor,
					"FgBar=" + (int)barFgColor,
					"ShowBar=" + (showBar ? "1" : "0")
				});
			}
		}
	}
}