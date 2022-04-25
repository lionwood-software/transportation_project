using ClosedXML.Excel;
using MongoDB.Driver;
using Repository.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TransportApp.ExternalDataClient;
using TransportApp.ExternalDataClient.Models;
using ReportEntity = TransportApp.EntityModel.Report;
using RouteEntity = TransportApp.EntityModel.Route;
using Carrier = TransportApp.EntityModel.Carrier;
using VehicleAgregation = TransportApp.EntityModel.VehicleAgregation;
using TransportApp.Storage;

namespace Transport.Worker.DailyReports.Services
{
    public class DailyReportSeederService
    {
        private readonly IRepository _repository;
        private readonly TransportApp.EntityModel.ReportVehicleTypes _reportTypesNumber;
        private readonly ExternalDataApiClient _externalDataApiClient;
        private const string _attachmentContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private readonly string _bucketName;
        private readonly IStorageService _storage;
        private readonly ILogger _logger;

        public DailyReportSeederService(IRepository repository, IStorageService storage, ExternalDataApiClient externalDataApiClient)
        {
            _repository = repository;
            _reportTypesNumber = new TransportApp.EntityModel.ReportVehicleTypes();
            _externalDataApiClient = externalDataApiClient;
            _bucketName = "dnipro";
            _storage = storage;
            _logger = Log.Logger;
        }

        public async Task Process()
        {
            var plans = await _externalDataApiClient.GetAsync<List<Plan>>("/db/plans?login=lviv&password=KV5G71bjTlJ551F");
            var apiRoutes = await _externalDataApiClient.GetAsync<List<Route>>("db/routes?login=lviv&password=KV5G71bjTlJ551F");

            foreach (var apiRoute in apiRoutes)
            {
                apiRoute.PlanWeekday = plans.FirstOrDefault(x => x.Route_id == apiRoute.Id && !x.Is_weekend);
                apiRoute.PlanWeekend = plans.FirstOrDefault(x => x.Route_id == apiRoute.Id && x.Is_weekend);
            }

            bool isWeekend = DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday;

            var vehicleAgregation = _repository.GetCollection<VehicleAgregation>().Find(x => x.Day >= DateTime.Now.Date && x.Day < DateTime.Now.AddDays(1).Date).ToList();
            var carriers = vehicleAgregation.GroupBy(x => x.CarrierId);
            var carrierIds = carriers.Select(x => x.Key).ToList();
            var carrierEntities = _repository.GetCollection<Carrier>().Find(x => carrierIds.Contains(x.Id)).ToList();

            var routeIds = carriers.SelectMany(x => x.Select(x => x.RouteId)).ToList();
            var routeEntities = _repository.GetCollection<RouteEntity>().Find(x => routeIds.Contains(x.Id)).ToList();

            // grouped by carrier
            foreach (var item in carriers)
            {
                XLWorkbook workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Звіт");
                ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;

                SetReportHeader(ws);

                var vehicleAggregationByType = item.ToList().GroupBy(x => x.Type).ToList();
                var startLine = 4;
                var contentLine = 7;

                ws.Cell(startLine + 1, 1).Value = Helpers.DateTimeHelper.GetUkraineTime(DateTime.Now).Value.ToString("dd-MM-yyyy HH:mm");

                // grouped by carrier and route type
                foreach (var itemTypeRoute in vehicleAggregationByType)
                {
                    ws.Cell(startLine + 2, 2).Value = _reportTypesNumber[(int)itemTypeRoute.Key];

                    var routesAggregation = itemTypeRoute.ToList().GroupBy(x => x.RouteId);
                    // grouped by carrier => route type => routes
                    foreach (var routeAggregation in routesAggregation)
                    {
                        var routeEntity = routeEntities.FirstOrDefault(x => x.Id == routeAggregation.Key);
                        if (routeEntity == null)
                        {
                            continue;
                        }

                        ws.Cell(contentLine, 3).Value = routeEntity.Number;

                        var forwardRacesSum = routeAggregation.Sum(y => y.ForwardRaces.Where(x => x.IsComplete).Count());
                        var backwardRacesSum = routeAggregation.Sum(y => y.BackwardRaces.Where(x => x.IsComplete).Count());
                        var totalRacesSum = forwardRacesSum + backwardRacesSum;

                        // only bus route has plan data
                        if (routeEntity.Type == TransportApp.EntityModel.VehicleType.Bus)
                        {
                            var apiRoute = apiRoutes.FirstOrDefault(x => x.Number == routeEntity.Number);
                            Plan plan = isWeekend ? apiRoute?.PlanWeekend : apiRoute?.PlanWeekday;
                            var planForwardTrips = plan?.Cars?.Select(x => x.Trip_forward).Sum() ?? 0;
                            var planBackwardTrips = plan?.Cars?.Select(x => x.Trip_backward).Sum() ?? 0;
                            var planTotalTrips = plan?.Cars?.Select(x => x.Trip_total).Sum() ?? 0;

                            ws.Cell(contentLine, 4).Value = planForwardTrips; // plan forward trips
                            ws.Cell(contentLine, 6).Value = planBackwardTrips; // plan backward trips
                            ws.Cell(contentLine, 8).Value = planTotalTrips; // plan total trips

                            // deviation
                            ws.Cell(contentLine, 10).Value = planTotalTrips > 0 ? (planTotalTrips > totalRacesSum ? $"{totalRacesSum - planTotalTrips}" : $"+{totalRacesSum - planTotalTrips}") : "0";
                            ws.Cell(contentLine, 11).Value = planTotalTrips > 0 ? (Math.Round((double)totalRacesSum / (double)planTotalTrips * 100.0, 2) - 100.0).ToString() : "0";
                        }

                        // fact data. displaying for all route types
                        ws.Cell(contentLine, 5).Value = forwardRacesSum;
                        ws.Cell(contentLine, 7).Value = backwardRacesSum;
                        ws.Cell(contentLine, 9).Value = totalRacesSum;

                        ws.Range(contentLine, 1, contentLine, 11).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                        contentLine++;
                    }
                    startLine = contentLine - 1;
                    contentLine += 2;
                }
                ws.Columns().Style.Alignment.WrapText = true;
                ws.Column(1).Width = 15;
                ws.Column(2).Width = 14;

                ws.Range(1, 1, ws.LastRowUsed().RowNumber(), ws.LastColumnUsed().ColumnNumber()).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, ws.LastRowUsed().RowNumber(), ws.LastColumnUsed().ColumnNumber()).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);

