# Week 5 - Reverse Proxy with YARP
 
A configurable reverse proxy built with Microsoft's [YARP (Yet Another Reverse Proxy)](https://microsoft.github.io/reverse-proxy/) library in C# / .NET 10. Routes incoming traffic to different backend services based on path prefixes, manipulates headers via transforms, and logs all proxied requests through custom middleware.
 
Part of a 12-week project-based learning plan.
 
## Features
 
- **Path-based routing** — incoming requests are routed to different backend clusters based on path prefix (`/api/`, `/web/`, `/admin/`)
- **Path prefix stripping** — prefixes are removed before forwarding so backends receive clean paths
- **Header transforms** — adds `X-Forwarded-By` request header, strips `Server` response header
- **Request logging** — custom middleware logs method, path, status code, and response time for every request using Serilog (console + rolling file output)
- **Basic auth** — `/admin/` routes are protected with HTTP Basic Authentication, credentials configured via appsettings
- **Docker support** — runs in Docker Compose with environment-specific configuration
## Project Structure
 
```
Week5-Reverse-Proxy/
├── Program.cs
├── appsettings.json                 # Full YARP config (routes, clusters, transforms)
├── appsettings.Development.json     # Local dev logging overrides
├── appsettings.Docker.json          # Docker-specific cluster address overrides
├── Middleware/
│   └── RequestLoggingMiddleware.cs
├── Authentication/
│   └── BasicAuthHandler.cs
├── Dockerfile
└── Week5-Reverse-Proxy.csproj
 
TestApi/
├── Program.cs
├── Dockerfile
└── TestApi.csproj
 
docker-compose.yml
```
 
## Routes
 
| Path | Backend | Auth |
|------|---------|------|
| `/api/{**remainder}` | httpbin.org | None |
| `/web/{**remainder}` | TestApi | None |
| `/admin/{**remainder}` | TestApi | Basic |
 
## Running Locally
 
Start the TestApi and reverse proxy from Visual Studio with the `Development` environment. The TestApi runs on `http://localhost:5081` and the proxy on `http://localhost:5222`.
 
## Running in Docker
 
```bash
docker compose up 
```
 
The proxy is available at `http://localhost:5222`. Inside Docker, the proxy reaches the TestApi via the Compose service name (`http://test.api:8080`), configured in `appsettings.Docker.json`.
 
## Testing
 
- `http://localhost:5222/api/get` — proxied to httpbin, returns JSON echo of the request including the `X-Forwarded-By` header
- `http://localhost:5222/admin/get` — same as above but prompts for Basic auth credentials
- `http://localhost:5222/web/weatherforecast` — proxied to TestApi, returns sample weather data

## Tech
 
- .NET 10
- YARP (Yarp.ReverseProxy)
- Serilog
- Docker
