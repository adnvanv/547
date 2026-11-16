# Linux desktop for running the published StayPlanner (linux-x64) build.
# No .NET SDK or runtime is installed: the app is self-contained and was compiled on Windows.
# Xvfb is a virtual X11 display, fluxbox a small window manager, xterm a terminal,
# scrot takes screenshots and xdotool clicks/types into windows.
FROM debian:bookworm-slim

RUN apt-get update && apt-get install -y --no-install-recommends \
        xvfb fluxbox xterm scrot xdotool x11-utils procps \
        libx11-6 libice6 libsm6 libxext6 libxrandr2 libxi6 libxcursor1 libfontconfig1 fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

RUN useradd -m student && mkdir -p /tmp/.X11-unix && chmod 1777 /tmp/.X11-unix
COPY start-desktop.sh /usr/local/bin/start-desktop.sh
RUN chmod +x /usr/local/bin/start-desktop.sh

USER student
WORKDIR /home/student
ENV DISPLAY=:99
CMD ["/usr/local/bin/start-desktop.sh"]
