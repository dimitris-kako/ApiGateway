# .NET Core 6 API Gateway

This project implements an API Gateway in .NET Core 6 that aggregates data from multiple external APIs, providing a unified endpoint to access a variety of services. The purpose of this gateway is to simplify client-side interactions by consolidating multiple API requests into a single request-response cycle.

## External APIs Used

This API Gateway integrates the following external APIs:
- **Spotify**: Music streaming services. [Developer Documentation](https://developer.spotify.com/documentation)
- **News API**: News articles from various sources. [API Documentation](https://newsapi.org/)
- **OMDb API**: Movie, television, and video game information. [API Documentation](https://www.omdbapi.com/)

## Getting Started

### Prerequisites

- .NET Core 6.0 SDK
- An IDE that supports .NET development (e.g., Visual Studio, Visual Studio Code)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/dimitris-kako/ApiGateway.git
2. Build and Run project
  - Can can either run project locally or use the Dockerfile included in the repo to run it in a Docker container.
  - After running the project you can find the Swagger page at https://localhost:7086/swagger/index.html
![gateway](https://github.com/dimitris-kako/ApiGateway/assets/7683676/764fe2ba-f435-4590-af8c-d957f8bc4731)

### Configuration

Before running the application, you'll need to configure the API keys and other settings for each of the external APIs. Obtain the necessary credentials from Spotify, News API, and OMDb API. Update the `appsettings.json` file with these details:

      {
        "ApiKeys": {
          "NewsApi": {
            "BaseUrl": "https://newsapi.org/v2/",
            "ApiKey": "your_newsapi_key"
          },
          "Spotify": {
            "ClientId": "your_spotify_client_id",
            "ClientSecret": "your_spotify_client_secret",
            "BaseUrl": "https://api.spotify.com/v1/search"
          },
          "MoviesApi": {
            "ApiKey": "your_omdb_api_key",
            "BaseUrl": "http://www.omdbapi.com/"
          }
        }
      }

## Features

This API Gateway includes several advanced features designed to enhance performance, scalability, and functionality:

### Memory Caching

To improve response times and reduce the load on external APIs, this gateway implements memory caching. This feature temporarily stores data retrieved from external APIs in memory. When the same data is requested again, it is served directly from the cache, significantly speeding up response times and reducing unnecessary calls to the external services.

### Parallel Programming with Async/Await

The gateway leverages C#'s asynchronous programming model to handle multiple API requests concurrently. By using async and await, the gateway efficiently manages API calls, especially when aggregating data from different sources. This approach ensures that the gateway remains responsive, even under high load conditions, by not blocking threads unnecessarily.

### Data Filtering

Data filtering capabilities are built into the gateway to allow clients to specify exactly what information they need. This feature helps in reducing the bandwidth and processing time by filtering out unnecessary data directly at the gateway level. Clients can use query parameters to filter the results according to their requirements, such as filtering news by category or movies by release year.

### Global Error Handling Middleware

This API Gateway includes global error handling middleware to manage exceptions. This feature ensures that errors are managed in a unified way, improving the robustness and reliability of the API.

### Logging

This project uses Serilog for logging, providing both console and file logging. Serilog is a powerful logging library that allows for structured logging, making it easier to diagnose issues and understand application behavior.

### Unit Testing
Work in Progress

### Authentication- Authorization 
Not yet implemented! 

### Docker Support

You can find the Dockerfile inside the WebApi project of the solution

## Usage Examples
- Example request
  ```
  curl -X 'GET' \
    'https://localhost:7086/api/Gateway/search?Query=test&NewsSortBy=relevance&NewsLanguage=en&NewsPage=1&NewsPageSize=10&SpotifyPageSize=10&SpotifyOffset=1&MoviesPage=1&MoviesYear=2000' \
    -H 'accept: text/plain'

  Or request URL
    https://localhost:7086/api/Gateway/search?Query=test&NewsSortBy=relevance&NewsLanguage=en&NewsPage=1&NewsPageSize=10&SpotifyPageSize=10&SpotifyOffset=1&MoviesPage=1&MoviesYear=2000

- Example response
  ```
  {
    "articles": [
      {
        "author": "string",
        "title": "string",
        "description": "string",
        "url": "string",
        "urlToImage": "string",
        "publishedAt": "string",
        "content": "string"
      }
    ],
    "totalNewsResults": 0,
    "spotifyTracks": [
      {
        "name": "string",
        "artists": [
          {
            "name": "string"
          }
        ]
      }
    ],
    "totalSpotifyResults": 0,
    "movies": [
      {
        "title": "string",
        "year": 0,
        "type": "string"
      }
    ],
    "totalMoviesResults": 0
  }
