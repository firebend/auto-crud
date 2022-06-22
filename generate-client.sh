rm -rf ./generated-client
./node_modules/.bin/openapi-generator-cli generate
dotnet build ./generated-client/src/Firebend.AutoCrud.Sample.Client/Firebend.AutoCrud.Sample.Client.csproj
