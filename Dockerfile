# ビルドステージ
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# プロジェクトをコピーしてリストア
COPY *.csproj ./
RUN dotnet restore

# 全ソースコードをコピーしてビルド
COPY . .
RUN dotnet publish -c Release -o out

# ランタイムステージ
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# ビルド出力をコピー
COPY --from=build /app/out .

# ポート設定
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# アプリケーション実行
ENTRYPOINT ["dotnet", "SongList.dll"]
