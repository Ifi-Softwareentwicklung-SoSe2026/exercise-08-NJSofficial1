using System;
using System.Threading;

namespace ProcessSimulator;

// 
public class ProcessEventArgs : EventArgs
{
    public string StepName { get; }
    public int Percent { get; }

    public ProcessEventArgs(string stepName, int percent)
    {
        StepName = stepName;
        Percent = percent;
    }
}


public class ProcessRunner
{
    private readonly string[] _steps =
    {
        "Downloading data",
        "Validating input",
        "Processing records",
        "Generating report",
        "Publishing results",
        "Cleaning up"
    };

    // Definition der einzelnen Events mit optionaler Eventübergabe
    public event EventHandler<ProcessEventArgs>? StepStarted;
    public event EventHandler<ProcessEventArgs>? ProgressChanged;
    public event EventHandler? ProcessCompleted;

    public void Run()
    {
        foreach (string step in _steps)
        {
            // Syntax immer: step -> Um welches Event geht es?, <Fortschritt in Prozent>
            OnStepStarted(new ProcessEventArgs(step, 0));

            for (int percent = 0; percent <= 100; percent += 5)
            {
                OnProgressChanged(new ProcessEventArgs(step, percent));
                // Simulation der benötigten Zeit, die die Ausführung des Schrittes benötigt
                Thread.Sleep(80);
            }

        }
        // Event, das bei Abschluss des Gesamtprozesses getriggert wird
        OnProcessCompleted(EventArgs.Empty);
    }
    
    // invoke = aufrufen, beschwören -> "Beschwöre/Erzeuge" das Event ;-)
    private void OnStepStarted(ProcessEventArgs e)
    {
        StepStarted?.Invoke(this, e);
    }

    protected virtual void OnProgressChanged(ProcessEventArgs e)
    {
        ProgressChanged?.Invoke(this, e);
    }

    protected virtual void OnProcessCompleted(EventArgs e)
    {
        ProcessCompleted?.Invoke(this, e);
    }


}

public static class Anzeigelogik
{
    public static void StartInitialization()
    {
        Console.CursorVisible = false;
        Console.WriteLine("=== Process Simulator ===");
        Console.WriteLine();
    }

    // Subscriber für den Fortschrittsbalken
    public static void OnProgressChanged(object? sender, ProcessEventArgs e)
    {
        const int width = 30;
        const char filledChar = '█';
        const char emptyChar = '░';
        const char barStartChar = '⟦';
        const char barEndChar = '⟧';

        int filled = e.Percent * width / 100;
        string bar = new string(filledChar, filled) + new string(emptyChar, width - filled);
        
        Console.Write($"\r{e.StepName,-22} {barStartChar}{bar}{barEndChar} {e.Percent,3}%");
    }

    // Subscriber für die 50%-Warnung
    public static void OnWarningHalfDone(object? sender, ProcessEventArgs e)
    {
        if (e.Percent == 50)
        {
            Console.WriteLine($"\n [WARNING] '{e.StepName}' is only halfway done.");
        }
    }

    // Subscriber für das Ende eines einzelnen Schrittes
    public static void OnStepFinished(object? sender, ProcessEventArgs e)
    {
        if (e.Percent == 100)
        {
            Console.WriteLine("\n");
        }
    }

    // Subscriber für das Ende des Gesamtprozesses
    public static void OnProcessCompleted(object? sender, EventArgs e)
    {
        Console.CursorVisible = true;
        Console.WriteLine("\nProzess abgeschlossen!");
    }
}

// Einstiegspunkt ins Programm -> neue Main()-Methode
class Program
{
    private static void Main()
    {
        Anzeigelogik.StartInitialization();

        // Erzeugung einer Prozessinstanz
        ProcessRunner runner = new ProcessRunner();


        // Verbindung der Events mit den Subscribern in der UI-Logik 
        runner.ProgressChanged += Anzeigelogik.OnProgressChanged;
        runner.ProgressChanged += Anzeigelogik.OnWarningHalfDone;
        runner.ProgressChanged += Anzeigelogik.OnStepFinished;
        runner.ProcessCompleted += Anzeigelogik.OnProcessCompleted;

        // Starten des Prozesses
        runner.Run();
    }
}