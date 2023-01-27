namespace Mattodev.SMiTE
{
	public static class Info
	{
		public static string
			version = "0.1";
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
		};

		static int scrollX = 0;
		static int scrollY = 0;

		static int cursorX = 0;
		static int cursorY = 0;

		static int cursorX2 = cursorX;
		static int cursorY2 = cursorY;

		static void resetcolor()
		{
			Console.BackgroundColor = ConsoleColor.Blue;
			Console.ForegroundColor = ConsoleColor.White;
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
			drawbar(w);
			resetcolor();
			if (scrollY < flCont.Length)
			{
				int y = 0;
				foreach (string ln in flCont[scrollY..])
				{
					resetcolor();
					if (scrollX < ln.Length)
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

						for (int j = ln[scrollX..].Length; j < w; j++)
						{
							if (cursorX - scrollX == j && cursorY - scrollY == y)
								invertcolor();

							Console.Write(' ');

							if (cursorX - scrollX == j && cursorY - scrollY == y)
								invertcolor();
						}


						Console.WriteLine();
					}
					y++;
				}
			}
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
			Console.BackgroundColor = ConsoleColor.Cyan;
			Console.ForegroundColor = ConsoleColor.Black;
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
			int width = 120, height = 26;

			flCont = new string[26];
			flCont[0] = "Welcome to SMiTE v" + Info.version + "!";
			for (int i = 1; i < flCont.Length; i++)
				flCont[i] = "";
			
			while (true)
			{
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
			}
		}
	}
}