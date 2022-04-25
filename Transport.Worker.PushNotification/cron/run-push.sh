#!/bin/bash
timestamp=`date +%Y/%m/%d-%H:%M:%S`
echo "Triggired push at $timestamp"
/usr/bin/dotnet /app/Transport.Worker.PushNotification.dll