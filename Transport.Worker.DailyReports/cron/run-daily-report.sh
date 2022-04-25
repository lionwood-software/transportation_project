#!/bin/bash
timestamp=`date +%Y/%m/%d-%H:%M:%S`
echo "Triggired daily report generation at $timestamp"
/usr/bin/dotnet /app/Transport.Worker.DailyReports.dll