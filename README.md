# VibeConnect

## Description

VibeConnect is a social networking platform built with C# and .NET 8.0. It provides a platform for users to connect and interact with each other in a secure and user-friendly environment. The application is designed to provide scalability and maintainability.

## Key Features

- **User Authentication and Authorization**: VibeConnect uses JWT for user authentication. It provides secure endpoints for user registration and login. The token service generates access and refresh tokens for authenticated users.

- **Friend Requests**: Users can send, receive, accept, or decline friend requests. The application ensures that users cannot send friend requests to themselves or to users who have already sent them a friend request.

- **Follow/Unfollow Users**: Users can follow or unfollow other users. The application updates the follower and following counts accordingly.

- **Posts**: Users can create, like, and comment on posts. The application provides endpoints for retrieving a user's posts and the posts of users they follow.

- **File Upload Service**: Users can upload profile pictures and post images. The application uses Cloudinary for storing and retrieving images.

- **Docker Support**: The application is Docker-ready for easy deployment. It includes a Dockerfile and a docker-compose.yml file for setting up the application and its dependencies in a Docker environment.

## Installation

1. Clone the repository: `git clone https://github.com/Evans-Prah/VibeConnect.git`
2. Navigate to the project directory: `cd VibeConnect`
3. Install the dependencies: `dotnet restore`
4. Build the project: `dotnet build`
5. Run the project: `dotnet run`

## Usage

After running the project, you can interact with the API using a tool like Postman or curl. The application provides a Swagger UI for testing the API endpoints.

## Docker Deployment

1. Build the Docker image: `docker build -t vibeconnect:2.2 .`
2. Run the Docker container: `docker-compose up`

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

[MIT](https://choosealicense.com/licenses/mit/)