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

| Method | Endpoint                           | Description                                                           | `X-API-Key` |
| ------ | ---------------------------------- | --------------------------------------------------------------------- | ----------- |
| GET    | `/{projectName}/top/{amount}`      | Get the top `amount` of highest scores                                | Client      |
| GET    | `/{projectName}/search/{username}` | Get the high score of user by `username`                              | Client      |
| POST   | `/{projectName}`                   | Add a new high score ([**Encrypted body**](#adding-a-new-high-score)) | Client      |
| DELETE | `/{projectName}`                   | Delete a high score                                                   | Admin       |

### **/api/projects**

| Method | Endpoint                | Description                                            | `X-API-Key` |
| ------ | ----------------------- | ------------------------------------------------------ | ----------- |
| GET    | `/search/{projectName}` | Get a project by `projectName`                         | Admin       |
| POST   | `/`                     | Add a new project                                      | Admin       |
| DELETE | `/{projectName}`        | Delete a project with all high scores by `projectName` | Admin       |

### **/api/users**

| Method | Endpoint                | Description                                         | `X-API-Key` |
| ------ | ----------------------- | --------------------------------------------------- | ----------- |
| GET    | `/{projectName}/random` | Get a random username that has not set a high score | Client      |

## Adding a new high score

### Disable encryption

By default, the API requests you to send encrypted high scores.  
If you want to disable this behaviour, remove this line in [`Program.cs`](./HighScoreAPI/Program.cs):

```cs
app.UseMiddleware<EncryptionMiddleware>();
```

### Generating an encryption key

The first step is to create a new project. Send a POST request to `/api/projects`.  
The `name` must be between `1` and `64` characters. Whitespaces will be replaced by dashes.

Example `JSON`:

```json
{
  "name": "My-Game"
}
```

The API will respond with an encryption key. You can use this key to encrypt a high score.

Example `JSON`:

```json
{
  "name": "My-Game",
  "aesKeyBase64": "HbrBX/TckMpgKFKZbLQsJkkfE3bUKJ1JuD2CPZbxt48="
}
```

### Encrypting a high score using AES

In [AesOperation.cs](./HighScoreAPI/Encryption/AesOperation.cs), you can see how to encrypt the high score.  
Use this implementation on the client to encrypt the high score.

```cs
public static string EncryptData(string plainText, string keyBase64, out string vectorBase64)
{
    using Aes aes = Aes.Create();
    aes.Key = Convert.FromBase64String(keyBase64);
    aes.GenerateIV();

    vectorBase64 = Convert.ToBase64String(aes.IV);

    ICryptoTransform encryptor = aes.CreateEncryptor();

    using MemoryStream ms = new();
    using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
    using StreamWriter sw = new(cs);
    sw.Write(plainText);
    sw.Close();

    byte[] encryptedData = ms.ToArray();

    return Convert.ToBase64String(encryptedData);
}
```

```cs
// Generated key associated with project
const string AesKeyBase64 = "HbrBX/TckMpgKFKZbLQsJkkfE3bUKJ1JuD2CPZbxt48=";

// The username must be between 1 and 64 characters. Detected profanity will be removed from the name.
// The score must be 1 or higher.
var highScore = new HighScore() { Username = "user", Score = 10 };
string jsonString = JsonConvert.SerializeObject(highScore);

// Encrypt the JSON string to receive cipherText and vectorBase64
string cipherText = EncryptData(jsonString, AesKeyBase64, out string vectorBase64);
```

### Posting the high score to the API

Now that you have an encrypted high score and a `vectorBase64`, you can POST the `cipherText` to `/api/highscores/{projectName}`.

Add an extra header called `AES-Operation` with the value of `vectorBase64`.  
The API uses this to decrypt the body of the request.

### Done!

That's it, your high score should now be added to the database!  
Try fetching `/api/highscores/{projectName}/top/10` to see it in the list.
