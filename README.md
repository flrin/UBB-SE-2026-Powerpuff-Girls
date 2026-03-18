# Prerequisites
- Visual Studio 2026
- \[WinUI application development] from the Visual Studio workloads
- Docker Desktop

# Setup
### PostgreSQL
1. Open Docker Desktop
2. Open command line to repo folder
3. docker-compose up -d

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
- Services: connects the controller and the database
- Helpers: utility classes

# Notes!
### Adding a .xaml file
1. Right click on views
2. Add
3. New Item...
4. Templates
5. Blank Page

Not .xml file!

### Database not up to date
1. Open repo folder in terminal
2. run docker-compose down -v
3. run docker-compose up -d 
