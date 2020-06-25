# Publish a trimmed single executable for Linux x64 platforms
# For more info: https://www.hanselman.com/blog/MakingATinyNETCore30EntirelySelfcontainedSingleExecutable.aspx
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true /p:PublishReadyToRun=true /p:PublishReadyToRunShowWarnings=true

# Publish an untrimmed single executable for Linux x64 platforms
# dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true