#!/bin/bash

# Start the run once job.
declare -p | grep -Ev 'BASHOPTS|BASH_VERSINFO|EUID|PPID|SHELLOPTS|UID' > /container.env

# Setup a cron schedule
echo "SHELL=/bin/bash
BASH_ENV=/container.env
0 * * * * /app/cron/run-daily-report.sh >> /var/log/cron.log 2>&1" > scheduler.txt

crontab scheduler.txt
cron -f