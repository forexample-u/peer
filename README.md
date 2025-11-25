# Peer

How use it?

1. Get APP_KEY and APP_SECRET:
Create Dropbox app with select App Folder: https://dropbox.com/developers/apps/create
Get your APP_KEY and APP_SECRET, and set your SCOPE create.file, read.file

2. Get Refresh Token:
Copy url with set APP_KEY: https://dropbox.com/oauth2/authorize?client_id=<APP_KEY>&token_access_type=offline&response_type=code

Do post query to https://api.dropboxapi.com/oauth2/token with APP_KEY and APP_SECRET:
```pyton
import requests
data = f'code={ACCESS_CODE_GENERATED_FROM_OAUTH2}&grant_type=authorization_code'
response = requests.post('https://api.dropboxapi.com/oauth2/token', data=data, auth=(APP_KEY, APP_SECRET))
print(json.dumps(json.loads(response.text), indent=2))
```