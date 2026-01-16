# Postman API Testing Setup

This directory contains the necessary files to test the Blog Platform API using Postman.

## Files

1.  **`postman_environment.json`**: Postman environment variables for the project.
2.  **`postman_collection.json`**: Postman collection containing all API requests.

## Setup Instructions

1.  **Import Environment**:
    - Open Postman.
    - Click on the **Import** button (top left).
    - Drag and drop `postman_environment.json` into the import window.
    - Once imported, select the **Project_Env_2026-01-16** environment from the environment dropdown (top right).

2.  **Import Collection**:
    - Click on the **Import** button again.
    - Drag and drop `postman_collection.json` into the import window.
    - The **Project_API_Tests** collection will appear in your Collections tab.

3.  **Configure Variables**:
    - Ensure the `baseUrl` in the environment is set to your running API's address (default is `http://localhost:5152`).
    - You may need to update `blogId` and `postId` variables as you create resources.

## Authentication Flow

1.  **Register**: Use the **Auth > Register** request to create a new user.
2.  **Login**: Use the **Auth > Login** request. This request has a **Test script** that automatically saves the `accessToken` and `refreshToken` to your environment.
3.  **Authenticated Requests**: All other requests in the **Auth** (Logout, Me, Revoke), **Blogs**, and **Posts** folders use the `{{accessToken}}` variable for Bearer authentication.

## Dynamic Variables

- `{{baseUrl}}`: The root URL of the API.
- `{{accessToken}}`: Automatically updated upon successful login/refresh.
- `{{refreshToken}}`: Automatically updated upon successful login/refresh.
- `{{blogId}}`: Used for blog-specific operations (e.g., `/api/blogs/{{blogId}}`).
- `{{postId}}`: Used for post-specific operations (e.g., `/api/posts/{{postId}}`).
