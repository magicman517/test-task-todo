# Todo App (Test Task)

**Demo:** [test-todo.nullmonkeys.com](https://test-todo.nullmonkeys.com)

---

## Tech Stack

- **Frontend:** Angular, [Spartan UI](https://spartan.ng/) (shadcn for Angular), Tailwind CSS
- **Backend:** .NET 10 (ASP.NET Core Web API, EF Core)
- **Database:** PostgreSQL
- **Orchestration:** .NET Aspire (handles Postgres container, migrations, backend, frontend, and YARP gateway)

---

## Prerequisites

- .NET 10
- Node.js & pnpm
- Docker / Podman

---

## How to Run

Start the application using one of the two options below (Aspire will automatically install npm dependencies, start the PostgreSQL container, run migrations, and launch both backend and frontend):

- **Option A: Using Aspire CLI**
  ```bash
  aspire run
  ```

- **Option B: Running AppHost directly (.NET CLI or IDE)**
  ```bash
  dotnet run --project aspire/AppHost
  ```
  *(or simply run the `AppHost` project from your IDE)*

---

### Local URLs

- **App:** [http://localhost:8080](http://localhost:8080)
- **API Docs (Scalar):** [http://localhost:10001/api/reference](http://localhost:10001/api/reference)
- **Aspire Dashboard:** URL printed in the terminal upon startup (logs, metrics, container status)
