namespace StockQuoteAlert.Console
{
    public class ConsoleWriter
    {
        public void WriteHeader(string ticker, decimal buyThreshold, decimal sellThreshold)
        {
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine("╔══════════════════════════════════════╗");
            System.Console.WriteLine("║          Stock Quote Alert           ║");
            System.Console.WriteLine("╚══════════════════════════════════════╝");
            System.Console.ResetColor();

            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("  Ativo: ");
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine(ticker);

            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("  Venda: ");
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"≥ R$ {sellThreshold:N2}");

            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("  Compra: ");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"≤ R$ {buyThreshold:N2}");



            System.Console.ResetColor();
            WriteSeparator();
        }

        public void WritePrice(string ticker, decimal price, decimal buyThreshold, decimal sellThreshold, string time)
        {
            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            System.Console.Write($"[{time}] ");

            if (price <= buyThreshold)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.Write($"↓ {ticker,-10} R$ {price,10:N2}");
                System.Console.ForegroundColor = ConsoleColor.DarkGreen;
                System.Console.WriteLine("  [COMPRA]");
            }
            else if (price >= sellThreshold)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.Write($"↑ {ticker,-10} R$ {price,10:N2}");
                System.Console.ForegroundColor = ConsoleColor.DarkRed;
                System.Console.WriteLine("  [VENDA]");
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine($"  {ticker,-10} R$ {price,10:N2}");
            }

            System.Console.ResetColor();
        }

        public void WriteSuccess(string message) => WriteColored(ConsoleColor.Green, $"✓ {message}");

        public void WriteWarning(string message) => WriteColored(ConsoleColor.Yellow, $"⚠ {message}");

        public void WriteInfo(string message) => WriteColored(ConsoleColor.White, message);

        public void WriteError(string message) => WriteColored(ConsoleColor.Red, $"✗ {message}");

        public void WriteHint(string message) => WriteColored(ConsoleColor.DarkGray, $"  {message}");

        public void WriteUsage()
        {
            WriteColored(ConsoleColor.White, "  Uso     : stock-quote-alert <ticker> <preço_venda> <preço_compra>");
            WriteColored(ConsoleColor.White, "  Exemplo : stock-quote-alert PETR4 30.50 28.00");
            WriteColored(ConsoleColor.DarkGray, "  Nota    : O sufixo .SA é adicionado automaticamente se omitido.");
        }

        public void WriteShutdown(string message) => WriteColored(ConsoleColor.Cyan, message);

        public void WriteSeparator() => WriteColored(ConsoleColor.DarkGray, new string('─', 42));

        private static void WriteColored(ConsoleColor color, string message)
        {
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(message);
            System.Console.ResetColor();
        }
    }
}
