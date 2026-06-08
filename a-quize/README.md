# Realtime Quiz

A clear starter project for a real-time online quiz system using ASP.NET Core, SignalR, Entity Framework Core, and MySQL.

## Features

- Host creates a quiz room and shares a 6-character code
- Simple host login for the admin/host area
- Host chooses quiz name, description, number of questions, and total quiz time
- Host adds questions one by one with four options and a correct answer
- Host publishes the quiz after all questions are added
- Students register or login
- Students see available published quizzes, refreshed live when the host publishes
- Students start a quiz attempt from the available quizzes page
- Overall quiz countdown for the whole attempt
- Previous and next navigation between questions
- Students can change selected answers before submitting
- Auto submit when time expires
- Auto scoring when participants submit the quiz
- Live leaderboard updates for host and participants

## Database

This project is configured for MySQL Community Server or MySQL Workbench:

```json
"Server=localhost;Port=3306;Database=realtime_quiz;User=root;Password=805744;"
```

For a fresh install, create the database first:

```sql
CREATE DATABASE IF NOT EXISTS realtime_quiz;
```

The app uses `Database.EnsureCreated()` on startup, so it creates the required tables automatically.

If you already ran the earlier simple version, reset the database so the new tables and columns are created:

```sql
DROP DATABASE IF EXISTS realtime_quiz;
CREATE DATABASE realtime_quiz;
```

## Run

Install the .NET 8 SDK, then run:

```powershell
dotnet restore
dotnet run
```

Open the shown local URL in your browser.

Default host login password:

```text
admin123
```

You can change it in `appsettings.json` under `Admin:Password`.

## Pages

- `/` home page
- `/Login` host login
- `/Host` create a sample quiz
- `/BuildQuiz/{code}` add questions and publish a quiz
- `/HostRoom/{code}` host console
- `/Leaderboard/{code}` standalone live leaderboard
- `/Register` student registration
- `/StudentLogin` student login
- `/Quizzes` available published quizzes
- `/Play/{code}/{participantId}` participant play screen

## Quiz Flow

The host creates a quiz, enters the planned number of questions, adds each question, then publishes it. Students login, see published quizzes, and click Start Quiz. The quiz uses one overall countdown, lets students move previous/next, change answers, and submit once. If time runs out, the browser submits the answers selected so far and unanswered questions receive zero points.

This is still a starter project, so passwords are stored plainly for simplicity. For production, replace this with ASP.NET Core Identity or hashed passwords.
