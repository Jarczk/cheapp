# Cheapp

Cheapp is a web application that aggregates deals, promotions, and discount codes from various stores. Users can compare prices to find the best deals and contribute their own promo codes or offers. This repository contains the backend (built with .NET), the frontend (built with React), and supporting documentation.

## Table of Contents
1. [Overview](#overview)
2. [Project Structure](#project-structure)
3. [Getting Started](#getting-started)
   - [Requirements](#requirements)
   - [Installation & Setup](#installation--setup)
4. [Running the App](#running-the-app)
5. [Testing](#testing)
6. [Contributors](#contributors)
7. [License](#license)

---

## Overview

- **Goal**: Provide a platform where users can quickly find the cheapest offers across different stores and easily share new discounts or promo codes.
- **Tech Stack**:
  - **Backend**: .NET (Web API)
  - **Frontend**: React
  - **Database**: MongoDB

This monorepo consolidates both the server and client code, along with documentation and any DevOps files (like GitHub Actions for CI/CD).

## Project Structure

```
cheapp/
├─ server/                 # Backend (ASP.NET Web API)
├─ tests/                  # Unit/Integration tests for the backend
├─ client/                 # Frontend (React)
├─ docs/                   # Additional documentation (diagrams, specs, etc.)
├─ .gitignore
└─ README.md               # You are here!
```

### Folders

- **client**: React application, generated with `create-react-app`.
- **server**: Main .NET Web API project.
- **tests**: Project containing backend tests.
- **docs**: Documentation resources and references.

## Getting Started

### Requirements

1. **.NET 8.0+**
2. **Node.js 14+** (or latest LTS)
3. **MongoDB** (local or remote instance)

### Installation & Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/Jarczk/cheapp.git
   cd cheapp
   ```

2. **.NET project setup**:
   ```bash
   cd server/Cheapp
   dotnet restore
   # Add any environment-specific settings in appsettings.json or user secrets
   cd ..
   ```

3. **React (client) setup**:
   ```bash
   cd client
   npm install
   # or: yarn install
   cd ..
   ```

4. **MongoDB connection**:
   - By default, your MongoDB connection string might be stored in `appsettings.json` or in environment variables.  
   - Make sure you update the connection string to point to your local/remote MongoDB instance.

## Running the App

Below are examples of how to run both the backend and frontend for local development.

### Start the Backend (ASP.NET Web API)
```bash
cd server/Cheapp
dotnet run
```
- The server will typically be available at `https://localhost:5001` or `http://localhost:5000` (depending on your .NET configuration).

### Start the Frontend (React)
```bash
cd client
npm start
```
- The React app usually runs on `http://localhost:3000`.  
- It may call the API endpoints at `http://localhost:5000` (configure in `client/src` or wherever your environment settings are).

## Testing

- **Backend tests** (xUnit, MSTest, or NUnit):
  ```bash
  cd tests
  dotnet test
  ```
- **Frontend tests** (Jest, React Testing Library, etc.):
  ```bash
  cd client
  npm test
  ```


## Contributors

 * [Jarczk](https://github.com/Jarczk)
 * [waldziorr](https://github.com/Waldziorr)
 * [sQnuf](https://github.com/sQnuf)


## License

This project is licensed under the [MIT License](https://en.wikipedia.org/wiki/MIT_License)
