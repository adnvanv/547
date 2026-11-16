#!/bin/sh
# Starts a virtual X11 desktop, a terminal that proves which OS this is, and the StayPlanner app.
# The published linux-x64 folder is mounted at /app (see run-linux.ps1).
set -e

Xvfb :99 -screen 0 1600x1200x24 -nolisten tcp &
sleep 2
fluxbox >/dev/null 2>&1 &
sleep 1

xterm -geometry 158x16+5+850 -fa Monospace -fs 10 -T "Terminal" -e sh -c '
  echo "$ uname -a"; uname -a; echo
  echo "$ grep PRETTY /etc/os-release"; grep PRETTY /etc/os-release; echo
  echo "$ ls -l /app"; ls -l /app; echo
  echo "$ ./StayPlanner &"; echo
  exec sh' &

mkdir -p "$HOME/.config/StayPlanner"
# Reuse the reservations file saved by the Windows run, to show the data file works on both OSes.
if [ -f /seed/reservations.json ]; then cp /seed/reservations.json "$HOME/.config/StayPlanner/reservations.json"; fi

cp /app/StayPlanner "$HOME/StayPlanner"
chmod +x "$HOME/StayPlanner"
cd "$HOME"
./StayPlanner > "$HOME/stayplanner.log" 2>&1 &
# Put the app window in the top-left corner, above the terminal.
xdotool search --sync --name '^Stay Planner$' windowmove 5 5 >/dev/null 2>&1 || true

wait