                var carrier = carrierEntities.First(x => x.Id == item.Key);
                var fileName = $"Щоденний звіт за {Helpers.DateTimeHelper.GetUkraineTime(DateTime.Now).Value:dd-MM-yyyy HH:mm} Перевізник {carrier.NameUA}";

                var namexlsx = _repository.NewId();
                var bucketName = $"icity-{_bucketName}-reports";
                if (await _storage.CreateBucketIfNotExists(bucketName))
                {
                    await _storage.UploadAsync($"{namexlsx}.xlsx", bucketName, stream, _attachmentContentType);
                }

                var dailyReport = new ReportEntity
                {
                    Attachments = new List<TransportApp.EntityModel.Attachment>() { new TransportApp.EntityModel.Attachment { Id = $"{namexlsx}.xlsx", BucketName = bucketName, ContentType = _attachmentContentType } },
                    CreatedAt = DateTime.Now,
                    Id = _repository.NewId(),
                    CarrierId = carrier.Id,
                    Name = fileName
                };
                _repository.GetCollection<ReportEntity>().InsertOne(dailyReport);
            }
        }
        private void SetReportHeader(IXLWorksheet ws)
        {
            ws.Cell(1, 1).Value = "Дата";
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(1, 1, 4, 1).Merge();

            ws.Cell(1, 2).Value = "Категорія, тип";
            ws.Cell(1, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(1, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(1, 2, 4, 2).Merge();

            ws.Cell(1, 3).Value = "№ м-ту";
            ws.Cell(1, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(1, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(1, 3, 4, 3).Merge();

            ws.Cell(1, 4).Value = "Рейси";
            ws.Cell(1, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(1, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(1, 4, 1, 7).Merge();

            ws.Cell(2, 4).Value = "Прямий напрямок";
            ws.Cell(2, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(2, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(2, 4, 3, 5).Merge();

            ws.Cell(4, 4).Value = "План";
            ws.Cell(4, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(4, 5).Value = "Факт";
            ws.Cell(4, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(2, 6).Value = "Зворотній напрямок";
            ws.Cell(2, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(2, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(2, 6, 3, 7).Merge();

            ws.Cell(4, 6).Value = "План";
            ws.Cell(4, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(4, 7).Value = "Факт";
            ws.Cell(4, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(1, 8).Value = "Всього виконано рейсів";
            ws.Cell(1, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(1, 8, 1, 11).Merge();

            ws.Cell(2, 8).Value = "План";
            ws.Cell(2, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(2, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(2, 8, 4, 8).Merge();

            ws.Cell(2, 9).Value = "Факт";
            ws.Cell(2, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(2, 9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(2, 9, 4, 9).Merge();

            ws.Cell(2, 10).Value = "Відхилен. (+/-)";
            ws.Cell(2, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(2, 10).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(2, 10, 4, 10).Merge();

            ws.Cell(2, 11).Value = "%";
            ws.Cell(2, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(2, 11).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(2, 11, 4, 11).Merge();

            ws.Range(1, 1, ws.LastRowUsed().RowNumber(), ws.LastColumnUsed().ColumnNumber()).Style.Font.Bold = true;
        }
    }
}
