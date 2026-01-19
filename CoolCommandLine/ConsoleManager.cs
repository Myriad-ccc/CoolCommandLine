namespace CoolCommandLine
{
    public abstract class Entity
    {
        public string ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Entity(int x, int y)
        {
            ID = Random.Shared.Next(10000).ToString(); //just for testing
            X = x;
            Y = y;
        }

        public void SetPos(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Player : Entity
    {
        public int ScreenX { get; set; }
        public int ScreenY { get; set; }
        public int Speed { get; set; }

        public Player(int x, int y) : base(x, y)
        {
            ScreenX = x;
            ScreenY = y;
        }

        public void SetScreenPos(int screenX, int screenY)
        {
            ScreenX = screenX;
            ScreenY = screenY;
        }

        public void SetSpeed(int speed) => Speed = speed;
    }

    public class Enemy : Entity
    {
        public int Speed { get; set; }

        public Enemy(int x, int y) : base(x, y) { }

        public void SetSpeed(int speed) => Speed = speed;
    }

    public static class ConsoleManager
    {
        public readonly record struct Command(Type Category, MethodInfo Method, string ID = null);
        private static readonly Dictionary<Type, HashSet<MethodInfo>> CommandRegistry = [];

        public static void AddCommand(Command command)
        {
            var category = command.Category;
            if (category.IsAbstract) return;
            var method = command.Method;

            if (!CommandRegistry.TryGetValue(category, out _))
                CommandRegistry[category] = [];
            CommandRegistry[category].Add(method);
        }

        public static void TryInvokeCommand(string fullCommand)
        {
            //MessageBox.Show($"{CommandRegistry.Keys.First()}");0
            var tokens = fullCommand.Split(' ');
            var mainArg = tokens[0].ToLower();

            Command command = new(null, null);

            //MessageBox.Show($"{string.Join(", ", CommandRegistry.Keys.Select(x => x.Name).ToArray())} -> {mainArg}");
            var qualifier = CommandRegistry.Keys.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, mainArg));
            if (qualifier != null)
            {
                if (tokens.Length < 2)
                {
                    ConsoleLogger.Log("Provided qualifier but no arguments.", false, Brushes.IndianRed);
                    return;
                }

                var qualifierMethod = CommandRegistry[qualifier].FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x?.Name, tokens[1]));
                if (qualifierMethod != null)
                    command = new(qualifier, qualifierMethod);
            }
            else
            {
                var matches = new List<Type>();
                foreach (var category in CommandRegistry)
                    foreach (var method in category.Value)
                        if (StringComparer.OrdinalIgnoreCase.Equals(method?.Name, mainArg))
                        {
                            matches.Add(category.Key);
                            command = new(null, method);
                        }
                //var matches = CommandRegistry
                //    .Where(x => x.Value.Any(y => StringComparer.OrdinalIgnoreCase.Equals(y?.Name, mainArg)))
                //    .Select(x => x.Key)
                //    .ToList();
                int matchCount = matches.Count;
                //MessageBox.Show($"mainArg:{mainArg},\ncategories: {string.Join(", ", CommandRegistry.Select(x => string.Join(", ", x)))},\ncategories with mainArg: {string.Join(", ", matches)}");

                if (matchCount == 0)
                {
                    ConsoleLogger.LogInvalid(mainArg);
                    return;
                }
                else if (matchCount >= 2)
                {
                    ConsoleLogger.LogTwo(mainArg, $" ambiguous between {string.Join(", ", matches.Select(x => $"[{x.Name}]"))}", Brushes.IndianRed, Brushes.White);
                    return;
                }

                var args = tokens.Skip(qualifier == null ? 1 : 2).ToArray();
                var parameters = command.Method.GetParameters();
                var parsedArgs = new object[parameters.Length];

                try
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        parsedArgs[i] = Convert.ChangeType(args[i], parameters[i].ParameterType);
                    }
                    //MessageBox.Show($"{string.Join(", ", parsedArgs.Select(x => $"[{x.GetType()}]"))}");
                    command.Method.Invoke(qualifier ?? null, parsedArgs);
                }
                catch (Exception e)
                {
                    ConsoleLogger.Log($"{e}");
                    return;
                }
                ConsoleLogger.Log("passed");
            }
        }

        public static void GetAllCommands()
        {
            foreach (var category in CommandRegistry)
                foreach (var method in category.Value)
                    ConsoleLogger.LogMany([category.Key.Name, "->", method?.Name], [Brushes.CornflowerBlue, Brushes.Gray, Brushes.LightBlue]);
        }
    }
}