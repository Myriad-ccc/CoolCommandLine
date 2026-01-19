namespace CoolCommandLine
{
    public static class ConsoleLogger
    {
        public static RichTextBox Logs;

        public static bool IncludeTimeStamps = true;
        private static Paragraph LastDiv => Logs.Document.Blocks.LastBlock as Paragraph;

        private static Brush Output = Brushes.White;
        private static Brush Successful = Brushes.Green;
        private static Brush Error = Brushes.Red;
        private static Brush Critical = Brushes.DarkRed;

        public static void Log(string arg, bool append = false, Brush color = null)
        {
            if (string.IsNullOrWhiteSpace(arg))
                return;

            color ??= Brushes.White;

            Paragraph div;
            if (append && LastDiv != null)
                div = LastDiv;
            else
            {
                div = new()
                {
                    Padding = new(0),
                    Margin = new(0)
                };

                if (IncludeTimeStamps)
                {
                    var ts = new Run($"[{DateTime.Now:HH:mm:ss}]") { Foreground = Brushes.Gray };
                    var floater = new Floater(new Paragraph(ts))
                    {
                        Padding = new(0),
                        Margin = new(0),
                        HorizontalAlignment = HorizontalAlignment.Right,
                    };
                    div.Inlines.Add(floater);
                }
            }

            var span = new Run(arg) { Foreground = color };
            div.Inlines.Add(span);

            Logs.Document.Blocks.Add(div);

            Logs.ScrollToEnd();
        }

        public static void LogTwo(string firstArg, string secondArg, Brush firstColor = null, Brush secondColor = null)
        {
            Log(firstArg, false, firstColor);
            Log(secondArg, true, secondColor);
        }

        public static void LogMany(string[] spans, Brush[] colors)
        {
            Log(spans[0], false, colors[0]);
            for (int i = 1; i < spans.Length; i++)
                if (!string.IsNullOrWhiteSpace(spans[i]))
                    Log(spans[i], true, colors[i]);
        }

        public static void LogSuccessful(string fallOut)
            => Log(fallOut, false, Successful);

        public static void LogInvalid(string arg)
            => LogTwo(arg, " not found.", Error, Output);

        public static void LogNull(string arg)
            => LogTwo(arg, " is null.", Critical, Output);

        public static void Clear() => Logs.Document.Blocks.Clear();
    }
}