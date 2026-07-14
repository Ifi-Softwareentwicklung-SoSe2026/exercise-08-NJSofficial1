using System;
using System.Threading;

namespace ProcessSimulator;
public delegate void ProgressReporter(string stepName, int percent);

internal class Program
{
    private static void Main()
    {
        Console.CursorVisible = false;
        Console.WriteLine("=== Process Simulator ===");
        Console.WriteLine();

        string[] steps =
        {
            "Downloading data",
            "Validating input",
            "Processing records",
            "Generating report",
            "Publishing results",
            "Cleaning up"
        };

        ProgressReporter progressChecker = Anzeigelogik.DrawProgressBar;
        progressChecker += (ProgressReporter) Anzeigelogik.WarningHalfDone + Anzeigelogik.InformationFinished;

        foreach (var step in steps)
        {
            for (int percent = 0; percent <= 100; percent += 5)
            {
                progressChecker(step, percent); 
            }
        }

        Console.CursorVisible = true;
        Console.WriteLine("\nProzess abgeschlossen!");
    }

    
}

public class Anzeigelogik
{
    public static void DrawProgressBar(string stepName, int percent)
    {
        const int width = 30;
        const char filledChar = '█';
        const char emptyChar = '░';
        const char barStartChar = '⟦';
        const char barEndChar = '⟧';

        int filled = percent * width / 100;

        string bar = new string(filledChar, filled) + new string(emptyChar, width - filled);
        // \r -> Zurücksetzen des Cursors an den Zeilenanfang
        Console.Write($"\r{stepName,-22} {barStartChar}{bar}{barEndChar} {percent,3}%");
        Thread.Sleep(80);
    }

    public static void WarningHalfDone(string stepName, int percent)
    {
        if (percent == 50)
        {
            Console.WriteLine($"\n [WARNING] '{stepName}' is only halfway done.");
        }
    }

    public static void InformationFinished(string stepName, int percent)
    {
        if (percent == 100)
        {
            Console.WriteLine();
        }
    }
}

// https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/using-delegates
