
# 3. Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app
COPY  /app/publish ./




EXPOSE 5205
# 4. Set environment variables (optional)
ENTRYPOINT ["dotnet", "MissionGenerator.dll"]
# 6. Run the application
