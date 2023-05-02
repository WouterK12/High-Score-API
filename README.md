# High Score API

A simple REST API used to keep track of high scores, made in ASP.NET Core 6.0

## Usage

```
docker-compose up --build
```

## Endpoints

All endpoints require authentication using the `X-API-Key` header.  
Make sure to change the default allowed key `"YOUR-SECRET-CLIENT-KEY"` in `appsettings.json`.

| Method | Endpoint                            | Description                                      |
| ------ | ----------------------------------- | ------------------------------------------------ |
| GET    | `/api/highscores/top/{amount}`      | Get the top `amount` of highest scores           |
| GET    | `/api/highscores/search/{username}` | Get the high score of user by `username`         |
| POST   | `/api/highscores`                   | Add a new high score                             |
| DELETE | `/api/highscores`                   | Delete all high scores                           |
| DELETE | `/api/highscores/delete`            | Delete a specific high score (Same body as POST) |

### Adding a new high score

The `username` must be between `1` and `30` characters. Detected profanity will be removed from the name.  
The `score` must be `1` or higher.

Example `JSON`:

```json
{
  "username": "Epic-Gamer",
  "score": 490
}
```

### Deleting all high scores

The `DELETE` endpoint requires authentication using the `X-API-Key` header.  
The default allowed key `"YOUR-SECRET-ADMIN-KEY"` is different from the other endpoints and should be changed in `appsettings.json`.
