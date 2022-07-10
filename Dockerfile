ARG DOTNET_VERSION=6.0

FROM mcr.microsoft.com/dotnet/sdk:$DOTNET_VERSION           AS sdk
FROM mcr.microsoft.com/dotnet/aspnet:$DOTNET_VERSION-alpine AS runtime

#~~~~~~~~~~~~~~~~~~~~~~~~ restore files extraction stage ~~~~~~~~~~~~~~~~~~~~~~~~#
FROM sdk AS restore
WORKDIR /src

#Copy all project files
COPY . .

#Make a tarball with only the .csproj and .sln files
RUN find . \( -name *.csproj -or -name *.sln -or -name NuGet.Config \) -print0 | tar -cvf restore.tar --null -T -

#Extract the tarball into /restore
RUN mkdir /restore
RUN tar -xvf restore.tar -C /restore

#~~~~~~~~~~~~~~~~~~~~~~~~ build stage ~~~~~~~~~~~~~~~~~~~~~~~~#
FROM sdk AS build

#Copy files for restore
WORKDIR /src

#Copy only the .csproj and .sln files
COPY --from=restore /restore ./

#Restore
RUN dotnet restore

#Copy the rest of the files
COPY src src/

#Build the application
RUN dotnet build -c Release


#~~~~~~~~~~~~~~~~~~~~~~~~ test stage ~~~~~~~~~~~~~~~~~~~~~~~~#
FROM build AS test
#Tests the application
RUN mkdir /src/TestResults
RUN dotnet test --no-build -c Release -r /src/TestResults


#~~~~~~~~~~~~~~~~~~~~~~~~ publish stage ~~~~~~~~~~~~~~~~~~~~~~~~#
FROM build as publish

#Publishes the application
RUN dotnet publish --no-build -c Release -o /app/publish src/ThFnsc.Nobreak


#~~~~~~~~~~~~~~~~~~~~~~~~ final stage ~~~~~~~~~~~~~~~~~~~~~~~~#
FROM runtime AS final

#Basic configs
ENV ASPNETCORE_URLS=http://*:80
EXPOSE 80
WORKDIR /app
HEALTHCHECK --interval=5s --timeout=1s CMD wget --no-verbose --tries=1 --spider http://localhost/ || exit 1

#Copy build files that rarely change
#COPY --from=publish /app/publish/runtimes ./runtimes
COPY --from=publish /app/publish/?? ./
COPY --from=publish /app/publish/??-??* ./

#Copy the important stuff
COPY --from=publish /app/publish/[^ThFnsc.]*.dll ./
COPY --from=publish /app/publish .
COPY --from=test /src/TestResults .

ENTRYPOINT dotnet ThFnsc.Nobreak.dll
