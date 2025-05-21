# Asteroid Explorer

A web application that displays information about near-Earth asteroids and the Astronomy Picture of the Day (APOD) using NASA's APIs.

## Features

- View near-Earth asteroids with detailed information
- Export asteroid data to Excel format
- Browse the Astronomy Picture of the Day
- Responsive design for all devices
- Modern and intuitive user interface
- When media format is not supported by the website's functionality, it displays a button with a link to NASA's website for viewing there instead.
- Users cannot choose a future date

## Technologies Used

- HTML5
- CSS3
- JavaScript (ES6+)
- Bootstrap 5
- Font Awesome
- NASA APIs
- SheetJS for Excel export

## Setup

1. Clone the repository:
```bash
git clone https://github.com/yourusername/AsteroidExplorer.git
```

2. Get a NASA API key:
   - Visit [NASA API Portal](https://api.nasa.gov/)
   - Sign up for an account
   - Generate an API key

3. Update the API key:
   - Open `appsettings.json`
   - Replace `'YOUR API KEY'` with your actual NASA API key

4. Run the project from command line:
```bash
dotnet clean
dotnet build
dotnet run
```
