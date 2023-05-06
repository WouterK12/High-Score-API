# High Score API

A simple REST API used to keep track of high scores, made in ASP.NET Core 6.0

## Usage

```
docker-compose up --build
```

By default the API starts on `127.0.0.1:5205`.  
You can change this in [`docker-compose.yml`](./docker-compose.yml).

## Endpoints

All endpoints require authentication using the `X-API-Key` header.  
Make sure to change the default allowed client and admin keys in [`appsettings.json`](./HighScoreAPI/appsettings.json).

### **/api/highscores**

| Method | Endpoint                           | Description                              | `X-API-Key` |
| ------ | ---------------------------------- | ---------------------------------------- | ----------- |
| GET    | `/{projectName}/top/{amount}`      | Get the top `amount` of highest scores   | Client      |
| GET    | `/{projectName}/search/{username}` | Get the high score of user by `username` | Client      |
| POST   | `/{projectName}`                   | Add a new high score                     | Client      |
| DELETE | `/{projectName}`                   | Delete a high score (Same body as POST)  | Admin       |

**Adding a new high score**

The `username` must be between `1` and `64` characters. Detected profanity will be removed from the name.  
The `score` must be `1` or higher.

Example `JSON`:

```json
{
  "username": "Epic-Gamer",
  "score": 490
}
```

### **/api/projects**

| Method | Endpoint                | Description                                                   | `X-API-Key` |
| ------ | ----------------------- | ------------------------------------------------------------- | ----------- |
| GET    | `/search/{projectName}` | Get a project by `projectName`                                | Admin       |
| POST   | `/`                     | Add a new project                                             | Admin       |
| DELETE | `/{projectName}`        | Delete a project with all of its high scores by `projectName` | Admin       |

**Adding a new project**

The `name` must be between `1` and `64` characters. Whitespaces will be replaced by dashes.

Example `JSON`:

```json
{
  "name": "My-Game"
}
```
