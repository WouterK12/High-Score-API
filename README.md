# High Score API

A simple REST API used to keep track of high scores, made in ASP.NET Core 6.0

## Usage

```
docker-compose up --build
```

## Endpoints

| method | endpoint                            | description                              |
| ------ | ----------------------------------- | ---------------------------------------- |
| GET    | `/api/highscores/top10`             | Get the top 10 of highest scores         |
| GET    | `/api/highscores/search/{username}` | Get the high score of user by `username` |
| POST   | `/api/highscores/add`               | Add a new high score                     |

### Adding a new high score

The `username` must be between `1` and `30` characters.  
The `score` must be `1` or higher.

Example `JSON`:

```json
{
  "username": "Epic-Gamer",
  "score": 490
}
```
