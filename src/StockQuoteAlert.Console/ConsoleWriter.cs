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
            System.Console.Write("  Ativo  : ");
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine(ticker);

            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("  Compra : ");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"≤ {buyThreshold:C}");

            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("  Venda  : ");
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"≥ {sellThreshold:C}");

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
                System.Console.Write($"↓ {ticker,-10} {price,10:C}");
                System.Console.ForegroundColor = ConsoleColor.DarkGreen;
                System.Console.WriteLine("  [COMPRA]");
            }
            else if (price >= sellThreshold)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.Write($"↑ {ticker,-10} {price,10:C}");
                System.Console.ForegroundColor = ConsoleColor.DarkRed;
                System.Console.WriteLine("  [VENDA]");
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine($"  {ticker,-10} {price,10:C}");
            }

            System.Console.ResetColor();
        }

        public void WriteSuccess(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"✓ {message}");
            System.Console.ResetColor();
        }

        public void WriteWarning(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine($"⚠ {message}");
            System.Console.ResetColor();
        }

        public void WriteInfo(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine(message);
            System.Console.ResetColor();
        }

        public void WriteError(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"✗ {message}");
            System.Console.ResetColor();
        }

        public void WriteSeparator()
        {
            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            System.Console.WriteLine(new string('─', 42));
            System.Console.ResetColor();
        }
    }
}
