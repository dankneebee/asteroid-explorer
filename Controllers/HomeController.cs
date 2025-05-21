using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AsteroidExplorer.Services;
using AsteroidExplorer.Models;
using System.IO;
using OfficeOpenXml;
using System.Collections.Generic;

namespace AsteroidExplorer.Controllers
{
    public class HomeController : Controller
    {
        private readonly NasaApiService _nasaApiService;
        private readonly IConfiguration _configuration;

        public HomeController(NasaApiService nasaApiService, IConfiguration configuration)
        {
            _nasaApiService = nasaApiService;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Asteroids(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // sets default dates if not provided
                startDate ??= DateTime.Today;
                endDate ??= startDate.Value.AddDays(7);

                // validates date range
                if (endDate < startDate)
                {
                    endDate = startDate;
                }

                // ensures date range doesn't exceed 7 days
                if ((endDate.Value - startDate.Value).TotalDays > 7)
                {
                    endDate = startDate.Value.AddDays(7);
                }

                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;

                var asteroids = await _nasaApiService.GetAsteroidsAsync(startDate.Value, endDate.Value);
                return View(asteroids);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error fetching asteroid data: {ex.Message}";
                return View(new List<Asteroid>());
            }
        }

        public async Task<IActionResult> ExportAsteroids(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // sets EPPlus license for v7.x
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                // sets default dates if not provided
                startDate ??= DateTime.Today;
                endDate ??= startDate.Value.AddDays(7);

                // validates date range
                if (endDate < startDate)
                {
                    endDate = startDate;
                }

                // ensures date range doesn't exceed 7 days
                if ((endDate.Value - startDate.Value).TotalDays > 7)
                {
                    endDate = startDate.Value.AddDays(7);
                }

                var asteroids = await _nasaApiService.GetAsteroidsAsync(startDate.Value, endDate.Value);

                // creates Excel package
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Asteroids");
                    
                    // adds headers
                    worksheet.Cells[1, 1].Value = "Name";
                    worksheet.Cells[1, 2].Value = "Estimated Diameter (km)";
                    worksheet.Cells[1, 3].Value = "Potentially Hazardous";
                    worksheet.Cells[1, 4].Value = "Close Approach Date";
                    worksheet.Cells[1, 5].Value = "Miss Distance (km)";
                    worksheet.Cells[1, 6].Value = "Orbiting Body";

                    // adds data
                    int row = 2;
                    foreach (var asteroid in asteroids)
                    {
                        worksheet.Cells[row, 1].Value = asteroid.Name;
                        worksheet.Cells[row, 2].Value = asteroid.EstimatedDiameter;
                        worksheet.Cells[row, 3].Value = asteroid.IsPotentiallyHazardous ? "Yes" : "No";
                        worksheet.Cells[row, 4].Value = asteroid.CloseApproachDate;
                        worksheet.Cells[row, 5].Value = asteroid.MissDistance;
                        worksheet.Cells[row, 6].Value = asteroid.OrbitingBody;
                        row++;
                    }

                    // auto-fits columns
                    worksheet.Cells.AutoFitColumns();

                    // sets content type and filename
                    var content = package.GetAsByteArray();
                    var fileName = $"Asteroids_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";
                    
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error exporting asteroid data: {ex.Message}";
                return RedirectToAction(nameof(Asteroids), new { startDate, endDate });
            }
        }

        public async Task<IActionResult> Apod(DateTime? date)
        {
            try
            {
                // sets default date to today if not provided
                if (!date.HasValue)
                    date = DateTime.Today;

                // validates that the date is not in the future
                if (date.Value.Date > DateTime.Today)
                {
                    ViewBag.Error = "Cannot view APOD for future dates. Please select a date up to today.";
                    date = DateTime.Today;
                }

                var apod = await _nasaApiService.GetApodAsync(date.Value);
                return View(apod);
            }
            catch (Exception ex)
            {
                // logs the error
                Console.WriteLine($"Error in APOD action: {ex.Message}");
                
                // returns to the view with an error message
                ViewBag.Error = "An error occurred while fetching the Astronomy Picture of the Day. Please try again.";
                return View(new Apod { Date = date ?? DateTime.Today });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
} 