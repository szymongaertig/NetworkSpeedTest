FROM mcr.microsoft.com/dotnet/runtime:5.0

RUN echo "APT{Ignore {"gpg-pubkey"; }};" > "/etc/apt/apt.conf"
RUN apt-get update &&  apt-get install -y \
    gnupg1 \
    apt-transport-https \
    dirmngr
RUN export INSTALL_KEY=379CE192D401AB61
RUN apt-key adv --keyserver keyserver.ubuntu.com --recv-keys $INSTALL_KEY
RUN echo "deb https://ookla.bintray.com/debian generic main" | tee /etc/apt/sources.list.d/speedtest.list
RUN apt-get update --allow-insecure-repositories
RUN apt-get install -y --allow-unauthenticated speedtest 

COPY output /app
WORKDIR /app
RUN ls -a
ENTRYPOINT ["dotnet","NetworkSpeedTest.dll"]