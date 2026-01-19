global using System.Windows;
global using System.Windows.Input;
global using System.Windows.Media;
global using System.Reflection;
global using System.Windows.Controls;
global using System.Windows.Documents;

namespace CoolCommandLine
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CommandLine.Focus();
            ConsoleLogger.Logs = ConsoleLogs;

            // Create test entities
            var player = new Player(10, 20);
            var enemy = new Enemy(50, 80);

            // PLAYER commands (instance-bound)
            ConsoleManager.AddCommand(new ConsoleManager.Command(
                typeof(Player),
                typeof(Player).GetMethod(nameof(Player.SetPos))!,  // instance method
                player.ID // bind to this player instance
            ));

            ConsoleManager.AddCommand(new ConsoleManager.Command(
                typeof(Player),
                typeof(Player).GetMethod(nameof(Player.SetScreenPos))!,
                player.ID
            ));

            ConsoleManager.AddCommand(new ConsoleManager.Command(
                typeof(Player),
                typeof(Player).GetMethod(nameof(Player.SetSpeed))!,
                player.ID
            ));

            // ENEMY commands (instance-bound)
            ConsoleManager.AddCommand(new ConsoleManager.Command(
                typeof(Enemy),
                typeof(Enemy).GetMethod(nameof(Enemy.SetPos))!, // inherits SetPos from Entity - use typeof(Enemy).GetMethod or typeof(Entity).GetMethod
                enemy.ID
            ));

            ConsoleManager.AddCommand(new ConsoleManager.Command(
                typeof(Enemy),
                typeof(Enemy).GetMethod(nameof(Enemy.SetSpeed))!,
                enemy.ID
            ));

            // ENTITY-level commands (type-scoped / unbound) — useful to force qualifier usage
            ConsoleManager.AddCommand(new ConsoleManager.Command(
                typeof(Entity),
                typeof(Entity).GetMethod(nameof(Entity.SetPos))!,
                null // unbound: you must qualify with which entity (or resolver chooses)
            ));

            // another unambiguous player-only command (type-scoped example)
            ConsoleManager.AddCommand(new ConsoleManager.Command(
                typeof(Player),
                typeof(Player).GetMethod(nameof(Player.SetScreenPos))!, // duplicate type-scoped; you already have instance-bound one
                null // registering an unbound variant to test qualifier resolving
            ));
        }

        private void CommandLine_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var text = CommandLine.Text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    CommandLine.Clear();
                    if (text == "get")
                    {
                        ConsoleManager.GetAllCommands();
                        return;
                    }
                    ConsoleManager.TryInvokeCommand(text);
                }
            }
        }
    }
}