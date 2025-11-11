# Departments API

This API provides CRUD operations for Department entities with data annotation validation.

## Endpoints

### GET /api/departments
**Description:** Get all departments  
**Response:** `200 OK` - Array of DepartmentDto

### GET /api/departments/{id}
**Description:** Get department by ID  
**Parameters:** `id` (int) - Department ID  
**Response:**
- `200 OK` - DepartmentDto
- `400 Bad Request` - Invalid ID
- `404 Not Found` - Department not found

### GET /api/departments/by-name/{name}
**Description:** Get department by name  
**Parameters:** `name` (string) - Department name  
**Response:**
- `200 OK` - DepartmentDto
- `400 Bad Request` - Invalid name
- `404 Not Found` - Department not found

### GET /api/departments/search?keyword={keyword}
**Description:** Search departments by name keyword  
**Parameters:** `keyword` (string) - Search keyword  
**Response:**
- `200 OK` - Array of DepartmentDto
- `400 Bad Request` - Invalid keyword

### POST /api/departments
**Description:** Create a new department  
**Request Body:** CreateDepartmentDto
```json
{
  "name": "string (required, 1-100 chars)",
  "description": "string (optional, max 500 chars)"
}
```
**Response:**
- `201 Created` - DepartmentDto
- `400 Bad Request` - Validation errors
- `409 Conflict` - Department name already exists

### PUT /api/departments/{id}
**Description:** Update an existing department  
**Parameters:** `id` (int) - Department ID  
**Request Body:** UpdateDepartmentDto
```json
{
  "name": "string (required, 1-100 chars)",
  "description": "string (optional, max 500 chars)"
}
```
**Response:**
- `200 OK` - DepartmentDto
- `400 Bad Request` - Validation errors
- `404 Not Found` - Department not found
- `409 Conflict` - Department name already exists

### DELETE /api/departments/{id}
**Description:** Delete a department  
**Parameters:** `id` (int) - Department ID  
**Response:**
- `204 No Content` - Successfully deleted
- `400 Bad Request` - Invalid ID
- `404 Not Found` - Department not found

## DTOs

### DepartmentDto
```json
{
  "id": 1,
  "name": "Human Resources",
  "description": "Manages employee relations"
}
```

### CreateDepartmentDto
```json
{
  "name": "Human Resources",          // Required, 1-100 characters
  "description": "Optional description" // Optional, max 500 characters
}
```

### UpdateDepartmentDto
```json
{
  "name": "Human Resources",          // Required, 1-100 characters
  "description": "Updated description" // Optional, max 500 characters
}
```

## Features
- ✅ Complete CRUD operations
- ✅ Data annotation validation
- ✅ Search functionality
- ✅ Comprehensive error handling
- ✅ Proper HTTP status codes
- ✅ Repository pattern integration
- ✅ Logging and error tracking