# Registration & Login API (ASP.NET Core)
Try it out (SPA Front-end): https://calm-cliff-059303d03.1.azurestaticapps.net/

## Overview

This project is a backend API demonstrating:

- User registration and login
- JWT authentication with access and refresh tokens
- Role-based authorization
- EF Core in-memory database for persistence
- Proper CORS configuration for multi-client access
- Clean architecture separating API client, services, and controllers

---

## Features

- **Authentication**
  - `POST /Auth/Register` — create a new user, returns access + refresh tokens
  - `POST /Auth/Login` — login with credentials, returns access + refresh tokens
  - `POST /Auth/Refresh` — obtain a new access token using a refresh token

- **Protected Endpoints**
  - `GET /Main/Test` — accessible by any authenticated user
  - `POST /Main/AssignAdmin` — assign admin role to current user (requires authentication)
  - `GET /Main/AdminTest` — accessible only by users with admin role

- **JWT**
  - Stateless authentication
  - Short-lived access tokens, long-lived refresh tokens
  - Claims include username, role, and user ID
  - Frontend can decode JWT for UI purposes but server enforces all authorization

- **CORS**
  - Configured dynamically from `appsettings.json`
  - Allows frontend app origin(s) to access the API
  - Supports credentials (cookies) and all standard HTTP methods/headers

---

## Tech Stack

- ASP.NET Core Web API
- Entity Framework Core (In-Memory)
- JWT Bearer Authentication
- CORS
- Swagger / OpenAPI for API testing

