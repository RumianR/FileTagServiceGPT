#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.


FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apt-get update -y && apt-get install -y libc6-dev libgdiplus libx11-dev libleptonica-dev software-properties-common wget gnupg2 libleptonica-dev ghostscript
RUN apt-get update -y && apt-get install apt-transport-https -y

RUN tee /etc/apt/sources.list.d/notesalexp.list<<EOF
RUN echo "deb https://notesalexp.org/tesseract-ocr5/$(lsb_release -cs)/ $(lsb_release -cs) main" \
   | tee /etc/apt/sources.list.d/notesalexp.list > /dev/null
RUN wget -O - https://notesalexp.org/debian/alexp_key.asc | apt-key add - 
RUN apt-get update -y && apt-get install tesseract-ocr -y    
RUN ls -la /usr/lib/x86_64-linux-gnu


FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OpenAIApp/OpenAIApp.csproj", "OpenAIApp/"]
RUN dotnet restore "OpenAIApp/OpenAIApp.csproj"
COPY . .
WORKDIR "/src/OpenAIApp"
RUN dotnet build "OpenAIApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenAIApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final


WORKDIR /app
COPY --from=publish /app/publish .
RUN ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/x64/libleptonica-1.82.0.so
RUN ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/x64/libtesseract50.so
RUN ln -s /usr/lib/x86_64-linux-gnu/libgs.so.9.53 /app/x64/libgs.so

ENTRYPOINT ["dotnet", "OpenAIApp.dll"]