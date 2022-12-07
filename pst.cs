using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pst
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // -m オプションがついてる時は大文字小文字を区別する
            bool ignoreCase = !args.Any(x => x == "-m");
            var searchWords = ignoreCase ? args.Where(x => x != "-m").Select(x => x.ToLower()) : args.Where(x => x != "-m");
            
            string path = Directory.GetParent(Application.ExecutablePath).FullName;
            StreamReader pst = new StreamReader(path + @"\PtrSafe.Txt");

            while (!pst.EndOfStream)
            {
                StringBuilder buf = new StringBuilder();
                string line = pst.ReadLine();

                // ブロック単位で読み込む
                if (line.StartsWith("#If"))
                {
                    buf.AppendLine(line);
                    while (!line.StartsWith("#End If"))
                    {
                        line = pst.ReadLine();
                        // インデントを調整
                        if (line.StartsWith("        ") && line.Length > 8) line = line.Substring(4);
                        if (!line.StartsWith("#")) line = "    " + line;
                        buf.AppendLine(line);
                    }
                }
                else if (line.StartsWith("Enum")
                    || (line.StartsWith("Function"))
                    || (line.StartsWith("Sub"))
                    || (line.StartsWith("Type")))
                {
                    buf.AppendLine(line);
                    while (!line.StartsWith("End"))
                    {
                        line = pst.ReadLine();
                        if (line.StartsWith("        ") && line.Length > 8) line = line.Substring(4);
                        buf.AppendLine(line);
                    }
                }
                else
                {
                    buf.AppendLine(line);
                }

                string lines = buf.ToString();

                // 全ての検索条件に一致したら候補としてコンソールに表示する
                if (searchWords.All(x => (ignoreCase ? lines.ToLower() : lines).Contains(x)))
                {
                    Console.WriteLine();
                    Console.WriteLine(new string('=', 20));
                    string[] ls = lines.Split('\n');
                    for (int i = 1; i < ls.Length; i++)
                        Console.WriteLine(i.ToString().PadLeft(3) + ": " + ls[i - 1]);
                    Console.WriteLine(new string('=', 20));

                    // ユーザーの選択待ち
                    string res;
                    do
                    {
                        Console.Write("    => Is this what you are looking for?  y/N/cancel : ");
                        res = Console.ReadLine().ToLower();
                    } while (!(new string[] { "", "n", "no", "y", "yes", "c", "cancel" }).Any(s => s == res));

                    // キャンセルなら即終了
                    if ((new string[] { "c", "cancel" }).Any(s => s == res))
                    {
                        pst.Close();
                        return;
                    }

                    // yesならクリップボードに放り込む
                    if ((new string[] { "y", "yes" }).Any(s => s == res))
                    {
                        Clipboard.SetText(lines.TrimEnd('\n'));
                        Console.WriteLine("Copied!");
                        pst.Close();
                        return;
                    }
                }
            }
            Console.WriteLine("Not Found.");
            pst.Close();
        }
    }
}