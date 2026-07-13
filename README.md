<!--

author:   Volker Göhler, Simon Hörtzsch
email:    volker.goehler@informatik.tu-freiberg.de
version:  0.0.1
language: de
narrator: Deutsch Female

edit: true
date: 2026-07-08

link:   https://raw.githubusercontent.com/vgoehler/LiaScript_CSS_Provider/refs/heads/main/dist/university.css

tags: [Sommersemester2026, Softwareentwicklung, Übung08]

-->

[![LiaScript Course](https://raw.githubusercontent.com/LiaScript/LiaScript/master/badges/course.svg)](https://liascript.github.io/course/?https://raw.githubusercontent.com/Ifi-Softwareentwicklung-SoSe2026/exercise-08-NJSofficial1/main/README.md)

# Aufgabe 08

Softwareentwicklung SoSe2026
============================

## Kontext

In dieser Übung refaktorierst du eine kleine Konsolenanwendung, die einen laufenden Prozess simuliert.
Die aktuelle Implementierung ist bewusst linear aufgebaut: Prozesslogik, Konsolenausgabe, Warnlogik und Abschlussmeldung sind stark miteinander vermischt.

Ziel ist es, das Design schrittweise mit Delegates und anschließend mit Events zu verbessern.

Die Simulation nutzt Fortschrittsbalken, damit der Ablauf im Terminal sichtbar wird.

Lernziele
====================

Nach der Bearbeitung solltest du:

- erklären können, warum eng gekoppelter linearer Code schwer erweiterbar ist,
- einen Delegate definieren und verwenden können,
- Methoden an Delegates oder Events abonnieren können,
- Events aus einer Klasse auslösen können,
- Prozesslogik von UI- bzw. Ausgabelogik trennen können.

## Aufgaben

Alle Aufgabenschritte werden auch durch Issues in GitHub ausgegeben.

Bearbeite immer das aktuell offene Maria-Issue:

- Erstelle für jeden Part einen eigenen Branch.
- Öffne danach eine Pull Request nach `main`.
- Schreibe in den PR-Body `Closes #<Issue-Nummer>`, damit der Agent den Part zuordnen kann.
- Bei Code-Parts muss der `dotnet-build` Workflow grün werden, bevor Lisa die PR freigibt.
- Wenn du festhängst, kommentiere im Issue oder PR mit `/help <deine Frage>`.
- Wenn du nach einem Push erneut Feedback möchtest, kommentiere im PR mit `/review`.

### Startpunkt

Öffne das Projekt in `src/ProcessSimulator` und betrachte `Program.cs`.

Führe das Programm aus:

```bash
cd src/ProcessSimulator
dotnet run
```

Du solltest mehrere simulierte Prozessschritte mit Fortschrittsbalken sehen.

### Aufgabe 1: Lineare Implementierung verstehen

Beantworte vor den Codeänderungen kurz:

1. Welche Teile des Codes gehören zur Prozesslogik?

```text
Zur Prozesslogik gehören alle Teile des Programms, die die Abfolge, die Datenverarbeitung und die *Vorbereitung* der Ausgabelogik steuern. Beispiele:
+ `foreach`-Schleife
+ Berechnung der `filled`-Variable und der `bar`
+ `if`-Prüfbedingungen
```

2. Welche Teile gehören zur Konsolenvisualisierung?

```text
Alle Code-Snippets mit `Console.WriteLine(...)` gehören zur Ausgabelogik in der CL, benso `Console.CursorVisible`.
```


3. Welche Teile wären in einer GUI- oder Webanwendung schwer wiederverwendbar?

```text
Schwer wiederverwendbar wären:
+ WriteLines in der Main-Methode
+ kein implementiertes Logging (Bezug auf Webapp: Es müsste ein weiterer Logstream vorhanden sein.)
```


4. Was müsstest du ändern, wenn du Logging hinzufügen willst?

```text
+ striktes Trennen von Programmlogik und Ausgabelogik ->
Verwenden eigener .cs-files, Klassen, Methoden und Separieren mittels namespaces
+ keine `Console.WriteLine(...)` in der Main-Logik
+ keine Unterscheidung der Loglevels vorhanden -> Warning momentan normal angezeigt
+ bei Erweiterung: fehlende TimeStamps und weitere Platzhalter müssten in jeder Logmessage mit hinzugefügt werden
```


Schreibe dies hier direkt in die `README.md` Datei.

- Ändere auch den Link zum LiaScript Badge am Anfang der Datei.
- Nutze einen Branch und Pull Request.

### Aufgabe 2: Delegate einführen

Kurz erklärt: Ein Delegate ist ein Typ für eine Methoden-Referenz.
Du legst damit fest, welche Signatur ein Aufruf haben muss (Parameter + Rückgabewert).
Der eigentliche Aufrufer kennt dann nur den Delegate-Typ, aber nicht die konkrete Methode.

**Beispiel:**

```csharp
public delegate bool CardRule(Card card);

public static int CountCards(IEnumerable<Card> deck, CardRule rule)
{
	int count = 0;
	foreach (var card in deck)
	{
		if (rule(card))
		{
			count++;
		}
	}
	return count;
}

// Konkrete Regeln, die zur Delegate-Signatur passen:
bool IsHeart(Card c) => c.Suit == Suit.Hearts;
bool IsFaceCard(Card c) => c.Rank is Rank.Jack or Rank.Queen or Rank.King;

int hearts = CountCards(deck, IsHeart);
int faceCards = CountCards(deck, IsFaceCard);
```

Die Methode `CountCards` bleibt unverändert, nur die übergebene Regel wechselt.
Genau dieses Entkoppeln sollst du in dieser Aufgabe mit dem Fortschritts-Callback erreichen.

**Aufgabe:**

Erstelle einen Delegate, mit dem Fortschritt gemeldet werden kann.

Vorgeschlagene Signatur:

```csharp
public delegate void ProgressReporter(string stepName, int percent);
```

Ändere die Prozesssimulation so, dass der Fortschritt nicht mehr direkt ausgegeben wird.
Stattdessen soll bei jeder Fortschrittsänderung der Delegate aufgerufen werden.

### Aufgabe 3: Mehrere Subscriber anbinden

Verwende den Delegate, um mehr als eine Methode aufzurufen:

- eine Methode zeichnet den Fortschrittsbalken,
- eine weitere gibt bei 50 % eine Warnung aus,
- optional kann eine Methode Fortschrittsmeldungen protokollieren.
- optional: Nutze `Console.SetCursorPosition(left, top)` um sicherzustellen das Output mit der Progressbar konsistent bleibt und Textoutput darunter gesetzt wird. `Console.Clear()` löscht den Terminal.

### Aufgabe 4: Auf Events refaktorieren

Kurz erklärt:

- Ein `delegate` beschreibt nur eine Methodensignatur (also: welche Methodenform erlaubt ist).
- Ein `event` nutzt intern genau so einen Delegate-Typ, kapselt ihn aber sicher.
- Von außen kannst du bei einem Event nur abonnieren (`+=`) oder abmelden (`-=`), aber es nicht direkt auslösen.
- Auslösen darf nur die Klasse selbst, die das Event besitzt. Dadurch bleibt die Kontrolle über den Ablauf in der Fachlogik.

Was bedeutet das Generische bei `EventHandler<TEventArgs>`?

- `EventHandler` ohne Generic bedeutet: Es werden keine zusätzlichen Daten mitgegeben (nur `sender` und `EventArgs.Empty`).
- `EventHandler<TEventArgs>` bedeutet: Beim Event werden zusätzliche, typisierte Daten mitgegeben.
- `TEventArgs` ist dabei der Datentyp für diese Zusatzinfos, z. B. `ProgressChangedEventArgs` oder `CardEventArgs`.
- Der Event-Handler hat dann die Form `void Handler(object? sender, TEventArgs e)`.

Beispiel:

```csharp
void OnCardDrawn(object? sender, CardEventArgs e)
{
	Console.WriteLine($"Gezogen: {e.Card}");
}
```

Mini-Beispiel:

```csharp
public sealed class CardEventArgs : EventArgs
{
	public CardEventArgs(Card card) => Card = card;
	public Card Card { get; }
}

public class CardDeck
{
	public event EventHandler<CardEventArgs>? CardDrawn;

	public Card Draw()
	{
		Card card = DrawTopCard();
		CardDrawn?.Invoke(this, new CardEventArgs(card));
		return card;
	}
}

// Von außen erlaubt (Subscriber):
deck.CardDrawn += LogDraw;
deck.CardDrawn += UpdateStatistics;

// Nicht erlaubt von außen:
// deck.CardDrawn?.Invoke(...)
```

Bezug zu dieser Aufgabe:
In Aufgabe 3 hast du mehrere Methoden an einen Delegate gehängt.
In Aufgabe 4 machst du daraus benannte Ereignisse (z. B. `StepStarted`, `ProgressChanged`), damit die Reaktionen klar strukturiert und besser erweiterbar sind.

Erstelle eine Klasse `ProcessRunner` mit Events wie z. B.:

```csharp
public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;
public event EventHandler<ProcessStepEventArgs>? StepStarted;
public event EventHandler<ProcessStepEventArgs>? StepCompleted;
public event EventHandler? ProcessCompleted;
```

`ProcessRunner` soll nichts über die Konsolen-UI wissen.


## Erwartetes Ergebnis

Am Ende soll weiterhin derselbe Prozess simuliert werden, aber mit sauber getrennten Verantwortlichkeiten:

- `ProcessRunner` steuert den simulierten Prozess,
- Event-Argumentklassen beschreiben, was passiert ist,
- UI-/Ausgabeklassen reagieren auf Events,
- neues Verhalten lässt sich ohne Änderung der Prozesslogik ergänzen.
