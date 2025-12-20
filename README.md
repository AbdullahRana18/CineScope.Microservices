This project is a microservices-based Movie Management System built using ASP.NET Core. It consists of three main components: AuthService, MovieService, and MovieWebUI, which work together to provide secure authentication, movie data management, and a user-friendly web interface.

The AuthService handles user registration, login, role-based access (Admin/User), and JWT token generation and validation. User credentials are securely stored using AES encryption, and JWT is used to authenticate and authorize requests between services.

The MovieService is responsible for fetching movie data from the TMDB API. It provides APIs for searching movies, retrieving movie details, and fetching multiple movies in parallel. The service uses in-memory caching to improve performance and includes a background worker to warm up frequently searched movie data. It also validates JWT tokens by communicating with the AuthService to ensure secure access.

The MovieWebUI is an ASP.NET Core MVC frontend that allows users to register, log in, search movies, view movie details, and manage sessions using JWT stored in server-side sessions. Admin users have access to an admin panel where they can view users and manage roles. The UI communicates with backend services using HttpClient and follows a clean, role-based navigation structure.

Overall, this project demonstrates a secure, scalable, and modular microservices architecture, integrating authentication, external APIs, caching, background processing, and a modern web UI into a single cohesive system.
