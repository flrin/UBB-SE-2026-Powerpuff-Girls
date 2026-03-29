# Prerequisites
- Visual Studio 2026
- \[WinUI application development] from the Visual Studio workloads
- MSSM

# Setup
### SQL Server
1. Create a new Database in MSSM
2. Copy the connection string from MSSM
3. Add "Initial Catalog=TestsAndInterviews;" to the connection string
4. In the project in the Env.cs class change the connection string
5. In MSSM run the sql script in the SQL folder from the project

### Project
1. Open Visual Studio 2026
2. git clone https://github.com/flrin/UBB-SE-2026-Powerpuff-Girls
3. Open the .sln file

# Git Workflow
1. Pull the latest code: Always run git pull origin main before starting new work.
2. Create a Feature Branch: Branch off main for your specific task.
3. Format: feature/your-feature-name or bugfix/the-bug-name
4. Checkout to the branch
4. Commit your work
5. Open a Pull Request (PR): Push your branch to the remote repository and open a PR into develop.

One person must review the PR!

# Project Structure
- Views: the UI, only .xaml files
- ViewModels: logic (or controller) for the UI
- Models: classes that represent objects (user, slot etc.)
- Services: business logic 
- Repositories: connects the services and the database
- Helpers: utility classes
- SQL: sql scripts for initializing/changing the database
- Assets: visual or audio elements
